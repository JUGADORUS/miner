using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const float WorkbenchRepairFraction = 0.2f;
    private const float DynamiteFuse = 3f;
    private const int DynamiteRadius = 5;
    private const int DroppedDynamiteRadius = 3;
    private const int LightningLength = 10;
    private const float LuckDuration = 10f;
    private const float LuckBonus = 10f;
    private const int CriticalDiggingRangeBonus = 2;
    internal const int DashHitsRequired = 3;
    private const float DashSpeed = 12f;
    internal const float DefaultCriticalDamageMultiplier = 1.5f;
    internal const int RunRegenerationBonus = 1;
    internal const float RunCriticalChanceBonus = 0.05f;
    internal const float RunCriticalDamageBonus = 0.25f;

    public static GameManager Instance { get; private set; }

    [SerializeField] private Player _player;
    [SerializeField] private Pickaxe _pickaxe;
    [SerializeField] private Material _lightningMaterial;
    [SerializeField] private SpriteRenderer _dynamiteSprite;

    private ChunkGenerator _foreground;
    private LineRenderer _lightning;
    private float _lightningChance;
    private float _dynamiteChance;
    private float _luckChance;
    private float _luckUntil;
    private Coroutine _hideLightning;
    private int _armedCharges;
    private bool _pickaxeBroken;
    private bool _sessionEnded;
    private int _regenerationAmount;
    private float _regenerationProgress;
    private float _baseCriticalChance;
    private float _criticalChance;
    private float _criticalDamageMultiplier = DefaultCriticalDamageMultiplier;
    private Block _lastBossHit;
    private double _lastBossHitTime;
    private Rigidbody2D _pickaxeBody;
    private double _lastDashChargeTime = double.NegativeInfinity;

    public int RewardPerSession { get; private set; }
    public int BlocksDestroyed { get; private set; }
    public float LuckTimeLeft => Mathf.Max(0f, _luckUntil - Time.time);
    public int RunCredits { get; private set; }
    internal int DashChargeHits { get; private set; } = DashHitsRequired;
    public bool CanBuyRunUpgrades => !_pickaxeBroken && _pickaxe.Durability > 0;
    public event System.Action RunShopChanged;

    private int _runDamageLevel;
    private int _runCapacityLevel;
    private int _runRepairs;
    private int _runRegenerationLevel;
    private int _runCriticalChanceLevel;
    private int _runCriticalDamageLevel;

    public enum RunUpgrade
    {
        Damage,
        Capacity,
        Repair,
        Regeneration,
        CriticalChance,
        CriticalDamage
    }

    public int GetRunUpgradeCost(RunUpgrade upgrade)
    {
        return upgrade switch
        {
            RunUpgrade.Damage => 6 + 4 * _runDamageLevel,
            RunUpgrade.Capacity => 8 + 5 * _runCapacityLevel,
            RunUpgrade.Repair => 5 + 3 * _runRepairs,
            RunUpgrade.Regeneration => 8 + 5 * _runRegenerationLevel,
            RunUpgrade.CriticalChance => 8 + 5 * _runCriticalChanceLevel,
            RunUpgrade.CriticalDamage => 8 + 5 * _runCriticalDamageLevel,
            _ => int.MaxValue
        };
    }

    public bool CanBuyRunUpgrade(RunUpgrade upgrade)
    {
        return CanBuyRunUpgrades && RunCredits >= GetRunUpgradeCost(upgrade) &&
            (upgrade != RunUpgrade.Repair || _pickaxe.Durability < _pickaxe.MaxDurability) &&
            (upgrade != RunUpgrade.CriticalChance || _criticalChance < 1f);
    }

    public bool TryBuyRunUpgrade(RunUpgrade upgrade)
    {
        if (!CanBuyRunUpgrade(upgrade)) return false;

        RunCredits -= GetRunUpgradeCost(upgrade);
        switch (upgrade)
        {
            case RunUpgrade.Damage:
                _runDamageLevel++;
                _pickaxe.Damage++;
                break;
            case RunUpgrade.Capacity:
                _runCapacityLevel++;
                _pickaxe.MaxDurability += 8;
                _pickaxe.Durability += 8;
                break;
            case RunUpgrade.Repair:
                _runRepairs++;
                _pickaxe.Durability = Mathf.Min(_pickaxe.MaxDurability, _pickaxe.Durability + 12);
                break;
            case RunUpgrade.Regeneration:
                _runRegenerationLevel++;
                _regenerationAmount += RunRegenerationBonus;
                break;
            case RunUpgrade.CriticalChance:
                _runCriticalChanceLevel++;
                _criticalChance = Mathf.Clamp01(_baseCriticalChance + _runCriticalChanceLevel * RunCriticalChanceBonus);
                break;
            case RunUpgrade.CriticalDamage:
                _runCriticalDamageLevel++;
                _criticalDamageMultiplier += RunCriticalDamageBonus;
                break;
        }

        GameEvents.OnDurabilityChanged?.Invoke(_pickaxe.Durability, _pickaxe.MaxDurability);
        RunShopChanged?.Invoke();
        return true;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        _foreground = GetComponent<ChunkGenerator>();
        _pickaxeBody = _pickaxe.GetComponent<Rigidbody2D>();
        GameEvents.OnHitBlockWithDirectionPosition += HandleHitBlock;
        GameEvents.OnBlockDestroyed += HandleBlockDestroyed;
        GameEvents.OnPickaxeBroken += HandlePickaxeBroken;
    }

    private void OnDestroy()
    {
        GameEvents.OnHitBlockWithDirectionPosition -= HandleHitBlock;
        GameEvents.OnBlockDestroyed -= HandleBlockDestroyed;
        GameEvents.OnPickaxeBroken -= HandlePickaxeBroken;

        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        if (_pickaxe.data == null) return;

        int id = _pickaxe.data.ID;
        var upgrades = UpgradesManager.Instance;
        if (upgrades != null)
        {
            _regenerationAmount = upgrades.GetRegenerationBonus(id);
            _baseCriticalChance = upgrades.GetCriticalChance(id);
            _criticalChance = _baseCriticalChance;
            _criticalDamageMultiplier = upgrades.GetCriticalDamageMultiplier(id);
        }

        _lightningChance = PickaxeEnchantments.GetHitChance(EnchantmentKind.Lightning,
            PickaxeEnchantments.GetLevel(id, EnchantmentKind.Lightning));
        _dynamiteChance = PickaxeEnchantments.GetHitChance(EnchantmentKind.Dynamite,
            PickaxeEnchantments.GetLevel(id, EnchantmentKind.Dynamite));
        _luckChance = PickaxeEnchantments.GetHitChance(EnchantmentKind.Luck,
            PickaxeEnchantments.GetLevel(id, EnchantmentKind.Luck));
    }

    private void Update()
    {
        UpdateRegeneration(Time.deltaTime);
    }

    private void UpdateRegeneration(float deltaTime)
    {
        if (_regenerationAmount <= 0 || !CanBuyRunUpgrades || _pickaxe.Durability >= _pickaxe.MaxDurability)
        {
            _regenerationProgress = 0f;
            return;
        }

        _regenerationProgress += deltaTime * _regenerationAmount;
        if (_regenerationProgress < 1f) return;

        int missingDurability = _pickaxe.MaxDurability - _pickaxe.Durability;
        int restored = Mathf.Min(Mathf.FloorToInt(_regenerationProgress), missingDurability);
        _regenerationProgress = restored == missingDurability ? 0f : _regenerationProgress - restored;
        _pickaxe.Durability += restored;
        GameEvents.OnDurabilityChanged?.Invoke(_pickaxe.Durability, _pickaxe.MaxDurability);
        for (int tick = 0; tick < restored; tick++)
            GameEvents.OnDurabilityRegenerated?.Invoke(1);
    }

    private void HandleHitBlock(Block block, Vector2 contactPoint, Vector2 normal)
    {
        if (block == null || block.IsDestroyed || _pickaxe.Durability <= 0) return;
        double hitTime = Time.fixedTimeAsDouble;
        if (block.IsBossWall)
        {
            // The head and handle can contact the same wall in one physics step.
            if (_lastBossHit == block && _lastBossHitTime == hitTime) return;

            _lastBossHit = block;
            _lastBossHitTime = hitTime;
        }

        Vector2 hitCenter = block.transform.position;
        _pickaxe.Durability = Mathf.Max(0, _pickaxe.Durability - block.GetHitWear());
        bool lightningTriggered = RollEnchantment(_lightningChance);
        bool dynamiteTriggered = RollEnchantment(_dynamiteChance);
        if (RollEnchantment(_luckChance))
            _luckUntil = Time.time + LuckDuration;

        DamageBlocks(block, contactPoint, normal);
        if (dynamiteTriggered)
            ArmDynamite(_dynamiteSprite, _pickaxe.transform.position + (Vector3)normal * 0.4f, DroppedDynamiteRadius, true);
        if (lightningTriggered)
            StrikeLightning(_foreground.WorldToBlock(hitCenter));

        GameEvents.OnDurabilityChanged?.Invoke(_pickaxe.Durability, _pickaxe.MaxDurability);

        if (_pickaxe.Durability == 0)
        {
            GameEvents.OnPickaxeBroken?.Invoke();
        }
        else
        {
            ChargeDash(hitTime);
        }
    }

    private void ChargeDash(double hitTime)
    {
        if (DashChargeHits < DashHitsRequired && _lastDashChargeTime != hitTime)
        {
            // Multiple contacts in the same physics step count as one impact.
            _lastDashChargeTime = hitTime;
            DashChargeHits++;
        }
    }

    internal bool TryDash(Vector2 target)
    {
        if (DashChargeHits < DashHitsRequired || Time.timeScale <= 0f || !CanBuyRunUpgrades ||
            _pickaxeBody == null || _pickaxeBody.bodyType != RigidbodyType2D.Dynamic)
            return false;

        Vector2 direction = target - _pickaxeBody.worldCenterOfMass;
        if (direction.sqrMagnitude < 0.0001f) return false;

        DashChargeHits = 0;
        _pickaxeBody.linearVelocity = direction.normalized * DashSpeed;
        return true;
    }

    private bool RollEnchantment(float chance) =>
        _foreground != null && chance > 0f && Random.value * 100f < chance;

    private void StrikeLightning(Vector2Int cell)
    {
        float angle = Random.Range(-180f, 0f) * Mathf.Deg2Rad;
        var direction = new Vector2(Mathf.Cos(angle), Mathf.Min(0f, Mathf.Sin(angle)));
        Vector2 travel = direction * LightningLength;
        if (_lightning == null)
        {
            var effect = new GameObject("Lightning", typeof(LineRenderer));
            effect.transform.SetParent(transform, false);
            _lightning = effect.GetComponent<LineRenderer>();
            _lightning.sharedMaterial = _lightningMaterial;
            _lightning.sortingLayerName = "Mine_Playground";
            _lightning.sortingOrder = 10;
            _lightning.widthMultiplier = _foreground.blockSize * 0.14f;
            _lightning.startColor = new Color(0.85f, 1f, 1f);
            _lightning.endColor = new Color(0.45f, 0.65f, 1f);
            _lightning.positionCount = LightningLength + 1;
        }

        Vector3 origin = _foreground.BlockToWorld(cell);
        var perpendicular = new Vector3(-direction.y, direction.x);
        for (int step = 0; step <= LightningLength; step++)
        {
            Vector3 point = origin + (Vector3)direction * (step * _foreground.blockSize);
            if (step > 0 && step < LightningLength)
                point += perpendicular * ((step % 2 == 0 ? 0.22f : -0.22f) * _foreground.blockSize);

            _lightning.SetPosition(step, point);
        }
        _foreground.BreakLine(cell, travel);

        _lightning.enabled = true;
        if (_hideLightning != null) StopCoroutine(_hideLightning);
        _hideLightning = StartCoroutine(HideLightning());
    }

    private IEnumerator HideLightning()
    {
        yield return new WaitForSeconds(0.2f);
        _lightning.enabled = false;
        _hideLightning = null;
    }

    private void DamageBlocks(Block hitBlock, Vector2 contactPoint, Vector2 normal)
    {
        Vector2 hitCenter = hitBlock.transform.position;
        int damage = _pickaxe.Damage;
        int range = _pickaxe.RangeOfDigging;
        bool critical = _criticalChance > 0f && (_criticalChance >= 1f || Random.value < _criticalChance);
        if (critical)
        {
            damage = (int)System.Math.Min(int.MaxValue,
                System.Math.Round(damage * (double)_criticalDamageMultiplier, System.MidpointRounding.AwayFromZero));
            range += CriticalDiggingRangeBonus;
        }

        hitBlock.TakeDamage(damage);
        if (critical) GameEvents.OnCriticalHit?.Invoke(contactPoint, damage);

        if (range <= 1 || _foreground == null) return;

        Vector2Int hitCell = _foreground.WorldToBlock(hitCenter);
        int halfRange = (range - 1) / 2;
        Vector2Int start = hitCell - new Vector2Int(halfRange, halfRange);

        if (range % 2 == 0)
        {
            // On the impact axis, put the extra cell into the struck surface.
            Vector2 bias = contactPoint - hitCenter;
            if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y))
                bias.x = -normal.x;
            else if (normal.y != 0f)
                bias.y = -normal.y;

            if (bias.x < 0f) start.x--;
            if (bias.y < 0f) start.y--;
        }

        for (int x = 0; x < range; x++)
        {
            for (int y = 0; y < range; y++)
            {
                Vector2Int cell = start + new Vector2Int(x, y);
                if (cell == hitCell) continue;

                Block block = _foreground.GetBlockAt(cell);
                if (block != null) block.TakeDamage(damage);
            }
        }
    }

    private void HandleBlockDestroyed(Block block, Vector2 position)
    {
        if (_sessionEnded) return;

        BlocksDestroyed++;
        if (block.IsDynamite)
        {
            ArmDynamite(block.GetComponent<SpriteRenderer>(), position, DynamiteRadius, false);
            return;
        }

        if (block.RepairsPickaxe)
        {
            if (_pickaxeBroken) return;
            int repairAmount = Mathf.CeilToInt(_pickaxe.MaxDurability * WorkbenchRepairFraction);
            _pickaxe.Durability = Mathf.Min(_pickaxe.MaxDurability, _pickaxe.Durability + repairAmount);
            return;
        }

        int baseReward = block.DetermineReward();
        float multiplier = _pickaxe.RewardMultiplyer + (LuckTimeLeft > 0f ? LuckBonus : 0f);
        int reward = Mathf.RoundToInt(baseReward * multiplier);

        RewardPerSession += reward;
        RunCredits += baseReward;
        RunShopChanged?.Invoke();
        if (reward > 0) GameEvents.OnBlockRewarded?.Invoke(position, reward);
    }

    private void ArmDynamite(SpriteRenderer source, Vector2 position, int radius, bool falling)
    {
        if (source == null) return;

        // The mined block returns to its pool independently of the fuse.
        var charge = new GameObject("Armed Dynamite");
        charge.transform.SetParent(transform, false);
        charge.transform.position = position;
        var visual = new GameObject("Dynamite Sprite", typeof(SpriteRenderer));
        visual.transform.SetParent(charge.transform, false);
        SpriteRenderer sprite = visual.GetComponent<SpriteRenderer>();
        sprite.sprite = source.sprite;
        sprite.sharedMaterial = source.sharedMaterial;
        sprite.sortingLayerID = source.sortingLayerID;
        sprite.sortingOrder = source.sortingOrder + 1;
        sprite.transform.localScale = source.transform.localScale;
        if (falling)
        {
            sprite.transform.localScale *= 0.65f;
            var body = charge.AddComponent<Rigidbody2D>();
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            var collider = charge.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one * (_foreground.blockSize * 0.55f);
            foreach (Collider2D axeCollider in _pickaxe.GetComponentsInChildren<Collider2D>())
                Physics2D.IgnoreCollision(collider, axeCollider);

            body.linearVelocity = new Vector2(Random.Range(-1.3f, 1.3f), 2f);
            body.angularVelocity = Random.Range(-100f, 100f);
        }

        _armedCharges++;
        StartCoroutine(Detonate(charge.transform, sprite, radius));
    }

    private IEnumerator Detonate(Transform charge, SpriteRenderer sprite, int radius)
    {
        Vector3 scale = sprite.transform.localScale;
        Color warning = new Color(1f, 0.5f, 0.25f);
        float elapsed = 0f;
        while (elapsed < DynamiteFuse)
        {
            float progress = elapsed / DynamiteFuse;
            float pulse = 0.5f - 0.5f * Mathf.Cos(Mathf.PI * 2f * (progress + 5f * progress * progress));
            sprite.transform.localScale = scale * (1f + 0.18f * pulse);
            sprite.color = Color.Lerp(Color.white, warning, pulse);
            yield return null;
            elapsed += Time.deltaTime;
        }

        Vector2 position = charge.position;
        charge.gameObject.SetActive(false);
        Destroy(charge.gameObject);
        if (ParticleManager.Instance != null)
            ParticleManager.Instance.SpawnBlockBreakEffects(position);
        int durabilityBeforeBlast = _pickaxe.Durability;
        _foreground.Blast(_foreground.WorldToBlock(position), radius);
        if (_pickaxe.Durability != durabilityBeforeBlast)
            GameEvents.OnDurabilityChanged?.Invoke(_pickaxe.Durability, _pickaxe.MaxDurability);
        _armedCharges--;
        TryEndSession();
    }

    private void HandlePickaxeBroken()
    {
        _pickaxeBroken = true;
        RunShopChanged?.Invoke();
        TryEndSession();
    }

    private void TryEndSession()
    {
        if (_sessionEnded || !_pickaxeBroken || _armedCharges != 0) return;

        _sessionEnded = true;
        if (RewardPerSession > 0) _player.GetReward(RewardPerSession);
        GameEvents.OnSessionEnded?.Invoke(RewardPerSession);
    }
}

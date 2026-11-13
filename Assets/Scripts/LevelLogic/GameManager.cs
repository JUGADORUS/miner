using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const float WorkbenchRepairFraction = 0.2f;
    private const float DynamiteFuse = 3f;
    private const int DynamiteRadius = 5;

    public static GameManager Instance { get; private set; }

    [SerializeField] private Player _player;
    [SerializeField] private Pickaxe _pickaxe;

    private ChunkGenerator _foreground;
    private int _armedCharges;
    private bool _pickaxeBroken;

    public int RewardPerSession { get; private set; }
    public int BlocksDestroyed { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        _foreground = GetComponent<ChunkGenerator>();
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

    private void HandleHitBlock(Block block, Vector2 contactPoint, Vector2 normal)
    {
        if (block == null || block.IsDestroyed || _pickaxe.Durability <= 0) return;

        _pickaxe.Durability = Mathf.Max(0, _pickaxe.Durability - block.DamageToPickaxe);
        DamageBlocks(block, contactPoint, normal);

        GameEvents.OnDurabilityChanged?.Invoke(_pickaxe.Durability, _pickaxe.MaxDurability);

        if (_pickaxe.Durability == 0)
        {
            GameEvents.OnPickaxeBroken?.Invoke();
        }
    }

    private void DamageBlocks(Block hitBlock, Vector2 contactPoint, Vector2 normal)
    {
        Vector2 hitCenter = hitBlock.transform.position;
        hitBlock.TakeDamage(_pickaxe.Damage);

        int range = _pickaxe.RangeOfDigging;
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
                if (block != null) block.TakeDamage(_pickaxe.Damage);
            }
        }
    }

    private void HandleBlockDestroyed(Block block, Vector2 position)
    {
        BlocksDestroyed++;
        if (block.IsDynamite)
        {
            _armedCharges++;
            StartCoroutine(Detonate(block.GetComponent<SpriteRenderer>(), position));
            return;
        }

        if (block.RepairsPickaxe)
        {
            if (_pickaxeBroken) return;
            int repairAmount = Mathf.CeilToInt(_pickaxe.MaxDurability * WorkbenchRepairFraction);
            _pickaxe.Durability = Mathf.Min(_pickaxe.MaxDurability, _pickaxe.Durability + repairAmount);
            return;
        }

        int reward = Mathf.RoundToInt(block.DetermineReward() * _pickaxe.RewardMultiplyer);

        RewardPerSession += reward;
        _player.GetReward(reward);
        if (reward > 0) GameEvents.OnBlockRewarded?.Invoke(position, reward);
    }

    private IEnumerator Detonate(SpriteRenderer source, Vector2 position)
    {
        Vector2Int cell = _foreground.WorldToBlock(position);
        // The mined block returns to its pool independently of the fuse.
        var charge = new GameObject("Armed Dynamite", typeof(SpriteRenderer));
        charge.transform.SetParent(transform, false);
        charge.transform.position = position;
        SpriteRenderer sprite = charge.GetComponent<SpriteRenderer>();
        sprite.sprite = source.sprite;
        sprite.sharedMaterial = source.sharedMaterial;
        sprite.sortingLayerID = source.sortingLayerID;
        sprite.sortingOrder = source.sortingOrder + 1;
        Vector3 scale = source.transform.localScale;
        Color warning = new Color(1f, 0.5f, 0.25f);
        float elapsed = 0f;
        while (elapsed < DynamiteFuse)
        {
            float progress = elapsed / DynamiteFuse;
            float pulse = 0.5f - 0.5f * Mathf.Cos(Mathf.PI * 2f * (progress + 5f * progress * progress));
            charge.transform.localScale = scale * (1f + 0.18f * pulse);
            sprite.color = Color.Lerp(Color.white, warning, pulse);
            yield return null;
            elapsed += Time.deltaTime;
        }

        Destroy(charge);
        if (ParticleManager.Instance != null)
            ParticleManager.Instance.SpawnBlockBreakEffects(position);
        int durabilityBeforeBlast = _pickaxe.Durability;
        _foreground.Blast(cell, DynamiteRadius);
        if (_pickaxe.Durability != durabilityBeforeBlast)
            GameEvents.OnDurabilityChanged?.Invoke(_pickaxe.Durability, _pickaxe.MaxDurability);
        _armedCharges--;
        TryEndSession();
    }

    private void HandlePickaxeBroken()
    {
        _pickaxeBroken = true;
        TryEndSession();
    }

    private void TryEndSession()
    {
        if (_pickaxeBroken && _armedCharges == 0)
            GameEvents.OnSessionEnded?.Invoke(RewardPerSession);
    }
}

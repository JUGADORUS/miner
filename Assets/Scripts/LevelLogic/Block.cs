using UnityEngine;

public class Block : MonoBehaviour
{
    private const int BossHealthBonus = 150;

    [SerializeField] public int Health;
    [SerializeField] private int MaxHealth;
    [SerializeField] private int MinReward;
    [SerializeField] private int MaxReward;
    [SerializeField] public int DamageToPickaxe;
    [SerializeField] public Destruction destruction;

    [SerializeField] private bool _repairsPickaxe;
    [SerializeField] private bool _isDynamite;
    [SerializeField] private bool _isBossWall;
    [SerializeField] private bool _isIndestructible;
    [SerializeField, Range(0, 100)] private int _hitWearPercent;

    public bool IsDestroyed { get; private set; }
    public int MaximumHealth => MaxHealth;
    public bool RepairsPickaxe => _repairsPickaxe;
    public bool IsDynamite => _isDynamite;
    public bool IsBossWall => _isBossWall;
    public bool IsIndestructible => _isIndestructible;
    private double _lastHitTime = double.NegativeInfinity;
    private int _bossHits;
    private bool _pooled;

    private void Awake()
    {
        Health = MaxHealth;
    }

    internal void PrepareForSpawn(Block prefab)
    {
        _pooled = true;
        IsDestroyed = false;
        _lastHitTime = double.NegativeInfinity;
        MaxHealth = prefab.MaxHealth;
        MinReward = prefab.MinReward;
        MaxReward = prefab.MaxReward;
        DamageToPickaxe = prefab.DamageToPickaxe;
        Health = MaxHealth;
    }

    internal void RestoreHealth(int? savedHealth)
    {
        Health = savedHealth ?? MaxHealth;
        UpdateDamageSprite();
    }

    internal bool TryRegisterHit(double hitTime)
    {
        if (_lastHitTime == hitTime) return false;

        _lastHitTime = hitTime;
        return true;
    }

    internal int GetHitWear(int maxDurability)
    {
        if (_hitWearPercent > 0)
            return (int)(((long)maxDurability * _hitWearPercent + 99) / 100);

        if (!IsBossWall) return DamageToPickaxe;

        double wear = DamageToPickaxe * System.Math.Pow(1.1, _bossHits++);
        return (int)System.Math.Min(int.MaxValue, System.Math.Round(wear));
    }

    public void ApplyDepthBonuses(int healthBonus, int wearBonus)
    {
        if (RepairsPickaxe || IsDynamite || IsIndestructible) return;

        MaxHealth += healthBonus;
        Health = MaxHealth;
        DamageToPickaxe += wearBonus;
    }

    internal void ScaleBossWall(int tier)
    {
        double scaledHealth = MaxHealth * System.Math.Pow(2, tier - 1) + BossHealthBonus;
        MaxHealth = (int)System.Math.Min(int.MaxValue, scaledHealth);
        Health = MaxHealth;
        DamageToPickaxe += tier - 1;
        MinReward *= tier + 1;
        MaxReward *= tier + 1;
    }

    public void TakeDamage(int damage)
    {
        if (IsDestroyed || IsIndestructible) return;

        Health -= damage;
        IsDestroyed = Health <= 0;

        UpdateDamageSprite();

        if (IsDestroyed)
        {
            try
            {
                GameEvents.OnBlockDestroyed?.Invoke(this, transform.position);
            }
            finally
            {
                if (_pooled) gameObject.SetActive(false);
                else Destroy(gameObject);
            }
        }
    }

    private void UpdateDamageSprite()
    {
        if (destruction != null)
            destruction.UpdateSprite((float)Health / MaxHealth * 100f);
    }

    public int DetermineReward()
    {
        return Random.Range(MinReward, MaxReward);
    }
}

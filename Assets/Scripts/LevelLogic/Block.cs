using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] public int Health;
    [SerializeField] private int MaxHealth;
    [SerializeField] private int MinReward;
    [SerializeField] private int MaxReward;
    [SerializeField] public int DamageToPickaxe;
    [SerializeField] public Destruction destruction;

    [SerializeField] private bool _repairsPickaxe;
    [SerializeField] private bool _isDynamite;

    public bool IsDestroyed { get; private set; }
    public bool RepairsPickaxe => _repairsPickaxe;
    public bool IsDynamite => _isDynamite;
    private bool _pooled;

    private void Awake()
    {
        Health = MaxHealth;
    }

    internal void PrepareForSpawn(Block prefab)
    {
        _pooled = true;
        IsDestroyed = false;
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

    public void ApplyDepthBonuses(int healthBonus, int wearBonus, int rewardBonus)
    {
        if (RepairsPickaxe || IsDynamite) return;

        MaxHealth += healthBonus;
        Health = MaxHealth;
        DamageToPickaxe += wearBonus;
        MinReward += rewardBonus;
        MaxReward += rewardBonus;
    }

    public void TakeDamage(int damage)
    {
        if (IsDestroyed) return;

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

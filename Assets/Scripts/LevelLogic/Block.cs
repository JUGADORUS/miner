using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] public int Health;
    [SerializeField] private int MaxHealth;
    [SerializeField] private int MinReward;
    [SerializeField] private int MaxReward;
    [SerializeField] public int DamageToPickaxe;
    [SerializeField] public Destruction destruction;

    public bool IsDestroyed { get; private set; }

    private void Awake()
    {
        Health = MaxHealth;
    }

    public void ApplyDepthBonuses(int healthBonus, int wearBonus, int rewardBonus)
    {
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

        float healthPercent = (float)Health / MaxHealth * 100f;
        destruction.UpdateSprite(healthPercent);

        if (IsDestroyed)
        {
            GameEvents.OnBlockDestroyed?.Invoke(this, transform.position);
            Destroy(gameObject);
        }
    }

    public int DetermineReward()
    {
        return Random.Range(MinReward, MaxReward);
    }
}

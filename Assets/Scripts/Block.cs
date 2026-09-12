using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] public int Health;
    [SerializeField] private int MaxHealth;
    [SerializeField] private int MinReward;
    [SerializeField] private int MaxReward;
    [SerializeField] public int DamageToPickaxe;
    [SerializeField] public Destruction destruction;

    private void Awake()
    {
        Health = MaxHealth;
    }

    public void TakeDamage(int damage)
    {
        Health = Health - damage;

        float healthPercent = (float)Health / MaxHealth * 100f;
        destruction.UpdateSprite(healthPercent);

        if (Health <= 0)
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

using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player player;
    public Pickaxe pickaxe;

    void Awake()
    {
        GameEvents.OnHitBlock += HandleHitBlock;
        GameEvents.OnBlockDestroyed += HandleBlockDestroyed;
        GameEvents.OnPickaxeBroken += HandlePickaxeBroken;
    }

    void HandleHitBlock(Block block, int pickaxeDurability)
    {
        block.TakeDamage(pickaxe.Damage);
        pickaxe.Durability = pickaxe.Durability - block.DamageToPickaxe;

        if (pickaxe.Durability <= 0)
        {
            GameEvents.OnPickaxeBroken?.Invoke();
        }

        Debug.Log($"ХП блока: {block.Health}\nХП кирки: {pickaxe.Durability}");
        GameEvents.OnDurabilityChanged?.Invoke(pickaxe.Durability, pickaxe.MaxDurability);
    }

    void HandleBlockDestroyed(Block block, Vector2 position)
    {
        int reward = block.DetermineReward();
        player.GetReward(reward);
        Debug.Log($"Деньги игрока: { player.Money}");
    }

    void HandlePickaxeBroken()
    {
        Debug.Log("Кирка сломалась!");
    }

    void OnDestroy()
    {
        GameEvents.OnHitBlock -= HandleHitBlock;
        GameEvents.OnBlockDestroyed -= HandleBlockDestroyed;
        GameEvents.OnPickaxeBroken -= HandlePickaxeBroken;
    }
}
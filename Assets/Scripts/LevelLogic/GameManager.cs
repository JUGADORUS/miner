using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Player _player;
    [SerializeField] private Pickaxe _pickaxe;

    public int RewardPerSession { get; private set; }
    public int BlocksDestroyed { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        GameEvents.OnHitBlock += HandleHitBlock;
        GameEvents.OnBlockDestroyed += HandleBlockDestroyed;
        GameEvents.OnPickaxeBroken += HandlePickaxeBroken;
    }

    private void OnDestroy()
    {
        GameEvents.OnHitBlock -= HandleHitBlock;
        GameEvents.OnBlockDestroyed -= HandleBlockDestroyed;
        GameEvents.OnPickaxeBroken -= HandlePickaxeBroken;

        if (Instance == this) Instance = null;
    }

    private void HandleHitBlock(Block block, int pickaxeDurability)
    {
        block.TakeDamage(_pickaxe.Damage);
        _pickaxe.Durability -= block.DamageToPickaxe;

        if (_pickaxe.Durability <= 0)
        {
            GameEvents.OnPickaxeBroken?.Invoke();
        }

        GameEvents.OnDurabilityChanged?.Invoke(_pickaxe.Durability, _pickaxe.MaxDurability);
    }

    private void HandleBlockDestroyed(Block block, Vector2 position)
    {
        int reward = block.DetermineReward();

        _player.GetReward(reward);
        RewardPerSession += reward;
        BlocksDestroyed++;
    }

    private void HandlePickaxeBroken()
    {
        GameEvents.OnSessionEnded?.Invoke(RewardPerSession);
    }
}
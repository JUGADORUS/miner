using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Player _player;
    [SerializeField] private Pickaxe _pickaxe;

    private ChunkGenerator _foreground;

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

        int durabilityCost = block.DamageToPickaxe;
        DamageBlocks(block, contactPoint, normal);

        _pickaxe.Durability = Mathf.Max(0, _pickaxe.Durability - durabilityCost);
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
        int reward = Mathf.RoundToInt(block.DetermineReward() * _pickaxe.RewardMultiplyer);

        RewardPerSession += reward;
        BlocksDestroyed++;
        _player.GetReward(reward);
        if (reward > 0) GameEvents.OnBlockRewarded?.Invoke(position, reward);
    }

    private void HandlePickaxeBroken()
    {
        GameEvents.OnSessionEnded?.Invoke(RewardPerSession);
    }
}
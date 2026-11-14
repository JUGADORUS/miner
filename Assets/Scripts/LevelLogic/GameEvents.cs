using UnityEngine;

public static class GameEvents
{
    public static System.Action<Block, int> OnHitBlock; // (block, durability)

    public static System.Action<Block, Vector2, Vector2> OnHitBlockWithDirectionPosition; 

    public static System.Action<int,int> OnDurabilityChanged; // (durabiility, maxDurbaility)

    public static System.Action<Block, Vector2> OnBlockDestroyed; // (block, position)

    public static System.Action OnPickaxeBroken;

    public static System.Action<int> OnMoneyChanged;

    public static System.Action<Vector2, int> OnBlockRewarded;

    public static System.Action<Vector2, int> OnCriticalHit;

    public static System.Action<int> OnDurabilityRegenerated;

    public static System.Action<GameObject> OnButtonHovered;

    public static System.Action<GameObject> OnButtonClicked;

    public static System.Action<int> OnSessionEnded;
}

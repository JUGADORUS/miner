using UnityEngine;

public enum EnchantmentKind
{
    Lightning,
    Dynamite,
    Luck
}

public static class PickaxeEnchantments
{
    private const int MaxLevel = 3;

    public static int GetLevel(int pickaxeId)
    {
        // Retain levels purchased before the second enchantment was added.
        int legacyLevel = PlayerPrefs.GetInt($"LightningEnchantment_{pickaxeId}", 0);
        return Mathf.Clamp(PlayerPrefs.GetInt(LevelKey(pickaxeId), legacyLevel), 0, MaxLevel);
    }

    public static EnchantmentKind GetKind(int pickaxeId) =>
        (EnchantmentKind)Mathf.Clamp(PlayerPrefs.GetInt($"EnchantmentKind_{pickaxeId}"), 0, (int)EnchantmentKind.Luck);

    public static float GetHitChance(EnchantmentKind kind, int level) =>
        level * (kind == EnchantmentKind.Lightning ? 2f : 1.5f);

    public static int GetCost(PickaxeData pickaxe)
    {
        return 75 + Mathf.RoundToInt(pickaxe.Price * 0.08f);
    }

    public static bool TryRoll(Player player, PickaxeData pickaxe)
    {
        if (!player.TrySpendMoney(GetCost(pickaxe))) return false;

        int roll = Random.Range(0, 100);
        int level = roll < 75 ? 1 : roll < 95 ? 2 : 3;
        PlayerPrefs.SetInt($"EnchantmentKind_{pickaxe.ID}", Random.Range(0, (int)EnchantmentKind.Luck + 1));
        PlayerPrefs.SetInt(LevelKey(pickaxe.ID), level);
        PlayerPrefs.Save();
        return true;
    }

    private static string LevelKey(int pickaxeId) => $"EnchantmentLevel_{pickaxeId}";
}

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
    internal const int KindCount = (int)EnchantmentKind.Luck + 1;
    private const int BitsPerLevel = 2;

    public static int GetLevel(int pickaxeId, EnchantmentKind kind)
    {
        int levels = PlayerPrefs.GetInt(LevelsKey(pickaxeId), -1);
        if (levels >= 0)
            return (levels >> ((int)kind * BitsPerLevel)) & MaxLevel;

        // Older saves store a single enchantment, or only a lightning level.
        var savedKind = (EnchantmentKind)Mathf.Clamp(
            PlayerPrefs.GetInt($"EnchantmentKind_{pickaxeId}"), 0, KindCount - 1);
        if (kind != savedKind) return 0;

        int lightningLevel = PlayerPrefs.GetInt($"LightningEnchantment_{pickaxeId}", 0);
        return Mathf.Clamp(PlayerPrefs.GetInt($"EnchantmentLevel_{pickaxeId}", lightningLevel), 0, MaxLevel);
    }

    public static float GetHitChance(EnchantmentKind kind, int level) =>
        level * (kind == EnchantmentKind.Lightning ? 2f : 1.5f);

    internal static Color32 GetColor(EnchantmentKind kind) => kind switch
    {
        EnchantmentKind.Dynamite => new Color32(255, 170, 115, 255),
        EnchantmentKind.Luck => new Color32(255, 224, 140, 255),
        _ => new Color32(157, 218, 255, 255)
    };

    public static int GetCost(PickaxeData pickaxe)
    {
        return 75 + Mathf.RoundToInt(pickaxe.Price * 0.08f);
    }

    public static bool TryRoll(Player player, PickaxeData pickaxe)
    {
        if (!player.TrySpendMoney(GetCost(pickaxe))) return false;

        int countRoll = Random.Range(0, 1000);
        int count = countRoll < 5 ? 3 : countRoll < 25 ? 2 : 1;
        int firstKind = Random.Range(0, KindCount);
        // With three kinds, excluding one random kind gives each pair the same chance.
        int levels = 0;
        for (int kind = 0; kind < KindCount; kind++)
        {
            if ((count == 1 && kind != firstKind) || (count == 2 && kind == firstKind))
                continue;

            int rankRoll = Random.Range(0, 100);
            int level = rankRoll < 75 ? 1 : rankRoll < 95 ? 2 : 3;
            levels |= level << (kind * BitsPerLevel);
        }

        PlayerPrefs.SetInt(LevelsKey(pickaxe.ID), levels);
        PlayerPrefs.Save();
        return true;
    }

    private static string LevelsKey(int pickaxeId) => $"EnchantmentLevels_{pickaxeId}";
}

using UnityEngine;
using YG;
using CloudPrefs = RedefineYG.PlayerPrefs;

internal static class MinerProgress
{
    private const string MigrationAccountKey = "Miner_LegacyProgressAccount";
    internal const string MoneyKey = "PlayerMoney";
    internal const string EquippedKey = "EquippedPickaxe";
    internal static bool CloudAvailable { get; set; }

#if UNITY_WEBGL && !UNITY_EDITOR
    private static bool UseCloud => CloudAvailable && YG2.player.auth && !string.IsNullOrEmpty(YG2.player.id);
#else
    private static bool UseCloud => false;
#endif

    private static readonly string[] UpgradeKeys =
    {
        UpgradeIds.Damage, UpgradeIds.Durability, UpgradeIds.Multiplier,
        UpgradeIds.Regeneration, UpgradeIds.CriticalChance, UpgradeIds.CriticalDamage
    };

    private static SavesYG _migrationChecked;
    private static bool _migrationPending;

    internal static int GetInt(string key, int fallback = 0)
    {
        if (!UseCloud) return PlayerPrefs.GetInt(key, fallback);
        MigrateLocalProgress();
        return CloudPrefs.GetInt(key, fallback);
    }

    internal static void SetInt(string key, int value)
    {
        if (!UseCloud)
        {
            PlayerPrefs.SetInt(key, value);
            return;
        }

        MigrateLocalProgress();
        CloudPrefs.SetInt(key, value);
    }

    internal static void Save()
    {
        if (!UseCloud)
        {
            PlayerPrefs.Save();
            return;
        }

        if (YG2.isSDKEnabled)
        {
            CloudPrefs.Save();
            _migrationPending = false;
        }
    }

    internal static void SaveMigration()
    {
        if (!UseCloud || !YG2.isSDKEnabled) return;
        MigrateLocalProgress();
        if (_migrationPending) Save();
    }

    private static void MigrateLocalProgress()
    {
        if (ReferenceEquals(_migrationChecked, YG2.saves)) return;
        _migrationChecked = YG2.saves;
        _migrationPending = false;
        string previousAccount = PlayerPrefs.GetString(MigrationAccountKey, "");
        if (YG2.saves.idSave != 0 || (previousAccount.Length > 0 && previousAccount != YG2.player.id))
            return;

        CopyLegacyValue(MoneyKey);
        CopyLegacyValue(EquippedKey);
        for (int id = 0; id <= PremiumPickaxe.PickaxeId; id++)
        {
            CopyLegacyValue($"Pickaxe_Unlocked_{id}");
            foreach (string upgrade in UpgradeKeys)
                CopyLegacyValue($"Upgrade_{id}_{upgrade}");

            CopyLegacyValue($"EnchantmentLevels_{id}");
            CopyLegacyValue($"EnchantmentKind_{id}");
            CopyLegacyValue($"EnchantmentLevel_{id}");
            CopyLegacyValue($"LightningEnchantment_{id}");
        }

        if (_migrationPending)
        {
            PlayerPrefs.SetString(MigrationAccountKey, YG2.player.id);
            PlayerPrefs.Save();
        }
    }

    private static void CopyLegacyValue(string key)
    {
        if (!PlayerPrefs.HasKey(key) || CloudPrefs.HasKey(key)) return;
        CloudPrefs.SetInt(key, PlayerPrefs.GetInt(key));
        _migrationPending = true;
    }
}

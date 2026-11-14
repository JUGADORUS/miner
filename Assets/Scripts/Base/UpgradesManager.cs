using UnityEngine;

public class UpgradesManager : MonoBehaviour
{
    [Header("Шаблоны апгрейдов")]
    [SerializeField] private UpgradeData _damageUpgrade;
    [SerializeField] private UpgradeData _durabilityUpgrade;
    [SerializeField] private UpgradeData _multiplierUpgrade;
    [SerializeField] private UpgradeData _regenerationUpgrade;
    [SerializeField] private UpgradeData _criticalDamageUpgrade;
    [SerializeField] private UpgradeData _criticalChanceUpgrade;

    public static UpgradesManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int GetDamageBonus(int pickaxeId)
    => Mathf.RoundToInt(_damageUpgrade.GetBonusForLevel(GetUpgradeLevel(pickaxeId, UpgradeIds.Damage)));

    public int GetDurabilityBonus(int pickaxeId)
        => Mathf.RoundToInt(_durabilityUpgrade.GetBonusForLevel(GetUpgradeLevel(pickaxeId, UpgradeIds.Durability)));

    public float GetMultiplierBonus(int pickaxeId)
        => _multiplierUpgrade.GetBonusForLevel(GetUpgradeLevel(pickaxeId, UpgradeIds.Multiplier));

    public int GetRegenerationBonus(int pickaxeId)
        => _regenerationUpgrade != null
            ? Mathf.RoundToInt(_regenerationUpgrade.GetBonusForLevel(GetUpgradeLevel(pickaxeId, UpgradeIds.Regeneration)))
            : 0;

    public float GetCriticalDamageMultiplier(int pickaxeId)
        => GameManager.DefaultCriticalDamageMultiplier + GetBonus(_criticalDamageUpgrade, pickaxeId);

    public float GetCriticalChance(int pickaxeId)
        => Mathf.Clamp01(GetBonus(_criticalChanceUpgrade, pickaxeId));

    private float GetBonus(UpgradeData upgrade, int pickaxeId)
        => upgrade != null ? upgrade.GetBonusForLevel(GetUpgradeLevel(pickaxeId, upgrade.upgradeID)) : 0f;

    public int GetUpgradeLevel(int pickaxeId, string upgradeId)
    {
        int savedLevel = PlayerPrefs.GetInt(GetKey(pickaxeId, upgradeId), 0);
        return Mathf.Clamp(savedLevel, 0, GetMaxLevel(upgradeId));
    }

    public void BuyUpgrade(int pickaxeId, string upgradeId)
    {
        int currentLevel = GetUpgradeLevel(pickaxeId, upgradeId);
        if (currentLevel >= GetMaxLevel(upgradeId)) return;

        PlayerPrefs.SetInt(GetKey(pickaxeId, upgradeId), currentLevel + 1);
        PlayerPrefs.Save();
    }

    private int GetMaxLevel(string upgradeId)
    {
        return upgradeId switch
        {
            UpgradeIds.Damage => _damageUpgrade.maxLevel,
            UpgradeIds.Durability => _durabilityUpgrade.maxLevel,
            UpgradeIds.Multiplier => _multiplierUpgrade.maxLevel,
            UpgradeIds.Regeneration => _regenerationUpgrade != null ? _regenerationUpgrade.maxLevel : 0,
            UpgradeIds.CriticalDamage => _criticalDamageUpgrade != null ? _criticalDamageUpgrade.maxLevel : 0,
            UpgradeIds.CriticalChance => _criticalChanceUpgrade != null ? _criticalChanceUpgrade.maxLevel : 0,
            _ => int.MaxValue
        };
    }

    private static string GetKey(int pickaxeId, string upgradeId)
    {
        return $"Upgrade_{pickaxeId}_{upgradeId}";
    }
}
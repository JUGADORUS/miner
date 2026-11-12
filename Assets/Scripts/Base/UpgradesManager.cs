using UnityEngine;

public class UpgradesManager : MonoBehaviour
{
    [Header("Шаблоны апгрейдов")]
    [SerializeField] private UpgradeData _damageUpgrade;
    [SerializeField] private UpgradeData _durabilityUpgrade;
    [SerializeField] private UpgradeData _multiplierUpgrade;

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
            _ => int.MaxValue
        };
    }

    private static string GetKey(int pickaxeId, string upgradeId)
    {
        return $"Upgrade_{pickaxeId}_{upgradeId}";
    }
}
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
        return PlayerPrefs.GetInt(GetKey(pickaxeId, upgradeId), 0);
    }

    public void BuyUpgrade(int pickaxeId, string upgradeId)
    {
        int currentLevel = GetUpgradeLevel(pickaxeId, upgradeId);
        PlayerPrefs.SetInt(GetKey(pickaxeId, upgradeId), currentLevel + 1);
        PlayerPrefs.Save();
    }

    private static string GetKey(int pickaxeId, string upgradeId)
    {
        return $"Upgrade_{pickaxeId}_{upgradeId}";
    }
}
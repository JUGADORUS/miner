using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPickaxeStatisticsPanel : MonoBehaviour
{
    [SerializeField] private Image _pickaxeIcon;
    [SerializeField] private TextMeshProUGUI _damageText;
    [SerializeField] private TextMeshProUGUI _durabilityText;
    [SerializeField] private TextMeshProUGUI _rangeOfDiggingText;
    [SerializeField] private TextMeshProUGUI _rewardMultiplierText;

    [Header("Шаблоны улучшений для расчета бонусов")]
    [SerializeField] private UpgradeData _damageUpgrade;
    [SerializeField] private UpgradeData _durabilityUpgrade;
    [SerializeField] private UpgradeData _multiplierUpgrade;

    public void UpdateInfo(PickaxeData pickaxeData)
    {
        if (pickaxeData == null) return;
        if (UpgradesManager.Instance == null) return;

        int damageLvl = UpgradesManager.Instance.GetUpgradeLevel(pickaxeData.ID, UpgradeIds.Damage);
        int durabilityLvl = UpgradesManager.Instance.GetUpgradeLevel(pickaxeData.ID, UpgradeIds.Durability);
        int multiplierLvl = UpgradesManager.Instance.GetUpgradeLevel(pickaxeData.ID, UpgradeIds.Multiplier);

        int finalDamage = pickaxeData.Damage + Mathf.RoundToInt(_damageUpgrade.GetBonusForLevel(damageLvl));
        int finalDurability = pickaxeData.MaxDurability + Mathf.RoundToInt(_durabilityUpgrade.GetBonusForLevel(durabilityLvl));
        float finalMultiplier = pickaxeData.RewardMultiplier + _multiplierUpgrade.GetBonusForLevel(multiplierLvl);

        _pickaxeIcon.sprite = pickaxeData.icon;

        _damageText.text = $"Damage: {finalDamage}\n(Lvl {damageLvl})";
        _durabilityText.text = $"Durability: {finalDurability}\n(Lvl {durabilityLvl})";
        _rangeOfDiggingText.text = $"Range of digging: {pickaxeData.RangeOfDigging}";
        _rewardMultiplierText.text = $"Reward multiplier: {finalMultiplier:0.##}\n(Lvl {multiplierLvl})";
    }
}
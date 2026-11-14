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
    [SerializeField] private TextMeshProUGUI _regenerationText;
    [SerializeField] private TextMeshProUGUI _criticalChanceText;
    [SerializeField] private TextMeshProUGUI _criticalDamageText;

    [Header("Шаблоны улучшений для расчета бонусов")]
    [SerializeField] private UpgradeData _damageUpgrade;
    [SerializeField] private UpgradeData _durabilityUpgrade;

    public void UpdateInfo(PickaxeData pickaxeData)
    {
        if (pickaxeData == null) return;
        if (UpgradesManager.Instance == null) return;

        int damageLvl = UpgradesManager.Instance.GetUpgradeLevel(pickaxeData.ID, UpgradeIds.Damage);
        int durabilityLvl = UpgradesManager.Instance.GetUpgradeLevel(pickaxeData.ID, UpgradeIds.Durability);
        int multiplierLvl = UpgradesManager.Instance.GetUpgradeLevel(pickaxeData.ID, UpgradeIds.Multiplier);

        int finalDamage = pickaxeData.Damage + Mathf.RoundToInt(_damageUpgrade.GetBonusForLevel(damageLvl));
        int finalDurability = pickaxeData.MaxDurability + Mathf.RoundToInt(_durabilityUpgrade.GetBonusForLevel(durabilityLvl));
        float finalMultiplier = pickaxeData.GetRewardMultiplier(UpgradesManager.Instance.GetMultiplierBonus(pickaxeData.ID));

        _pickaxeIcon.sprite = pickaxeData.icon;

        _damageText.text = $"Damage: {finalDamage} <size=75%>(Lvl {damageLvl})</size>";
        _durabilityText.text = $"Durability: {finalDurability} <size=75%>(Lvl {durabilityLvl})</size>";
        _rangeOfDiggingText.text = $"Range of digging: {pickaxeData.RangeOfDigging}";
        _rewardMultiplierText.text = $"Reward multiplier: {finalMultiplier:0.##} <size=75%>(Lvl {multiplierLvl})</size>";

        var upgrades = UpgradesManager.Instance;
        int pickaxeId = pickaxeData.ID;
        if (_regenerationText != null)
            _regenerationText.text = $"Regen: +{upgrades.GetRegenerationBonus(pickaxeId)} / s <size=75%>(Lvl {upgrades.GetUpgradeLevel(pickaxeId, UpgradeIds.Regeneration)})</size>";
        if (_criticalChanceText != null)
            _criticalChanceText.text = $"Crit chance: {upgrades.GetCriticalChance(pickaxeId):P0} <size=75%>(Lvl {upgrades.GetUpgradeLevel(pickaxeId, UpgradeIds.CriticalChance)})</size>";
        if (_criticalDamageText != null)
            _criticalDamageText.text = $"Crit damage: x{upgrades.GetCriticalDamageMultiplier(pickaxeId):0.##} <size=75%>(Lvl {upgrades.GetUpgradeLevel(pickaxeId, UpgradeIds.CriticalDamage)})</size>";
    }
}
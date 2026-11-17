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

    private PickaxeData _shownPickaxe;

    private void OnEnable()
    {
        MinerLocalization.Changed += RefreshLanguage;
        RefreshLanguage();
    }

    private void OnDisable() => MinerLocalization.Changed -= RefreshLanguage;

    private void RefreshLanguage() => UpdateInfo(_shownPickaxe);

    public void UpdateInfo(PickaxeData pickaxeData)
    {
        _shownPickaxe = pickaxeData;
        if (pickaxeData == null) return;
        if (UpgradesManager.Instance == null) return;

        int damageLvl = UpgradesManager.Instance.GetUpgradeLevel(pickaxeData.ID, UpgradeIds.Damage);
        int durabilityLvl = UpgradesManager.Instance.GetUpgradeLevel(pickaxeData.ID, UpgradeIds.Durability);
        int multiplierLvl = UpgradesManager.Instance.GetUpgradeLevel(pickaxeData.ID, UpgradeIds.Multiplier);

        int finalDamage = pickaxeData.Damage + Mathf.RoundToInt(_damageUpgrade.GetBonusForLevel(damageLvl));
        int finalDurability = pickaxeData.MaxDurability + Mathf.RoundToInt(_durabilityUpgrade.GetBonusForLevel(durabilityLvl));
        float finalMultiplier = pickaxeData.GetRewardMultiplier(UpgradesManager.Instance.GetMultiplierBonus(pickaxeData.ID));

        _pickaxeIcon.sprite = pickaxeData.icon;
        _pickaxeIcon.color = pickaxeData.Tint;

        _damageText.text = $"{MinerLocalization.Text("Damage")}: {finalDamage} <size=75%>({MinerLocalization.Format("Lvl {0}", damageLvl)})</size>";
        _durabilityText.text = $"{MinerLocalization.Text("Durability")}: {finalDurability} <size=75%>({MinerLocalization.Format("Lvl {0}", durabilityLvl)})</size>";
        _rangeOfDiggingText.text = $"{MinerLocalization.Text("Range of digging")}: {pickaxeData.RangeOfDigging}";
        _rewardMultiplierText.text = $"{MinerLocalization.Text("Reward multiplier")}: {finalMultiplier:0.##} <size=75%>({MinerLocalization.Format("Lvl {0}", multiplierLvl)})</size>";

        var upgrades = UpgradesManager.Instance;
        int pickaxeId = pickaxeData.ID;
        if (_regenerationText != null)
            _regenerationText.text = $"{MinerLocalization.Text("Regen")}: +{MinerLocalization.Format("{0} / s", upgrades.GetRegenerationBonus(pickaxeId))} <size=75%>({MinerLocalization.Format("Lvl {0}", upgrades.GetUpgradeLevel(pickaxeId, UpgradeIds.Regeneration))})</size>";
        if (_criticalChanceText != null)
            _criticalChanceText.text = $"{MinerLocalization.Text("Crit chance")}: {upgrades.GetCriticalChance(pickaxeId):P0} <size=75%>({MinerLocalization.Format("Lvl {0}", upgrades.GetUpgradeLevel(pickaxeId, UpgradeIds.CriticalChance))})</size>";
        if (_criticalDamageText != null)
            _criticalDamageText.text = $"{MinerLocalization.Text("Crit damage")}: x{upgrades.GetCriticalDamageMultiplier(pickaxeId):0.##} <size=75%>({MinerLocalization.Format("Lvl {0}", upgrades.GetUpgradeLevel(pickaxeId, UpgradeIds.CriticalDamage))})</size>";
    }
}
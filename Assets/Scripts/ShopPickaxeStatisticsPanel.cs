using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPickaxeStatisticsPanel : MonoBehaviour
{
    [SerializeField] private Image _pickaxeIcon;
    [SerializeField] private TextMeshProUGUI _damageText;
    [SerializeField] private TextMeshProUGUI _durabilityText;
    [SerializeField] private TextMeshProUGUI _rangeOfDiggingText;
    [SerializeField] private TextMeshProUGUI _rewardMultiplyerText;

    public void UpdateInfo(PickaxeData pickaxeData)
    {
        _pickaxeIcon.sprite = pickaxeData.icon;
        _damageText.text = $"Damage: {pickaxeData.Damage}";
        _durabilityText.text = $"Durability: {pickaxeData.MaxDurability}";
        _rangeOfDiggingText.text = $"Range of digging: {pickaxeData.RangeOfDigging}";
        _rewardMultiplyerText.text = $"Reward multiplyer: {pickaxeData.RewardMultiplyer}";
    }
}

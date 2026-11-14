using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ForgeUI : MonoBehaviour
{
    [Header("Настройки UI списка")]
    [SerializeField] private GameObject _itemPrefab;
    [SerializeField] private GameObject _whereToSpawn;
    [SerializeField] private TextMeshProUGUI _moneyText;

    [Header("Ссылки на внешние компоненты")]
    [SerializeField] private Player _player;
    [SerializeField] private ShopPickaxeStatisticsPanel _statisticsPanel;
    [SerializeField] private PickaxeDatabase _pickaxeDatabase;

    [Header("Список доступных улучшений")]
    [SerializeField] private List<UpgradeData> _availableUpgrades;

    private const string EquippedKey = "EquippedPickaxe";

    private void OnEnable()
    {
        UpdateMoneyText(_player.Money);
        GameEvents.OnMoneyChanged += UpdateMoneyText;

        if (UpgradesManager.Instance != null)
        {
            RefreshForgeUI();
        }
    }

    private void OnDisable()
    {
        GameEvents.OnMoneyChanged -= UpdateMoneyText;
    }

    private void RefreshForgeUI()
    {
        foreach (Transform child in _whereToSpawn.transform)
        {
            Destroy(child.gameObject);
        }

        int equippedPickaxeId = PlayerPrefs.GetInt(EquippedKey, 0);

        PickaxeData equippedData = _pickaxeDatabase.GetById(equippedPickaxeId);
        if (equippedData == null) return;

        _statisticsPanel.UpdateInfo(equippedData);

        foreach (UpgradeData upgrade in _availableUpgrades)
        {
            GameObject obj = Instantiate(_itemPrefab, _whereToSpawn.transform);
            ItemInShop item = obj.GetComponent<ItemInShop>();

            item.Name.text = upgrade.upgradeID switch
            {
                UpgradeIds.Regeneration => $"{upgrade.upgradeName}\n+{upgrade.valuePerLevel:0.##} durability / sec per level",
                UpgradeIds.CriticalDamage => $"{upgrade.upgradeName}\nx{UpgradesManager.Instance.GetCriticalDamageMultiplier(equippedData.ID):0.##} (+{upgrade.valuePerLevel:0.##}x/lvl)",
                UpgradeIds.CriticalChance => $"{upgrade.upgradeName}\n{UpgradesManager.Instance.GetCriticalChance(equippedData.ID):P0} (+{upgrade.valuePerLevel:P0}/lvl)",
                _ => upgrade.upgradeName
            };

            UpdateUpgradeButtonVisual(item, upgrade, equippedData);

            item.Button.onClick.RemoveAllListeners();
            item.Button.onClick.AddListener(() => TryBuyUpgrade(upgrade, equippedData));
        }
    }

    private void UpdateUpgradeButtonVisual(ItemInShop item, UpgradeData upgrade, PickaxeData pickaxe)
    {
        if (upgrade == null || UpgradesManager.Instance == null) return;

        int currentLvl = UpgradesManager.Instance.GetUpgradeLevel(pickaxe.ID, upgrade.upgradeID);

        if (currentLvl >= upgrade.maxLevel)
        {
            SetButton(item, "#DF1428", "MAX", 40f, false);
            return;
        }

        int cost = GetUpgradeCost(upgrade, currentLvl, pickaxe);

        if (_player.CanSpend(cost))
        {
            SetButton(item, "#29BB3C", $"Lvl {currentLvl}\n{cost}$", 35f, true);
        }
        else
        {
            SetButton(item, "#DF1428", $"Lvl {currentLvl}\n{cost}$", 35f, false);
        }
    }

    private void TryBuyUpgrade(UpgradeData upgrade, PickaxeData pickaxe)
    {
        if (UpgradesManager.Instance == null) return;

        int currentLvl = UpgradesManager.Instance.GetUpgradeLevel(pickaxe.ID, upgrade.upgradeID);
        if (currentLvl >= upgrade.maxLevel) return;

        int cost = GetUpgradeCost(upgrade, currentLvl, pickaxe);
        if (!_player.TrySpendMoney(cost)) return;

        UpgradesManager.Instance.BuyUpgrade(pickaxe.ID, upgrade.upgradeID);
        RefreshForgeUI();
    }

    private int GetUpgradeCost(UpgradeData upgrade, int currentLevel, PickaxeData pickaxe)
    {
        PickaxeData nextPickaxe = _pickaxeDatabase.GetById(pickaxe.ID + 1);
        return upgrade.GetCostForLevel(currentLevel, nextPickaxe != null ? nextPickaxe.Price : pickaxe.Price);
    }

    private static void SetButton(ItemInShop item, string hexColor, string text, float fontSize, bool interactable)
    {
        ColorUtility.TryParseHtmlString(hexColor, out Color color);
        item.ButtonImage.color = color;
        item.Action.text = text;
        item.Action.fontSize = fontSize;
        item.Button.interactable = interactable;
    }

    private void UpdateMoneyText(int money)
    {
        _moneyText.text = money.ToString();
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private GameObject _itemPrefab;
    [SerializeField] private GameObject _whereToSpawn;
    [SerializeField] private PickaxeDatabase _pickaxeDatabase;
    [SerializeField] private Player _player;
    [SerializeField] private ShopPickaxeStatisticsPanel _statisticsPanel;
    [SerializeField] private TextMeshProUGUI _moneyText;

    private const string EquippedKey = "EquippedPickaxe";

    private void Start()
    {
        Unlock(0);
        UpdateMoneyText(PlayerPrefs.GetInt("PlayerMoney", 0));
        GameEvents.OnMoneyChanged += UpdateMoneyText;

        RefreshShopUI();
        _statisticsPanel.UpdateInfo(_pickaxeDatabase.GetById(GetEquippedId()));
    }

    private void OnEnable()
    {
        GameEvents.OnMoneyChanged += HandleMoneyChanged;
        RefreshShopUI();
    }

    private void OnDisable()
    {
        GameEvents.OnMoneyChanged -= HandleMoneyChanged;
    }

    private void HandleMoneyChanged(int money)
    {
        UpdateMoneyText(money);
        RefreshShopUI();
    }

    private void OnDestroy()
    {
        GameEvents.OnMoneyChanged -= UpdateMoneyText;
    }

    private void AddAllPickaxes()
    {
        foreach (PickaxeData data in _pickaxeDatabase.AllPickaxes)
        {
            GameObject obj = Instantiate(_itemPrefab, _whereToSpawn.transform);
            ItemInShop item = obj.GetComponent<ItemInShop>();

            item.Pickaxe.sprite = data.icon;
            item.Name.text = data.Name;

            item.Button.onClick.AddListener(() => OnItemClicked(data));
            obj.GetComponent<Button>().onClick.AddListener(() => _statisticsPanel.UpdateInfo(data));

            ApplyButtonState(item, data);
        }
    }

    private void ApplyButtonState(ItemInShop item, PickaxeData data)
    {
        if (IsUnlocked(data.ID) && IsEquipped(data.ID))
        {
            SetButton(item, "#EB8900", "Equipped", 40f);
        }
        else if (IsUnlocked(data.ID))
        {
            SetButton(item, "#F3B200", "Equip", 50f);
        }
        else if (_player.CanSpend(data.Price))
        {
            SetButton(item, "#29BB3C", $"Buy\n{data.Price}$", 40f);
        }
        else
        {
            SetButton(item, "#DF1428", $"Buy\n{data.Price}$", 40f);
        }
    }

    private static void SetButton(ItemInShop item, string hexColor, string text, float fontSize)
    {
        ColorUtility.TryParseHtmlString(hexColor, out Color color);
        item.ButtonImage.color = color;
        item.Action.text = text;
        item.Action.fontSize = fontSize;
    }

    private void OnItemClicked(PickaxeData data)
    {
        if (IsUnlocked(data.ID))
        {
            PlayerPrefs.SetInt(EquippedKey, data.ID);
            PlayerPrefs.Save();
        }
        else if (_player.TrySpendMoney(data.Price))
        {
            Unlock(data.ID);
        }

        RefreshShopUI();
    }

    private void RefreshShopUI()
    {
        foreach (Transform child in _whereToSpawn.transform)
        {
            Destroy(child.gameObject);
        }

        AddAllPickaxes();
    }

    public static bool IsUnlocked(int pickaxeId)
    {
        if (pickaxeId == 0) return true;
        return PlayerPrefs.GetInt($"Pickaxe_Unlocked_{pickaxeId}", 0) == 1;
    }

    public static void Unlock(int pickaxeId)
    {
        PlayerPrefs.SetInt($"Pickaxe_Unlocked_{pickaxeId}", 1);
        PlayerPrefs.Save();
    }

    public static int GetEquippedId()
    {
        return PlayerPrefs.GetInt(EquippedKey, 0);
    }

    public static bool IsEquipped(int pickaxeId)
    {
        return GetEquippedId() == pickaxeId;
    }

    private void UpdateMoneyText(int money)
    {
        _moneyText.text = money.ToString();
    }

    [ContextMenu("Мод много денег")]
    public void GetMoney()
    {
        _player.GetReward(1000);
    }
}
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

    private const string EquippedKey = MinerProgress.EquippedKey;

    private void Start()
    {
        UpdateMoneyText(MinerProgress.GetInt("PlayerMoney", 0));
        GameEvents.OnMoneyChanged += UpdateMoneyText;

        RefreshShopUI();
        _statisticsPanel.UpdateInfo(_pickaxeDatabase.GetById(GetEquippedId()));
    }

    private void OnEnable()
    {
        GameEvents.OnMoneyChanged += HandleMoneyChanged;
        MinerLocalization.Changed += RefreshShopUI;
        PremiumPickaxe.Changed += RefreshShopUI;
        RefreshShopUI();
    }

    private void OnDisable()
    {
        GameEvents.OnMoneyChanged -= HandleMoneyChanged;
        MinerLocalization.Changed -= RefreshShopUI;
        PremiumPickaxe.Changed -= RefreshShopUI;
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
            if (data.ID == PremiumPickaxe.PickaxeId && !PremiumPickaxe.Owned &&
                string.IsNullOrEmpty(PremiumPickaxe.Product?.price))
                continue;

            GameObject obj = Instantiate(_itemPrefab, _whereToSpawn.transform);
            ItemInShop item = obj.GetComponent<ItemInShop>();

            item.Pickaxe.sprite = data.icon;
            item.Pickaxe.color = data.Tint;
            item.Name.text = MinerLocalization.Text(data.Name);

            item.Button.onClick.AddListener(() => OnItemClicked(data));
            obj.GetComponent<Button>().onClick.AddListener(() => _statisticsPanel.UpdateInfo(data));

            ApplyButtonState(item, data);
        }
    }

    private void ApplyButtonState(ItemInShop item, PickaxeData data)
    {
        if (IsUnlocked(data.ID) && IsEquipped(data.ID))
        {
            SetButton(item, "#EB8900", MinerLocalization.Text("Equipped"), 40f);
        }
        else if (IsUnlocked(data.ID))
        {
            SetButton(item, "#F3B200", MinerLocalization.Text("Equip"), 50f);
        }
        else if (data.ID == PremiumPickaxe.PickaxeId)
        {
            SetPremiumButton(item);
        }
        else if (_player.CanSpend(data.Price))
        {
            SetButton(item, "#29BB3C", MinerLocalization.Format("Buy\n{0}$", data.Price), 40f);
        }
        else
        {
            SetButton(item, "#DF1428", MinerLocalization.Format("Buy\n{0}$", data.Price), 40f);
        }
    }

    private static void SetPremiumButton(ItemInShop item)
    {
        var product = PremiumPickaxe.Product;
        string label = PremiumPickaxe.Buying ? MinerLocalization.Text("Opening payment‚Ä¶")
            : MinerLocalization.Format(PremiumPickaxe.Failed ? "Retry payment\n{0}" : "Buy\n{0}", product.price);
        SetButton(item, "#C493F2", label, 32f);
        item.Action.enableAutoSizing = true;
        item.Action.fontSizeMin = 16f;
        item.Action.fontSizeMax = 32f;
        item.Button.interactable = !PremiumPickaxe.Buying;

        if (PremiumPickaxe.Buying || string.IsNullOrEmpty(product.currencyImageURL))
            return;

        var icon = new GameObject("Portal Currency", typeof(RectTransform), typeof(RawImage));
        icon.layer = item.Action.gameObject.layer;
        var rect = (RectTransform)icon.transform;
        rect.SetParent(item.Action.rectTransform, false);
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.25f);
        rect.anchoredPosition = new Vector2(-17f, 0f);
        rect.sizeDelta = new Vector2(24f, 24f);
        var image = icon.GetComponent<RawImage>();
        image.raycastTarget = false;
        var loader = icon.AddComponent<YG.ImageLoadYG>();
        loader.rawImage = image;
        image.enabled = false;
        loader.Load(product.currencyImageURL);
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
            MinerProgress.SetInt(EquippedKey, data.ID);
            MinerProgress.Save();
        }
        else if (data.ID == PremiumPickaxe.PickaxeId)
        {
            PremiumPickaxe.Buy();
            return;
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
        if (pickaxeId == PremiumPickaxe.PickaxeId) return PremiumPickaxe.Owned;
        return MinerProgress.GetInt($"Pickaxe_Unlocked_{pickaxeId}", 0) == 1;
    }

    public static void Unlock(int pickaxeId)
    {
        if (pickaxeId == PremiumPickaxe.PickaxeId) return;
        MinerProgress.SetInt($"Pickaxe_Unlocked_{pickaxeId}", 1);
        MinerProgress.Save();
    }

    public static int GetEquippedId()
    {
        int id = MinerProgress.GetInt(EquippedKey);
        return id == PremiumPickaxe.PickaxeId && !PremiumPickaxe.Owned ? 0 : id;
    }

    public static bool IsEquipped(int pickaxeId)
    {
        return GetEquippedId() == pickaxeId;
    }

    private void UpdateMoneyText(int money)
    {
        _moneyText.text = money.ToString();
    }

    [ContextMenu("ÃÓ‰ ÏÌÓ„Ó ‰ÂÌÂ„")]
    public void GetMoney()
    {
        _player.GetReward(1000);
    }
}
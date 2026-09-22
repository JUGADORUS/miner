using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] GameObject Item;
    [SerializeField] GameObject WhereToSpawn;
    [SerializeField] PickaxeDatabase PickaxeDatabase;
    [SerializeField] Player player;
    [SerializeField] ShopPickaxeStatisticsPanel statisticsPanel;
    [SerializeField] TextMeshProUGUI Money;

    private const string EquippedKey = "EquippedPickaxe";
    private const string SelectedKey = "SelectedPickaxe";

    private void Awake()
    {
        Unlock(0);
        AddAllPickaxes();
    }

    private void Start()
    {
        RefreshShopUI();
        statisticsPanel.UpdateInfo(PickaxeDatabase.GetByIndex(GetEquippedId()));
    }

    private void AddAllPickaxes()
    {
        List<PickaxeData> pickaxeDatas = PickaxeDatabase.GetPickaxeDatas();

        foreach (PickaxeData data in pickaxeDatas)
        {
            GameObject objectd = Instantiate(Item,WhereToSpawn.transform);
            ItemInShop itemInShop = objectd.GetComponent<ItemInShop>();
            itemInShop.Button.onClick.AddListener(() => OnItemClicked(data, itemInShop));
            objectd.GetComponent<Button>().onClick.AddListener(() => statisticsPanel.UpdateInfo(data));

            itemInShop.Pickaxe.sprite = data.icon;
            itemInShop.Name.text = data.Name;

            if(IsUnlocked(data.ID) && IsEquipped(data.ID))
            {
                MakeEquippedButton(itemInShop, data);
            }
            else if(IsUnlocked(data.ID) && !IsEquipped(data.ID))
            {
                MakeEquipButton(itemInShop, data);
            }
            else if(!IsUnlocked(data.ID) && player.CanSpend(data.Price))
            {
                MakeBuyButton(itemInShop,data);
            }
            else if (!IsUnlocked(data.ID) && !player.CanSpend(data.Price))
            {
                MakeCantBuyButton(itemInShop,data);
            }
        }
    }

    public static bool IsUnlocked(int pickaxeId)
    {
        if (pickaxeId == 0) return true;
        return PlayerPrefs.GetInt("Pickaxe_Unlocked_" + pickaxeId, 0) == 1;
    }

    public static void Unlock(int pickaxeId)
    {
        PlayerPrefs.SetInt("Pickaxe_Unlocked_" + pickaxeId, 1);
        PlayerPrefs.Save();
    }

    public static int GetEquippedId()
    {
        return PlayerPrefs.GetInt(EquippedKey, 0);
    }

    public static int GetSelectedId()
    {
        return PlayerPrefs.GetInt(SelectedKey, 0);
    }

    public static bool IsEquipped(int pickaxeId)
    {
        return GetEquippedId() == pickaxeId;
    }

    private void OnItemClicked(PickaxeData data, ItemInShop uiItem)
    {
        if (IsUnlocked(data.ID))
        {
            PlayerPrefs.SetInt(EquippedKey, data.ID);
            PlayerPrefs.Save();
        }
        else
        {
            if (player.TrySpendMoney(data.Price))
            {
                Unlock(data.ID);
            }
            else
            {
                Debug.Log("Недостаточно денег!");
            }
        }
        RefreshShopUI();
    }

    private void RefreshShopUI()
    {
        foreach (Transform child in WhereToSpawn.transform)
        {
            Destroy(child.gameObject);
        }

        AddAllPickaxes();
    }

    private void MakeBuyButton(ItemInShop itemInShop, PickaxeData data)
    {
        ColorUtility.TryParseHtmlString("#29BB3C", out Color myColor);
        itemInShop.ButtonImage.color = myColor;

        itemInShop.Action.text = $"Buy\n{data.Price}$";
        itemInShop.Action.fontSize = 40f;
    }

    private void MakeEquippedButton(ItemInShop itemInShop, PickaxeData data)
    {
        ColorUtility.TryParseHtmlString("#EB8900", out Color myColor);
        itemInShop.ButtonImage.color = myColor;

        itemInShop.Action.text = "Equipped";
        itemInShop.Action.fontSize = 40f;
    }

    private void MakeEquipButton(ItemInShop itemInShop, PickaxeData data)
    {
        ColorUtility.TryParseHtmlString("#F3B200", out Color myColor);
        itemInShop.ButtonImage.color = myColor;

        itemInShop.Action.text = "Equip";
        itemInShop.Action.fontSize = 50f;
    }

    private void MakeCantBuyButton(ItemInShop itemInShop, PickaxeData data)
    {
        ColorUtility.TryParseHtmlString("#DF1428", out Color myColor);
        itemInShop.ButtonImage.color = myColor;

        itemInShop.Action.text = $"Buy\n{data.Price}$";
        itemInShop.Action.fontSize = 40f;
    }
}

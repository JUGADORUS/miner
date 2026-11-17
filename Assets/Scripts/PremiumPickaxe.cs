using System;
using UnityEngine;
using YG;
using YG.Utils.Pay;

internal static class PremiumPickaxe
{
    internal const int PickaxeId = 7;
    internal const string ProductId = "miner_titan_pickaxe";
    private const string SimulationKey = "Miner_TitanPickaxe_Simulated";

    internal static event Action Changed;
    internal static bool Buying { get; private set; }
    internal static bool Failed { get; private set; }
    internal static Purchase Product => YG2.PurchaseByID(ProductId);

#if UNITY_EDITOR
    internal static bool Owned => PlayerPrefs.GetInt(SimulationKey, 0) == 1;
#else
    internal static bool Owned => Array.IndexOf(YG2.ownedProductIds, ProductId) >= 0;
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        Buying = false;
        Failed = false;
        Changed = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Subscribe()
    {
        YG2.onGetPayments -= Refresh;
        YG2.onGetPayments += Refresh;
        YG2.onGetSDKData -= Refresh;
        YG2.onGetSDKData += Refresh;
        YG2.onPurchaseSuccess -= PurchaseSucceeded;
        YG2.onPurchaseSuccess += PurchaseSucceeded;
        YG2.onPurchaseFailed -= PurchaseFailed;
        YG2.onPurchaseFailed += PurchaseFailed;
        Refresh();
    }

    internal static void Buy()
    {
        if (Buying || Owned || Product == null || string.IsNullOrEmpty(Product.price) ||
            !YG2.isSDKEnabled || YG2.nowAdsShow)
            return;

        Buying = true;
        Failed = false;
        Changed?.Invoke();
        YG2.BuyPayments(ProductId);
    }

    private static void Refresh()
    {
        MinerProgress.SaveMigration();
        Changed?.Invoke();
    }

    private static void PurchaseSucceeded(string id)
    {
        if (id != ProductId) return;
#if UNITY_EDITOR
        PlayerPrefs.SetInt(SimulationKey, 1);
        PlayerPrefs.Save();
#endif
        Buying = false;
        Failed = false;
        Changed?.Invoke();
    }

    private static void PurchaseFailed(string id)
    {
        if (id != ProductId || !Buying) return;
        Buying = false;
        Failed = true;
        Changed?.Invoke();
    }
}

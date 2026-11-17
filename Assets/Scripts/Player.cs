using UnityEngine;

public class Player : MonoBehaviour
{
    public int Money { get; private set; }

    private const string MoneyKey = MinerProgress.MoneyKey;

    private void Awake()
    {
        Money = MinerProgress.GetInt(MoneyKey, 0);
        YG.YG2.onGetSDKData += RefreshCloudBalance;
    }

    private void Start()
    {
        GameEvents.OnMoneyChanged?.Invoke(Money);
    }

    private void OnDestroy() => YG.YG2.onGetSDKData -= RefreshCloudBalance;

    private void RefreshCloudBalance()
    {
        Money = MinerProgress.GetInt(MoneyKey);
        GameEvents.OnMoneyChanged?.Invoke(Money);
    }

    public bool CanSpend(int amount) => Money >= amount;

    public bool TrySpendMoney(int amount)
    {
        if (!CanSpend(amount)) return false;
        SetMoney(Money - amount);
        return true;
    }

    public void GetReward(int reward) => SetMoney(Money + reward);

    private void SetMoney(int value)
    {
        Money = value;
        MinerProgress.SetInt(MoneyKey, Money);
        MinerProgress.Save();
        GameEvents.OnMoneyChanged?.Invoke(Money);
    }
}
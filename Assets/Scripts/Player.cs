using UnityEngine;

public class Player : MonoBehaviour
{
    public int Money { get; private set; }

    private const string MoneyKey = "PlayerMoney";

    private void Awake()
    {
        Money = PlayerPrefs.GetInt(MoneyKey, 0);
    }

    private void Start()
    {
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
        PlayerPrefs.SetInt(MoneyKey, Money);
        PlayerPrefs.Save();
        GameEvents.OnMoneyChanged?.Invoke(Money);
    }
}
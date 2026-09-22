using UnityEngine;

public class Player : MonoBehaviour
{
    public int Money;

    private void Awake()
    {
        Money = PlayerPrefs.GetInt("PlayerMoney", 0);
    }

    private void Start()
    {
        GameEvents.OnMoneyChanged?.Invoke(Money);
    }

    public void GetReward(int reward)
    {
        Money = PlayerPrefs.GetInt("PlayerMoney", 0) + reward;
        PlayerPrefs.SetInt("PlayerMoney", Money);
        PlayerPrefs.Save();

        GameEvents.OnMoneyChanged?.Invoke(Money);
    }

    public bool TrySpendMoney(int amount)
    {
        if (Money >= amount)
        {
            Money -= amount;
            PlayerPrefs.SetInt("PlayerMoney", Money);
            PlayerPrefs.Save();
            GameEvents.OnMoneyChanged?.Invoke(Money);
            return true;
        }

        return false;
    }

    public bool CanSpend(int money)
    {
        return PlayerPrefs.GetInt("PlayerMoney", 0) >= money;
    }
}
using UnityEngine;

public class Player : MonoBehaviour
{
    public int Money;

    public void GetReward(int reward)
    {
        Money = Money + reward;

        GameEvents.OnMoneyChanged?.Invoke(Money);
    }
}

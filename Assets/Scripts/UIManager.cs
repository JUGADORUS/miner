using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Money;
    [SerializeField] TextMeshProUGUI Depth;
    [SerializeField] TextMeshProUGUI Durability;

    [SerializeField] private Slider durabilitySlider;

    private int maxDepth = 0;

    void Start()
    {
        UpdateMoneyText(PlayerPrefs.GetInt("PlayerMoney", 0));
        GameEvents.OnMoneyChanged += UpdateMoneyText;
        GameEvents.OnHitBlock += (block, durability) => UpdateDurabilityText(durability); 
        GameEvents.OnBlockDestroyed += (block, position) => UpdateDepthText( Mathf.Abs((int)position.y) );
        GameEvents.OnDurabilityChanged += (durability, maxDurability) => UpdateDurabilitySlider(durability, maxDurability);

        if(Depth != null) { Depth.text = "0"; }
    }

    public void UpdateMoneyText(int money)
    {
        Money.text = money.ToString();
    }
    
    public void UpdateDepthText(int depth)
    {
        if (depth > maxDepth)
        {
            maxDepth = depth;
            Depth.text = maxDepth.ToString();
        }
    }

    public void UpdateDurabilityText(int durability)
    {
        Durability.text = "Durability: " + durability;
    }

    public void UpdateDurabilitySlider(int durability, int maxDurability)
    {
        durabilitySlider.value = (float)durability / (float)maxDurability;
    }
}

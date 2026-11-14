using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "PickaxeData", menuName = "Scriptable Objects/PickaxeData")]
public class PickaxeData : ScriptableObject
{
    public string Name;

    public int ID;
    public int MaxDurability;
    public int Damage;
    public int RangeOfDigging;
    [FormerlySerializedAs("RewardMultiplyer")]
    public float RewardMultiplier;
    public int Price;

    public Sprite icon;
    public Sprite EnchantmentOutline;

    public float GetRewardMultiplier(float upgradeBonus)
    {
        return Mathf.Max(1f, RewardMultiplier) + upgradeBonus;
    }
}

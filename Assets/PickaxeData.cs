using UnityEngine;

[CreateAssetMenu(fileName = "PickaxeData", menuName = "Scriptable Objects/PickaxeData")]
public class PickaxeData : ScriptableObject
{
    public string Name;

    public int ID;
    public int MaxDurability;
    public int Damage;
    public int RangeOfDigging;
    public float RewardMultiplyer;
    public int Price;

    public Sprite icon;
}

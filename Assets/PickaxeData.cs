using UnityEngine;

[CreateAssetMenu(fileName = "PickaxeData", menuName = "Scriptable Objects/PickaxeData")]
public class PickaxeData : ScriptableObject
{
    public string pickaxeName;

    public int maxDurability;
    public int damage;
    public int rangeOfDigging;
    public int rewardMultiplyer;

    public Sprite icon;
}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PickaxeDatabase", menuName = "Scriptable Objects/Pickaxe Database")]
public class PickaxeDatabase : ScriptableObject
{
    [SerializeField] private List<PickaxeData> allPickaxes = new();

    public IReadOnlyList<PickaxeData> AllPickaxes => allPickaxes;

    public PickaxeData GetById(int id)
    {
        return allPickaxes.Find(p => p.ID == id);
    }
}
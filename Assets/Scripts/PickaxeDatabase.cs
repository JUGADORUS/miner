using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PickaxeDatabase", menuName = "Scriptable Objects/Pickaxe Database")]
public class PickaxeDatabase : ScriptableObject
{
    [SerializeField] private List<PickaxeData> allPickaxes = new List<PickaxeData>();

    public IReadOnlyList<PickaxeData> AllPickaxes => allPickaxes;

    public List<PickaxeData> GetPickaxeDatas() 
    { 
        return allPickaxes; 
    }

    public PickaxeData GetById(string id)
    {
        return allPickaxes.Find(p => p.name == id);
    }

    public PickaxeData GetByIndex(int index)
    {
        if (index < 0 || index >= allPickaxes.Count) return null;
        return allPickaxes[index];
    }
}
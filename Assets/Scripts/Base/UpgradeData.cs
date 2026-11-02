using UnityEngine;

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Scriptable Objects/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Tooltip("Уникальный ID улучшения. Например: damage, durability, multiplier")]
    public string upgradeID;

    [Tooltip("Отображаемое название в UI. Например: 'Урон кирки'")]
    public string upgradeName;

    [Tooltip("Иконка этого улучшения для UI")]
    public Sprite icon;

    [Tooltip("Максимальный уровень, до которого можно прокачать")]
    public int maxLevel = 10;

    [Header("Настройки стоимости")]
    [Tooltip("Цена самого первого уровня (с 0 на 1)")]
    public int baseCost = 100;
    [Tooltip("Коэффициент удорожания. Каждый уровень цена умножается на это число")]
    public float costMultiplier = 1.5f;

    [Header("Настройки характеристик")]
    [Tooltip("Сколько добавляет ОДИН уровень прокачки к характеристике")]
    public float valuePerLevel = 2f;

    /// <summary>
    /// Метод автоматически рассчитает стоимость для любого уровня
    /// </summary>
    public int GetCostForLevel(int currentLevel)
    {
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel));
    }

    /// <summary>
    /// Метод автоматически рассчитает прибавку характеристики для любого уровня
    /// </summary>
    public float GetBonusForLevel(int currentLevel)
    {
        return currentLevel * valuePerLevel;
    }
}
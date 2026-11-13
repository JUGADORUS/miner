using UnityEngine;

public class Destruction : MonoBehaviour
{
    [Header("Спрайты разрушения")]
    [SerializeField] private Sprite[] destructionSprites;
    [SerializeField] private int spriteCount = 7;

    private SpriteRenderer spriteRenderer;
    private Block block;

    private float stepPercent;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        block = GetComponent<Block>();
        stepPercent = 100f / (spriteCount - 1);

        UpdateSprite(100f);
    }

    void Start()
    {

        // Подписываемся на событие получения урона (если хочешь)
        // Или вызываем из Block.TakeDamage напрямую
    }

    // Вызывается из Block.TakeDamage
    public void UpdateSprite(float healthPercent)
    {
        if (destructionSprites == null || destructionSprites.Length == 0)
        {
            Debug.LogWarning("Нет спрайтов разрушения у блока!");
            return;
        }

        int index = GetSpriteIndex(healthPercent);
        index = Mathf.Clamp(index, 0, destructionSprites.Length - 1);
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = destructionSprites[index];
        }
    }

    private int GetSpriteIndex(float healthPercent)
    {
        float percentDestroyed = 100f - healthPercent;
        int index = Mathf.FloorToInt(percentDestroyed / stepPercent);
        index = Mathf.Clamp(index, 0, destructionSprites.Length - 1);

        return index;
    }
}

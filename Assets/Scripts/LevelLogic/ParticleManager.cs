using UnityEngine;
using UnityEngine.InputSystem;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance; // Singleton для простого доступа

    [Header("Префабы эффектов")]
    [SerializeField] private ParticleSystem rockChunksPrefab;  // Осколки камня
    [SerializeField] private ParticleSystem stoneDustPrefab;   // Пыль
    [SerializeField] private ParticleSystem hitStonePrefab;   // Искры при ударе
    [SerializeField] private ParticleSystem breakFlashPrefab;  // Вспышка разрушения
    [SerializeField] private ParticleSystem pickaxeSparksPrefab; // Искры от кирки

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GameEvents.OnBlockDestroyed += SpawnBlockDestroyEffects;
        GameEvents.OnHitBlockWithDirectionPosition += SpawnHitStoneEffects;
        GameEvents.OnHitBlockWithDirectionPosition += SpawnPickaxeSparkEffects;
    }

    public void SpawnRockChunks(Vector2 position)
    {
        if (rockChunksPrefab == null) return;
        ParticleSystem chunks = Instantiate(rockChunksPrefab, position, Quaternion.identity);
        Destroy(chunks.gameObject, 5f);
    }

    public void SpawnBlockBreakEffects(Vector2 position)
    {
        SpawnRockChunks(position);
        SpawnBreakFlash(position);
    }

    private void SpawnBreakFlash(Vector2 position)
    {
        if (breakFlashPrefab == null) return;
        ParticleSystem flash = Instantiate(breakFlashPrefab, position, Quaternion.identity);
        flash.Play();
        Destroy(flash.gameObject, 0.5f);
    }

    private void SpawnBlockDestroyEffects(Block block, Vector2 position)
    {
        SpawnBlockBreakEffects(position);
    }

    private void SpawnPickaxeSparkEffects(Block block, Vector2 position, Vector2 direction)
    {
        if (hitStonePrefab == null) return;

        int x, y;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            x = Mathf.RoundToInt(direction.x);
            y = 0;
        }
        else
        {
            x = 0;
            y = Mathf.RoundToInt(direction.y);
        }

        Vector2 newPosition = new Vector2(0f, 0f);
        float offset = 0.3f;

        Quaternion targetRotation = Quaternion.Euler(0, 0, 0);

        if (x == 0 && y == 1)
        {
            targetRotation = Quaternion.Euler(-90, 0, 0);
            newPosition = position + new Vector2(0f, offset);
        }
        else if (x == -1 && y == 0)
        {
            targetRotation = Quaternion.Euler(0, -90, 0);
            newPosition = position + new Vector2(-offset, 0f);
        }
        else if (x == 1 && y == 0)
        {
            targetRotation = Quaternion.Euler(0, 90, 0);
            newPosition = position + new Vector2(offset, 0f);
        }
        else if (x == 0 && y == -1)
        {
            targetRotation = Quaternion.Euler(90, 90, 0);
            newPosition = position + new Vector2(offset, 0f);
        }

        ParticleSystem sparks = Instantiate(pickaxeSparksPrefab, newPosition, targetRotation);
        Destroy(sparks.gameObject, 5f);
    }

    private void SpawnHitStoneEffects(Block block, Vector2 position, Vector2 direction)
    {
        if (hitStonePrefab == null) return;

        int x, y;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            x = Mathf.RoundToInt(direction.x);
            y = 0;
        }
        else
        {
            x = 0;
            y = Mathf.RoundToInt(direction.y);
        }

        Vector2 newPosition = new Vector2(0f, 0f);
        float offset = 0.3f;

        Quaternion targetRotation = Quaternion.Euler(0, 0, 0);

        if (x == 0 && y == 1)
        {
            targetRotation = Quaternion.Euler(-90, 0, 0);
            newPosition = position + new Vector2(0f, offset);
        }
        else if (x == -1 && y == 0)
        {
            targetRotation = Quaternion.Euler(0, -90, 0);
            newPosition = position + new Vector2(-offset, 0f);
        }
        else if (x == 1 && y == 0)
        {
            targetRotation = Quaternion.Euler(0, 90, 0);
            newPosition = position + new Vector2(offset, 0f);
        }
        else if (x == 0 && y == -1)
        {
            targetRotation = Quaternion.Euler(90, 90, 0);
            newPosition = position + new Vector2(offset, 0f);
        }
        else
        {
            Time.timeScale = 0.1f;
        }

        ParticleSystem sparks = Instantiate(hitStonePrefab, newPosition, targetRotation);
        Destroy(sparks.gameObject, 5f);
    }

    private void OnDestroy()
    {
        // Отписка (важно!)
        GameEvents.OnBlockDestroyed -= SpawnBlockDestroyEffects;
        GameEvents.OnHitBlockWithDirectionPosition -= SpawnHitStoneEffects;
        GameEvents.OnHitBlockWithDirectionPosition -= SpawnPickaxeSparkEffects;
    }
}
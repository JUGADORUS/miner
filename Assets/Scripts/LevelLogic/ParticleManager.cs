using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance; // Singleton для простого доступа

    [Header("Префабы эффектов")]
    [SerializeField] private ParticleSystem rockChunksPrefab;  // Осколки камня
    [SerializeField] private ParticleSystem stoneDustPrefab;   // Пыль
    [SerializeField] private ParticleSystem hitStonePrefab;   // Искры при ударе
    [SerializeField] private ParticleSystem breakFlashPrefab;  // Вспышка разрушения
    [SerializeField] private ParticleSystem pickaxeSparksPrefab; // Искры от кирки

    private const int MaxPooledPerEffect = 64;

    private readonly Dictionary<ParticleSystem, Stack<ParticleSystem>> _pools = new();
    private readonly List<ActiveEffect> _activeEffects = new();

    private struct ActiveEffect
    {
        public ParticleSystem Prefab;
        public ParticleSystem Instance;
        public float ExpiresAt;
    }

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
        PlayEffect(rockChunksPrefab, position, Quaternion.identity, 5f);
    }

    public void SpawnBlockBreakEffects(Vector2 position)
    {
        SpawnRockChunks(position);
        SpawnBreakFlash(position);
    }

    private void SpawnBreakFlash(Vector2 position)
    {
        if (breakFlashPrefab == null) return;
        PlayEffect(breakFlashPrefab, position, Quaternion.identity, 0.5f);
    }

    private void SpawnBlockDestroyEffects(Block block, Vector2 position)
    {
        if (!block.IsDynamite) SpawnBlockBreakEffects(position);
    }

    private void SpawnPickaxeSparkEffects(Block block, Vector2 position, Vector2 direction)
    {
        if (pickaxeSparksPrefab == null) return;

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

        PlayEffect(pickaxeSparksPrefab, newPosition, targetRotation, 5f);
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

        PlayEffect(hitStonePrefab, newPosition, targetRotation, 5f);
    }

    private void PlayEffect(ParticleSystem prefab, Vector2 position, Quaternion rotation, float lifetime)
    {
        if (!_pools.TryGetValue(prefab, out Stack<ParticleSystem> pool))
        {
            pool = new Stack<ParticleSystem>();
            _pools.Add(prefab, pool);
        }

        ParticleSystem effect = pool.Count > 0 ? pool.Pop() : Instantiate(prefab, transform);
        effect.transform.SetPositionAndRotation(position, rotation);
        effect.gameObject.SetActive(true);
        effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        effect.Play(true);

        _activeEffects.Add(new ActiveEffect
        {
            Prefab = prefab,
            Instance = effect,
            ExpiresAt = Time.time + lifetime
        });
    }

    private void LateUpdate()
    {
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            ActiveEffect active = _activeEffects[i];
            ParticleSystem effect = active.Instance;
            if (effect != null && Time.time < active.ExpiresAt && effect.IsAlive(true))
                continue;

            int last = _activeEffects.Count - 1;
            _activeEffects[i] = _activeEffects[last];
            _activeEffects.RemoveAt(last);
            if (effect == null) continue;

            effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            effect.gameObject.SetActive(false);
            Stack<ParticleSystem> pool = _pools[active.Prefab];
            if (pool.Count < MaxPooledPerEffect) pool.Push(effect);
            else Destroy(effect.gameObject);
        }
    }

    private void OnDestroy()
    {
        // Отписка (важно!)
        GameEvents.OnBlockDestroyed -= SpawnBlockDestroyEffects;
        GameEvents.OnHitBlockWithDirectionPosition -= SpawnHitStoneEffects;
        GameEvents.OnHitBlockWithDirectionPosition -= SpawnPickaxeSparkEffects;
        if (Instance == this) Instance = null;
    }
}
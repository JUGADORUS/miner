using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
    public GameObject[] blockPrefabs;
    public float blockSize = 1f;

    public Transform player;
    public Camera mainCamera;

    public int width = 20;
    public int heightBelow = 15;
    public int heightAbove = 3;
    public int extraBuffer = 5;

    public bool isBackground;

    [Header("Streaming")]
    [SerializeField, Min(0)] private int _unloadBuffer = 8;

    [Header("Background")]
    [SerializeField] private ChunkGenerator _foreground;
    [SerializeField, Range(0f, 1f)] private float _backgroundBrightness = 0.447f;

    [Header("Workbench (foreground)")]
    [SerializeField] private GameObject _workbenchPrefab;
    [SerializeField, Min(1)] private int _workbenchSpawnInterval = 80;

    [Header("Dynamite (foreground)")]
    [SerializeField] private GameObject _dynamitePrefab;
    [SerializeField, Min(1)] private int _dynamiteSpawnInterval = 120;

    [Header("Ores (foreground)")]
    [SerializeField] private OreSpawn[] _ores = System.Array.Empty<OreSpawn>();

    [Header("Boss walls (foreground)")]
    [SerializeField] private Block _bossWallPrefab;

    [Header("Depth scaling (foreground)")]
    [SerializeField, Min(1)] private int _depthBandSize = 12;
    [SerializeField, Min(0)] private int _healthPerBand = 2;
    [SerializeField, Min(1)] private int _bandsPerWearIncrease = 2;

    [System.Serializable]
    private struct OreSpawn
    {
        public GameObject Prefab;
        [Range(0f, 100f)] public float ChancePercent;
    }

    private const int MaxPooledPerPrefab = 256;
    private const int BossInterval = 60;
    private const int BossClearance = 4;

    private Vector2Int _startCell;
    private Vector2Int _lastPlayerCell;
    private RectInt _lastWindow;
    private bool _hasWindow;
    private readonly Dictionary<Vector2Int, GameObject> blocks = new();
    private readonly Dictionary<Vector2Int, CellState> _cells = new();
    private readonly Dictionary<GameObject, Stack<GameObject>> _pools = new();
    private readonly List<Vector2Int> _cellsToRelease = new();
    private readonly Dictionary<int, Block> _bossWalls = new();

    private struct CellState
    {
        public GameObject Prefab;
        public int? Health;
        public bool Destroyed;
    }

    private void Start()
    {
        _startCell = WorldToBlock(player.position);
        UpdateWorld();
    }

    private void Update()
    {
        UpdateWorld();
    }

    private void UpdateWorld()
    {
        Vector2Int playerCell = WorldToBlock(player.position);
        Vector2Int cameraMin = WorldToBlock(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)));
        Vector2Int cameraMax = WorldToBlock(mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0)));
        // Always cover the visible cells, even when the camera leads the player.
        int minX = Mathf.Min(cameraMin.x, Mathf.Max(cameraMin.x - extraBuffer, playerCell.x - width));
        int maxX = Mathf.Max(cameraMax.x, Mathf.Min(cameraMax.x + extraBuffer, playerCell.x + width));
        int minY = Mathf.Min(cameraMin.y, Mathf.Max(cameraMin.y - extraBuffer, playerCell.y - heightBelow));
        int maxY = cameraMax.y + extraBuffer;
        var window = new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
        if (_hasWindow && window == _lastWindow && playerCell == _lastPlayerCell)
            return;

        var retained = new RectInt(
            minX - _unloadBuffer, minY - _unloadBuffer,
            window.width + 2 * _unloadBuffer, window.height + 2 * _unloadBuffer);

        ReleaseBlocks(retained);
        if (!isBackground && _bossWallPrefab != null)
            UpdateBossWalls(window, retained);
        GenerateBlocks(window, playerCell);
        _hasWindow = true;
        _lastWindow = window;
        _lastPlayerCell = playerCell;
    }

    private void ReleaseBlocks(RectInt retained)
    {
        foreach (var entry in blocks)
        {
            if (entry.Value == null || !entry.Value.activeSelf || !retained.Contains(entry.Key))
                _cellsToRelease.Add(entry.Key);
        }

        foreach (Vector2Int cell in _cellsToRelease)
        {
            GameObject instance = blocks[cell];
            blocks.Remove(cell);
            CellState state = _cells[cell];
            if (instance == null)
            {
                state.Destroyed = true;
            }
            else
            {
                if (!isBackground)
                {
                    Block block = instance.GetComponent<Block>();
                    state.Destroyed = block.IsDestroyed;
                    state.Health = block.Health;
                }

                instance.SetActive(false);
                Stack<GameObject> pool = _pools[state.Prefab];
                if (pool.Count < MaxPooledPerPrefab) pool.Push(instance);
                else Destroy(instance);
            }

            _cells[cell] = state;
        }

        _cellsToRelease.Clear();
    }

    private void GenerateBlocks(RectInt window, Vector2Int playerCell)
    {
        int top = (isBackground ? playerCell.y : _startCell.y) + heightAbove;
        for (int x = window.xMin; x < window.xMax; x++)
        for (int y = window.yMin; y < window.yMax; y++)
        {
            var cell = new Vector2Int(x, y);
            LoadCell(cell, playerCell, top);
        }
    }

    private void LoadCell(Vector2Int cell, Vector2Int playerCell, int top)
    {
        if (blocks.ContainsKey(cell)) return;
        if (!isBackground && _bossWallPrefab != null && IsBossRowOrClearance(cell.y))
            return;

        if (!_cells.TryGetValue(cell, out CellState state))
        {
            if (cell.y > top) return;
            if (!isBackground && Mathf.Abs(cell.x - _startCell.x) <= 1 && Mathf.Abs(cell.y - _startCell.y) <= 2)
                return;
            if (isBackground && cell.x == playerCell.x && cell.y == playerCell.y - 1)
                return;

            state.Prefab = GetRandomBlockPrefab();
            if (state.Prefab == null) return;
            _cells.Add(cell, state);
        }

        if (!state.Destroyed)
            blocks.Add(cell, RentBlock(state, BlockToWorld(cell)));
    }

    private static bool IsBossRowOrClearance(int row)
    {
        int depth = -row;
        if (depth < BossInterval - BossClearance) return false;

        int offset = depth % BossInterval;
        return offset <= BossClearance || offset >= BossInterval - BossClearance;
    }

    private void UpdateBossWalls(RectInt window, RectInt retained)
    {
        foreach (var entry in _bossWalls)
        {
            if (entry.Value != null && (entry.Key < retained.yMin || entry.Key >= retained.yMax))
                entry.Value.gameObject.SetActive(false);
        }

        int firstTier = Mathf.Max(1, Mathf.CeilToInt(-(window.yMax - 1) / (float)BossInterval));
        int lastTier = -window.yMin / BossInterval;
        for (int tier = firstTier; tier <= lastTier; tier++)
        {
            int row = -tier * BossInterval;
            if (!_bossWalls.TryGetValue(row, out Block wall))
            {
                wall = Instantiate(_bossWallPrefab, transform);
                wall.ScaleBossWall(tier);
                _bossWalls.Add(row, wall);
            }
            // A destroyed wall stays recorded, so backtracking cannot recreate it.
            if (wall == null || wall.IsDestroyed) continue;

            var collider = wall.GetComponent<BoxCollider2D>();
            float centerX = (retained.xMin + retained.width * 0.5f) * blockSize;
            wall.transform.position = new Vector3(centerX, (row + 0.5f) * blockSize, 0f);
            wall.transform.localScale = Vector3.one * blockSize;
            Vector2 size = new Vector2(retained.width, 1f);
            collider.size = size;
            wall.GetComponent<SpriteRenderer>().size = size;
            wall.gameObject.SetActive(true);
        }
    }

    internal void Blast(Vector2Int center, int radius)
    {
        if (isBackground) return;

        for (int x = -radius; x <= radius; x++)
        for (int y = -radius; y <= radius; y++)
        {
            if (x * x + y * y > radius * radius) continue;

            BreakCell(center + new Vector2Int(x, y));
        }
    }

    internal void BreakLine(Vector2Int cell, Vector2 travel)
    {
        // Starting at the cell center, visit each cell entered by the ray in order.
        int stepX = travel.x < 0f ? -1 : 1;
        int stepY = travel.y < 0f ? -1 : 1;
        float intervalX = 1f / Mathf.Abs(travel.x);
        float intervalY = 1f / Mathf.Abs(travel.y);
        float nextX = 0.5f * intervalX;
        float nextY = 0.5f * intervalY;

        while (Mathf.Min(nextX, nextY) < 1f)
        {
            bool corner = Mathf.Approximately(nextX, nextY);
            bool crossX = corner || nextX < nextY;
            bool crossY = corner || nextY < nextX;
            if (crossX)
            {
                cell.x += stepX;
                nextX += intervalX;
            }
            if (crossY)
            {
                cell.y += stepY;
                nextY += intervalY;
            }
            BreakCell(cell);
        }
    }

    private void BreakCell(Vector2Int cell)
    {
        if (isBackground) return;

        LoadCell(cell, default, _startCell.y + heightAbove);
        Block block = GetBlockAt(cell);
        if (block != null && !block.IsDestroyed)
            block.TakeDamage(block.Health);
    }

    private GameObject RentBlock(CellState state, Vector3 position)
    {
        GameObject prefab = state.Prefab;
        if (!_pools.TryGetValue(prefab, out Stack<GameObject> pool))
        {
            pool = new Stack<GameObject>();
            _pools.Add(prefab, pool);
        }

        GameObject instance = pool.Count > 0
            ? pool.Pop()
            : isBackground
                ? CreateBackgroundTile(prefab, position)
                : Instantiate(prefab, position, Quaternion.identity, transform);

        instance.transform.position = position;
        if (!isBackground)
        {
            Block block = instance.GetComponent<Block>();
            block.PrepareForSpawn(prefab.GetComponent<Block>());
            ApplyDepthScaling(instance, position.y);
            block.RestoreHealth(state.Health);
        }

        instance.SetActive(true);
        return instance;
    }

    private GameObject CreateBackgroundTile(GameObject prefab, Vector3 position)
    {
        var tile = new GameObject(prefab.name, typeof(SpriteRenderer));
        tile.transform.SetParent(transform, false);
        tile.transform.position = position;
        tile.transform.localScale = prefab.transform.localScale;

        SpriteRenderer source = prefab.GetComponent<SpriteRenderer>();
        SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
        renderer.sprite = source.sprite;
        renderer.sharedMaterial = source.sharedMaterial;
        renderer.sortingLayerName = "Mine_Background";
        renderer.flipX = source.flipX;
        renderer.flipY = source.flipY;

        Color color = source.color;
        renderer.color = new Color(
            color.r * _backgroundBrightness,
            color.g * _backgroundBrightness,
            color.b * _backgroundBrightness,
            color.a);
        return tile;
    }

    private void ApplyDepthScaling(GameObject blockObject, float worldY)
    {
        int band = Mathf.FloorToInt(Mathf.Max(0f, -worldY) / Mathf.Max(1, _depthBandSize));
        if (band == 0 || !blockObject.TryGetComponent(out Block block)) return;

        block.ApplyDepthBonuses(
            band * _healthPerBand,
            band / Mathf.Max(1, _bandsPerWearIncrease));
    }

    private GameObject GetRandomBlockPrefab()
    {
        if (!isBackground && _workbenchPrefab != null &&
            Random.Range(0, _workbenchSpawnInterval) == 0)
        {
            return _workbenchPrefab;
        }

        if (!isBackground && _dynamitePrefab != null &&
            Random.Range(0, _dynamiteSpawnInterval) == 0)
        {
            return _dynamitePrefab;
        }

        if (!isBackground && _ores.Length > 0)
        {
            float roll = Random.Range(0f, 100f);
            foreach (OreSpawn ore in _ores)
            {
                if (ore.Prefab == null) continue;
                roll -= ore.ChancePercent;
                if (roll < 0f) return ore.Prefab;
            }
        }

        GameObject[] prefabs = isBackground && _foreground != null ? _foreground.blockPrefabs : blockPrefabs;
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogError("ChunkGenerator has no block prefabs.", this);
            return null;
        }

        return prefabs[Random.Range(0, prefabs.Length)];
    }

    public Block GetBlockAt(Vector2Int cell)
    {
        if (!blocks.TryGetValue(cell, out GameObject instance) || instance == null || !instance.activeInHierarchy)
            return null;

        return instance.GetComponent<Block>();
    }

    public Vector2Int WorldToBlock(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / blockSize),
            Mathf.FloorToInt(worldPos.y / blockSize));
    }

    internal Vector3 BlockToWorld(Vector2Int cell)
    {
        return new Vector3(
            (cell.x + 0.5f) * blockSize,
            (cell.y + 0.5f) * blockSize,
            0f);
    }
}

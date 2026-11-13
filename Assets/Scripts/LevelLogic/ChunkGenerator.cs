using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
    [Header("Настройки блоков")]
    public GameObject[] blockPrefabs;
    public float blockSize = 1f;

    public Transform player;
    public Camera mainCamera;

    private int lastMinX, lastMaxX, lastMinY, lastMaxY;

    public int width = 20;
    public int heightBelow = 15;
    public int heightAbove = 3;
    public int extraBuffer = 5;

    public bool isBackground;

    [Header("Workbench (foreground)")]
    [SerializeField] private GameObject _workbenchPrefab;
    [SerializeField, Min(1)] private int _workbenchSpawnInterval = 80;

    [Header("Depth scaling (foreground)")]
    [SerializeField, Min(1)] private int _depthBandSize = 12;
    [SerializeField, Min(0)] private int _healthPerBand = 2;
    [SerializeField, Min(1)] private int _bandsPerWearIncrease = 2;
    [SerializeField, Min(0)] private int _rewardPerBand = 1;

    private Vector2Int _startCell;
    private Dictionary<Vector2Int, GameObject> blocks = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        _startCell = WorldToBlock(player.position);
    }

    void Update()
    {
        if (Time.frameCount % 10 == 0)
        {
            UpdateWorld();
        }
    }

    void UpdateWorld()
    {
        Vector2Int playerBlockPos = WorldToBlock(player.position);

        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));

        Vector2Int camMin = WorldToBlock(bottomLeft);
        Vector2Int camMax = WorldToBlock(topRight);

        int minX = camMin.x - extraBuffer;
        int maxX = camMax.x + extraBuffer;
        int minY = camMin.y - extraBuffer;
        int maxY = camMax.y + extraBuffer;

        int playerY = playerBlockPos.y;
        int topCell = isBackground ? playerY : _startCell.y;
        maxY = Mathf.Min(maxY, topCell + heightAbove);
        minY = Mathf.Max(minY, playerY - heightBelow);

        minX = Mathf.Max(minX, playerBlockPos.x - width);
        maxX = Mathf.Min(maxX, playerBlockPos.x + width);

        //if (minX == lastMinX && maxX == lastMaxX && minY == lastMinY && maxY == lastMaxY)
        //    return;

        GenerateBlocks(minX, maxX, minY, maxY);

        lastMinX = minX;
        lastMaxX = maxX;
        lastMinY = minY;
        lastMaxY = maxY;
    }


    void GenerateBlocks(int minX, int maxX, int minY, int maxY)
    {
        Vector2Int playerPos = WorldToBlock(player.position);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                if (blocks.ContainsKey(pos))
                    continue;

                if (!isBackground && Mathf.Abs(pos.x - _startCell.x) <= 1 && Mathf.Abs(pos.y - _startCell.y) <= 2)
                    continue;

                if (isBackground && pos.x == playerPos.x && pos.y == playerPos.y - 1)
                    continue;

                Vector3 position = BlockToWorld(pos);
                GameObject newBlock = Instantiate(GetRandomBlockPrefab(), position, Quaternion.identity, transform);
                if (!isBackground)
                {
                    ApplyDepthScaling(newBlock, position.y);
                }

                blocks.Add(pos, newBlock);
            }
        }
    }

    private void ApplyDepthScaling(GameObject blockObject, float worldY)
    {
        int band = Mathf.FloorToInt(Mathf.Max(0f, -worldY) / Mathf.Max(1, _depthBandSize));
        if (band == 0 || !blockObject.TryGetComponent(out Block block)) return;

        block.ApplyDepthBonuses(
            band * _healthPerBand,
            band / Mathf.Max(1, _bandsPerWearIncrease),
            band * _rewardPerBand);
    }

    GameObject GetRandomBlockPrefab()
    {
        if (!isBackground && _workbenchPrefab != null &&
            Random.Range(0, _workbenchSpawnInterval) == 0)
        {
            return _workbenchPrefab;
        }

        if (blockPrefabs.Length == 0)
        {
            Debug.LogError("Нет префабов блоков! Добавьте их в массиве blockPrefabs.");
            return null;
        }
        return blockPrefabs[Random.Range(0, blockPrefabs.Length)];
    }

    public Block GetBlockAt(Vector2Int cell)
    {
        if (!blocks.TryGetValue(cell, out GameObject blockObject) || blockObject == null || !blockObject.activeInHierarchy)
        {
            return null;
        }

        return blockObject.GetComponent<Block>();
    }

    public Vector2Int WorldToBlock(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / blockSize),
            Mathf.FloorToInt(worldPos.y / blockSize)
        );
    }

    Vector3 BlockToWorld(Vector2Int blockPos)
    {
        return new Vector3(
            blockPos.x * blockSize + blockSize / 2f,
            blockPos.y * blockSize + blockSize / 2f,
            0f
        );
    }
}
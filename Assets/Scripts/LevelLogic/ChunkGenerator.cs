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


    private Dictionary<Vector2Int, GameObject> blocks = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
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
        maxY = Mathf.Min(maxY, playerY + heightAbove);
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

                if ((Mathf.Abs(pos.x - playerPos.x) <= 1 && Mathf.Abs(pos.y - playerPos.y) <= 2) && !isBackground)
                    continue;

                if (pos.x == playerPos.x && pos.y == playerPos.y - 1)
                    continue;

                GameObject newBlock = Instantiate(GetRandomBlockPrefab(), BlockToWorld(pos), Quaternion.identity, transform);
                blocks.Add(pos, newBlock);
            }
        }
    }

    GameObject GetRandomBlockPrefab()
    {
        if (blockPrefabs.Length == 0)
        {
            Debug.LogError("Нет префабов блоков! Добавьте их в массиве blockPrefabs.");
            return null;
        }
        return blockPrefabs[Random.Range(0, blockPrefabs.Length)];
    }

    Vector2Int WorldToBlock(Vector3 worldPos)
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
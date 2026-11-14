using System.Collections.Generic;
using UnityEngine;

internal sealed class RockFragmentMeshes
{
    private static readonly Vector2[][] Shapes =
    {
        new[]
        {
            new Vector2(-0.46f, -0.24f), new Vector2(-0.36f, 0.23f), new Vector2(-0.08f, 0.43f),
            new Vector2(0.32f, 0.31f), new Vector2(0.49f, 0.06f), new Vector2(0.15f, -0.42f)
        },
        new[]
        {
            new Vector2(-0.49f, -0.12f), new Vector2(-0.19f, 0.32f), new Vector2(0.19f, 0.2f),
            new Vector2(0.49f, -0.05f), new Vector2(0.27f, -0.22f), new Vector2(-0.25f, -0.3f)
        },
        new[]
        {
            new Vector2(-0.36f, -0.19f), new Vector2(-0.22f, 0.1f), new Vector2(0.1f, 0.47f),
            new Vector2(0.36f, 0.21f), new Vector2(0.24f, -0.4f), new Vector2(-0.12f, -0.33f)
        }
    };

    private static readonly Vector2[] SampleCenters =
    {
        new Vector2(0.27f, 0.29f), new Vector2(0.71f, 0.36f), new Vector2(0.49f, 0.72f)
    };

    private readonly Dictionary<Sprite, Mesh[]> _bySprite = new();
    private Mesh[] _fallback;

    public Mesh[] Get(Sprite sprite)
    {
        if (sprite == null) return _fallback ??= Create(Vector2.zero, Vector2.one);
        if (_bySprite.TryGetValue(sprite, out Mesh[] meshes)) return meshes;

        Vector2 min = Vector2.one;
        Vector2 max = Vector2.zero;
        foreach (Vector2 uv in sprite.uv)
        {
            min = Vector2.Min(min, uv);
            max = Vector2.Max(max, uv);
        }

        meshes = Create(min, max);
        _bySprite.Add(sprite, meshes);
        return meshes;
    }

    private static Mesh[] Create(Vector2 min, Vector2 max)
    {
        var meshes = new Mesh[Shapes.Length];
        for (int index = 0; index < meshes.Length; index++)
        {
            Vector2[] shape = Shapes[index];
            var vertices = new Vector3[shape.Length];
            var uv = new Vector2[shape.Length];
            var triangles = new int[(shape.Length - 2) * 3];
            for (int vertex = 0; vertex < shape.Length; vertex++)
            {
                vertices[vertex] = shape[vertex];
                Vector2 sample = SampleCenters[index] + shape[vertex] * 0.44f;
                uv[vertex] = min + (max - min) * sample;
            }

            for (int triangle = 0; triangle < shape.Length - 2; triangle++)
            {
                triangles[triangle * 3] = 0;
                triangles[triangle * 3 + 1] = triangle + 1;
                triangles[triangle * 3 + 2] = triangle + 2;
            }

            var mesh = new Mesh { name = "Rock fragment" };
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            meshes[index] = mesh;
        }
        return meshes;
    }

    public void Release()
    {
        foreach (Mesh[] meshes in _bySprite.Values) DestroyMeshes(meshes);
        if (_fallback != null) DestroyMeshes(_fallback);
    }

    private static void DestroyMeshes(Mesh[] meshes)
    {
        foreach (Mesh mesh in meshes) Object.Destroy(mesh);
    }
}

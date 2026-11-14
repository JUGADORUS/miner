using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DynamiteExplosionFeedback : MonoBehaviour
{
    [SerializeField] private GameManager _run;
    [SerializeField] private Material _material;

    private const int SparkCount = 12;
    private const int RingSegments = 40;
    private const int PoolCapacity = 8;
    private const float Duration = 0.46f;
    private static readonly Color SparkCore = new Color(1f, 0.93f, 0.62f);
    private static readonly Color Ember = new Color(1f, 0.38f, 0.1f, 0f);
    private readonly List<Burst> _active = new();
    private readonly Stack<Burst> _pool = new();

    private sealed class Burst
    {
        public GameObject Root;
        public Light2D Flash;
        public LineRenderer Ring;
        public readonly LineRenderer[] Sparks = new LineRenderer[SparkCount];
        public readonly Vector2[] Directions = new Vector2[SparkCount];
        public float Radius;
        public float Elapsed;
    }

    private void OnEnable() => _run.DynamiteExploded += Play;

    private void OnDisable()
    {
        _run.DynamiteExploded -= Play;
        foreach (Burst burst in _active) Release(burst);
        _active.Clear();
    }

    private void Play(Vector2 position, float radius)
    {
        Burst burst = _pool.Count > 0 ? _pool.Pop() : CreateBurst();
        burst.Root.transform.position = position;
        burst.Radius = radius;
        burst.Elapsed = 0f;
        burst.Flash.pointLightOuterRadius = radius * 1.15f;
        UpdateBurst(burst);
        burst.Root.SetActive(true);
        _active.Add(burst);
    }

    private void LateUpdate()
    {
        if (Time.deltaTime <= 0f) return;

        for (int index = _active.Count - 1; index >= 0; index--)
        {
            Burst burst = _active[index];
            burst.Elapsed += Time.deltaTime;
            if (burst.Elapsed >= Duration)
            {
                _active.RemoveAt(index);
                Release(burst);
            }
            else
            {
                UpdateBurst(burst);
            }
        }
    }

    private static void UpdateBurst(Burst burst)
    {
        float progress = burst.Elapsed / Duration;
        float fade = 1f - progress;
        float expansion = 1f - fade * fade;
        float flash = 1f - Mathf.Clamp01(burst.Elapsed / 0.17f);
        burst.Flash.enabled = flash > 0f;
        burst.Flash.intensity = 2.6f * flash * flash;
        burst.Ring.transform.localScale = Vector3.one * (burst.Radius * Mathf.Lerp(0.12f, 1f, expansion));
        burst.Ring.widthMultiplier = Mathf.Lerp(0.13f, 0.025f, progress);
        var ringColor = Color.Lerp(SparkCore, Ember, progress);
        ringColor.a = fade * fade * 0.9f;
        burst.Ring.startColor = burst.Ring.endColor = ringColor;

        for (int index = 0; index < SparkCount; index++)
        {
            var spark = burst.Sparks[index];
            Vector2 direction = burst.Directions[index];
            float reach = 0.55f + index % 4 * 0.12f;
            Vector2 tip = direction * (burst.Radius * reach * expansion + 0.3f);
            tip.y -= progress * progress * 0.6f;
            spark.SetPosition(0, tip);
            spark.SetPosition(1, tip - direction * (fade * (0.28f + index % 3 * 0.12f)));
            spark.startColor = new Color(1f, 0.78f, 0.38f, fade);
            spark.endColor = Ember;
        }
    }

    private Burst CreateBurst()
    {
        var root = new GameObject("TNT Explosion");
        root.transform.SetParent(transform, false);
        root.SetActive(false);
        int layer = SortingLayer.NameToID("Mine_Playground");
        var burst = new Burst
        {
            Root = root,
            Flash = root.AddComponent<Light2D>(),
            Ring = AddLine(root.transform, "Shockwave", layer)
        };
        burst.Flash.lightType = Light2D.LightType.Point;
        burst.Flash.targetSortingLayers = new[] { layer };
        burst.Flash.color = new Color(1f, 0.52f, 0.19f);
        burst.Flash.pointLightInnerRadius = 0.3f;
        burst.Ring.loop = true;
        burst.Ring.positionCount = RingSegments;
        for (int index = 0; index < RingSegments; index++)
        {
            float angle = index * (2f * Mathf.PI / RingSegments);
            burst.Ring.SetPosition(index, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)));
        }

        for (int index = 0; index < SparkCount; index++)
        {
            float angle = (index + (index % 3) * 0.19f) * (2f * Mathf.PI / SparkCount);
            burst.Directions[index] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            var spark = AddLine(root.transform, "Explosion Spark", layer);
            spark.positionCount = 2;
            spark.startWidth = 0.09f;
            spark.endWidth = 0.02f;
            burst.Sparks[index] = spark;
        }
        return burst;
    }

    private LineRenderer AddLine(Transform parent, string objectName, int sortingLayer)
    {
        var obj = new GameObject(objectName, typeof(LineRenderer));
        obj.transform.SetParent(parent, false);
        var line = obj.GetComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.sharedMaterial = _material;
        line.sortingLayerID = sortingLayer;
        line.sortingOrder = 101;
        return line;
    }

    private void Release(Burst burst)
    {
        burst.Root.SetActive(false);
        if (_pool.Count < PoolCapacity) _pool.Push(burst);
        else Destroy(burst.Root);
    }
}

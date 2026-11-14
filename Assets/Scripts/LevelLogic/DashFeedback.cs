using UnityEngine;

public class DashFeedback : MonoBehaviour
{
    [SerializeField] private GameManager _run;
    [SerializeField] private SpriteRenderer _pickaxeIcon;
    [SerializeField] private Material _effectMaterial;

    private const float BurstDuration = 0.18f;
    private const float TrailEmitDuration = 0.16f;

    private TrailRenderer _trail;
    private LineRenderer[] _streaks;
    private Vector3 _origin;
    private Vector2 _direction;
    private float _elapsed;

    private void OnEnable() => _run.Dashed += Play;

    private void OnDisable()
    {
        _run.Dashed -= Play;
        if (_trail == null) return;

        _trail.emitting = false;
        _trail.Clear();
        _trail.enabled = false;
        foreach (LineRenderer streak in _streaks) streak.enabled = false;
    }

    private void Play(Vector2 direction)
    {
        if (_trail == null) CreateEffects();

        _origin = _pickaxeIcon.bounds.center;
        _direction = direction;
        _elapsed = 0f;
        _trail.Clear();
        _trail.enabled = true;
        _trail.emitting = true;
        _trail.AddPosition(_origin);
        UpdateBurst(0f);
    }

    private void LateUpdate()
    {
        if (_trail == null || !_trail.enabled) return;

        _elapsed += Time.deltaTime;
        _trail.emitting = _elapsed < TrailEmitDuration;
        UpdateBurst(Mathf.Clamp01(_elapsed / BurstDuration));
        if (_elapsed >= TrailEmitDuration + _trail.time) _trail.enabled = false;
    }

    private void UpdateBurst(float progress)
    {
        var normal = new Vector2(-_direction.y, _direction.x);
        for (int index = 0; index < _streaks.Length; index++)
        {
            var streak = _streaks[index];
            streak.enabled = progress < 1f;
            if (!streak.enabled) continue;

            float spread = index - (_streaks.Length - 1) * 0.5f;
            Vector2 ray = (_direction + normal * spread * 0.28f).normalized;
            Vector3 start = _origin + (Vector3)(normal * spread * 0.12f + ray * progress * 0.6f);
            streak.SetPosition(0, start);
            streak.SetPosition(1, start + (Vector3)(ray * (1f - progress) * 0.64f));
            streak.startColor = new Color(1f, 0.89f, 0.6f, 1f - progress);
            streak.endColor = new Color(1f, 0.53f, 0.25f, 0f);
        }
    }

    private void CreateEffects()
    {
        var trail = new GameObject("Dash Trail", typeof(TrailRenderer));
        trail.transform.SetParent(_pickaxeIcon.transform, false);
        trail.transform.position = _pickaxeIcon.bounds.center;
        _trail = trail.GetComponent<TrailRenderer>();
        _trail.sharedMaterial = _effectMaterial;
        _trail.sortingLayerID = _pickaxeIcon.sortingLayerID;
        _trail.sortingOrder = _pickaxeIcon.sortingOrder - 1;
        _trail.time = 0.19f;
        _trail.minVertexDistance = 0.06f;
        _trail.startWidth = 0.23f;
        _trail.endWidth = 0.02f;
        _trail.startColor = new Color(1f, 0.81f, 0.48f, 0.85f);
        _trail.endColor = new Color(1f, 0.42f, 0.22f, 0f);

        _streaks = new LineRenderer[5];
        for (int index = 0; index < _streaks.Length; index++)
        {
            var obj = new GameObject("Dash Burst", typeof(LineRenderer));
            obj.transform.SetParent(transform, false);
            var streak = obj.GetComponent<LineRenderer>();
            streak.sharedMaterial = _effectMaterial;
            streak.sortingLayerID = _pickaxeIcon.sortingLayerID;
            streak.sortingOrder = _pickaxeIcon.sortingOrder + 1;
            streak.positionCount = 2;
            streak.startWidth = 0.065f;
            streak.endWidth = 0.015f;
            _streaks[index] = streak;
        }
    }

    private void OnDestroy()
    {
        if (_trail != null) Destroy(_trail.gameObject);
    }
}

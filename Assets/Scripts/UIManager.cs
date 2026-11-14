using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _depthText;
    [SerializeField] private Slider _durabilitySlider;

    private const float DurabilityHitDuration = 0.24f;
    private const float DurabilityHitAngle = 3f;
    private const float DurabilityHitScale = 0.10f;

    private int _maxDepth;
    private int? _lastDurability;
    private Transform _durabilitySliderTransform;
    private Quaternion _durabilityRestRotation;
    private Vector3 _durabilityRestScale;
    private float _durabilityHitTimeLeft;

    private void Awake()
    {
        _durabilitySliderTransform = _durabilitySlider.transform;
        _durabilityRestRotation = _durabilitySliderTransform.localRotation;
        _durabilityRestScale = _durabilitySliderTransform.localScale;

        GameEvents.OnBlockDestroyed += HandleBlockDestroyed;
        GameEvents.OnDurabilityChanged += UpdateDurabilitySlider;
    }

    private void Start()
    {
        _depthText.text = "0";
    }

    private void OnDestroy()
    {
        GameEvents.OnBlockDestroyed -= HandleBlockDestroyed;
        GameEvents.OnDurabilityChanged -= UpdateDurabilitySlider;
    }

    private void OnDisable()
    {
        if (_durabilityHitTimeLeft > 0f)
        {
            ResetDurabilityHitFeedback();
        }
    }

    private void Update()
    {
        UpdateDurabilityHitFeedback(Time.unscaledDeltaTime);
    }

    private void HandleBlockDestroyed(Block block, Vector2 position)
    {
        int depth = Mathf.Abs((int)position.y);
        if (depth > _maxDepth)
        {
            _maxDepth = depth;
            _depthText.text = _maxDepth.ToString();
        }
    }

    private void UpdateDurabilitySlider(int durability, int maxDurability)
    {
        int previousDurability = _lastDurability ?? maxDurability;
        _lastDurability = durability;
        _durabilitySlider.value = maxDurability > 0 ? (float)durability / maxDurability : 0f;

        if (durability < previousDurability && isActiveAndEnabled)
        {
            _durabilityHitTimeLeft = DurabilityHitDuration;
        }
    }

    private void UpdateDurabilityHitFeedback(float deltaTime)
    {
        if (_durabilityHitTimeLeft <= 0f) return;

        _durabilityHitTimeLeft -= deltaTime;
        if (_durabilityHitTimeLeft <= 0f)
        {
            ResetDurabilityHitFeedback();
            return;
        }

        float remaining = _durabilityHitTimeLeft / DurabilityHitDuration;
        float progress = 1f - remaining;
        float falloff = remaining * remaining;
        float angle = Mathf.Sin(progress * Mathf.PI * 4f) * DurabilityHitAngle * falloff;
        float scalePulse = Mathf.Sin(progress * Mathf.PI * 3f) * DurabilityHitScale * falloff;

        _durabilitySliderTransform.localRotation = _durabilityRestRotation * Quaternion.Euler(0f, 0f, angle);
        _durabilitySliderTransform.localScale = Vector3.Scale(
            _durabilityRestScale, new Vector3(1f + scalePulse * 0.5f, 1f + scalePulse, 1f));
    }

    private void ResetDurabilityHitFeedback()
    {
        _durabilityHitTimeLeft = 0f;
        _durabilitySliderTransform.localRotation = _durabilityRestRotation;
        _durabilitySliderTransform.localScale = _durabilityRestScale;
    }
}

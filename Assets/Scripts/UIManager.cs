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
    private const float LowDurabilityThreshold = 0.5f;
    private const float VignettePulsePeriod = 1.25f;

    private int _maxDepth;
    private int? _lastDurability;
    private Transform _durabilitySliderTransform;
    private Quaternion _durabilityRestRotation;
    private Vector3 _durabilityRestScale;
    private float _durabilityHitTimeLeft;
    private RawImage _vignette;
    private float _vignetteStrength;

    private void Awake()
    {
        _durabilitySliderTransform = _durabilitySlider.transform;
        _durabilityRestRotation = _durabilitySliderTransform.localRotation;
        _durabilityRestScale = _durabilitySliderTransform.localScale;
        CreateVignette();

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
        if (_vignette.enabled) UpdateVignette();
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
        float remaining = maxDurability > 0 ? Mathf.Clamp01((float)durability / maxDurability) : 0f;
        _durabilitySlider.value = remaining;

        _vignetteStrength = maxDurability > 0 ? Mathf.InverseLerp(LowDurabilityThreshold, 0f, remaining) : 0f;
        _vignette.enabled = _vignetteStrength > 0f && _vignette.texture != null;
        if (_vignette.enabled) UpdateVignette();

        if (durability < previousDurability && isActiveAndEnabled)
        {
            _durabilityHitTimeLeft = DurabilityHitDuration;
        }
    }

    private void CreateVignette()
    {
        var overlay = new GameObject("Low Durability Vignette", typeof(RectTransform), typeof(RawImage));
        overlay.layer = _durabilitySlider.gameObject.layer;
        var rect = (RectTransform)overlay.transform;
        rect.SetParent(_durabilitySliderTransform.parent, false);
        rect.SetAsFirstSibling();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        _vignette = overlay.GetComponent<RawImage>();
        _vignette.texture = Resources.Load<Texture2D>("LowDurabilityVignette");
        _vignette.raycastTarget = false;
        _vignette.enabled = false;
    }

    private void UpdateVignette()
    {
        float phase = Time.time * (2f * Mathf.PI / VignettePulsePeriod);
        float pulse = Mathf.Lerp(0.52f, 1f, (1f + Mathf.Cos(phase)) * 0.5f);
        float opacity = Mathf.Lerp(0.07f, 0.68f, _vignetteStrength) * pulse;
        _vignette.color = new Color(1f, 0.055f, 0.075f, opacity);
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
        if (_durabilitySliderTransform == null) return;
        _durabilitySliderTransform.localRotation = _durabilityRestRotation;
        _durabilitySliderTransform.localScale = _durabilityRestScale;
    }
}

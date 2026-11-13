using UnityEngine;
using UnityEngine.UI;

public class LuckFeedback : MonoBehaviour
{
    [SerializeField] private GameManager _run;
    [SerializeField] private Sprite _vignette;
    [SerializeField] private Sprite _coin;

    private const int CoinCount = 18;
    private const float PulsePeriod = 1.1f;
    private readonly Image[] _coins = new Image[CoinCount];
    private RectTransform _overlay;
    private Image _edgeGlow;
    private float _elapsed;

    private void Update()
    {
        float remaining = _run.LuckTimeLeft;
        if (remaining <= 0f)
        {
            if (_overlay != null) _overlay.gameObject.SetActive(false);
            _elapsed = 0f;
            return;
        }

        if (_overlay == null) CreateOverlay();
        _overlay.gameObject.SetActive(true);
        _elapsed += Time.deltaTime;
        float fade = Mathf.Min(Mathf.Clamp01(_elapsed * 5f), Mathf.Clamp01(remaining * 2f));
        float pulse = 0.5f + 0.5f * Mathf.Cos(_elapsed * (2f * Mathf.PI / PulsePeriod));
        _edgeGlow.color = new Color(1f, 1f, 1f, fade * Mathf.Lerp(0.55f, 1f, pulse));

        Rect rect = _overlay.rect;
        for (int i = 0; i < _coins.Length; i++)
        {
            float progress = Mathf.Repeat(_elapsed * (0.37f + i % 4 * 0.055f) - i * 0.19f, 1f);
            float lane = Mathf.Repeat(i * 0.618f, 1f);
            float drift = Mathf.Sin(progress * 6f + i) * 13f;
            RectTransform coin = _coins[i].rectTransform;
            coin.anchoredPosition = new Vector2(
                (lane - 0.5f) * rect.width + drift,
                (0.5f - progress) * (rect.height + 32f));
            coin.localScale = new Vector3(0.25f + 0.75f * Mathf.Abs(Mathf.Cos(progress * 18f + i)), 1f, 1f);
            _coins[i].color = new Color(1f, 1f, 1f, fade * 0.75f);
        }
    }

    private void CreateOverlay()
    {
        _edgeGlow = AddImage(transform, "Luck Vignette", _vignette);
        _overlay = _edgeGlow.rectTransform;
        _overlay.SetAsFirstSibling();
        _overlay.anchorMin = Vector2.zero;
        _overlay.anchorMax = Vector2.one;
        _overlay.offsetMin = Vector2.zero;
        _overlay.offsetMax = Vector2.zero;

        for (int i = 0; i < _coins.Length; i++)
        {
            Image coin = AddImage(_overlay, "Falling Coin", _coin);
            coin.preserveAspect = true;
            coin.rectTransform.sizeDelta = Vector2.one * (12f + i % 4 * 3f);
            _coins[i] = coin;
        }
    }

    private static Image AddImage(Transform parent, string objectName, Sprite sprite)
    {
        var obj = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        obj.layer = parent.gameObject.layer;
        obj.transform.SetParent(parent, false);
        Image image = obj.GetComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;
        return image;
    }
}

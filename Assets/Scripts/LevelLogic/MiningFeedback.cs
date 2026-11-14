using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiningFeedback : MonoBehaviour
{
    [SerializeField] private Camera _worldCamera;
    [SerializeField] private RectTransform _durabilityBar;
    [SerializeField] private TMP_FontAsset _font;

    private const float PopupDuration = 0.65f;
    private readonly List<Popup> _activePopups = new();
    private readonly Stack<Popup> _popupPool = new();
    private RectTransform _layer;
    private int _regenerationPopupIndex;

    private sealed class Popup
    {
        public TextMeshProUGUI Text;
        public RectTransform Rect;
        public Color Color;
        public Vector2 Position;
        public bool AtDurabilityBar;
        public float Elapsed;
    }

    private void OnEnable()
    {
        GameEvents.OnCriticalHit += ShowCritical;
        GameEvents.OnDurabilityRegenerated += ShowRegeneration;
    }

    private void OnDisable()
    {
        GameEvents.OnCriticalHit -= ShowCritical;
        GameEvents.OnDurabilityRegenerated -= ShowRegeneration;
        foreach (Popup popup in _activePopups)
            ReleasePopup(popup);
        _activePopups.Clear();
    }

    private void OnDestroy()
    {
        if (_layer != null) Destroy(_layer.gameObject);
    }

    private void ShowCritical(Vector2 position, int damage) =>
        ShowPopup($"CRIT {damage}", new Color32(255, 196, 97, 255), position, false);

    private void ShowRegeneration(int amount)
    {
        int slot = _regenerationPopupIndex;
        _regenerationPopupIndex = (_regenerationPopupIndex + 1) % 10;
        var offset = new Vector2((slot % 5 - 2) * 32f, slot / 5 * 18f);
        ShowPopup($"+{amount}", new Color32(134, 255, 171, 255), offset, true);
    }

    private void ShowPopup(string message, Color color, Vector2 position, bool atDurabilityBar)
    {
        Popup popup = _popupPool.Count > 0 ? _popupPool.Pop() : CreatePopup();
        popup.Color = color;
        popup.Position = position;
        popup.AtDurabilityBar = atDurabilityBar;
        popup.Elapsed = 0f;
        popup.Text.fontSize = atDurabilityBar ? 19f : 24f;
        popup.Text.text = message;
        popup.Rect.SetAsLastSibling();
        UpdatePopup(popup, 0f);
        popup.Rect.gameObject.SetActive(true);
        _activePopups.Add(popup);
    }

    private Popup CreatePopup()
    {
        if (_layer == null)
        {
            var layer = new GameObject("Mining Feedback", typeof(RectTransform));
            layer.layer = gameObject.layer;
            _layer = (RectTransform)layer.transform;
            _layer.SetParent(transform, false);
            _layer.anchorMin = Vector2.zero;
            _layer.anchorMax = Vector2.one;
            _layer.offsetMin = _layer.offsetMax = Vector2.zero;
        }

        var popup = new GameObject("Mining Popup", typeof(RectTransform), typeof(TextMeshProUGUI));
        popup.layer = gameObject.layer;
        var text = popup.GetComponent<TextMeshProUGUI>();
        var rect = text.rectTransform;
        rect.SetParent(_layer, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(260f, 32f);
        text.font = _font;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;

        return new Popup { Text = text, Rect = rect };
    }

    private void LateUpdate()
    {
        float deltaTime = Time.unscaledDeltaTime;
        for (int index = _activePopups.Count - 1; index >= 0; index--)
        {
            Popup popup = _activePopups[index];
            popup.Elapsed += deltaTime;
            if (popup.Elapsed >= PopupDuration)
            {
                _activePopups.RemoveAt(index);
                ReleasePopup(popup);
            }
            else
            {
                UpdatePopup(popup, popup.Elapsed / PopupDuration);
            }
        }
    }

    private void UpdatePopup(Popup popup, float progress)
    {
        Vector2 origin;
        if (popup.AtDurabilityBar)
        {
            var bar = _durabilityBar.rect;
            origin = _layer.InverseTransformPoint(_durabilityBar.TransformPoint(new Vector2(bar.center.x, bar.yMin)));
            origin.x += popup.Position.x;
            origin.y -= 16f + popup.Position.y + 22f * progress;
        }
        else
        {
            Vector3 screen = _worldCamera.WorldToScreenPoint(popup.Position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_layer, screen, null, out origin);
            origin.x = Mathf.Clamp(origin.x, _layer.rect.xMin + 100f, _layer.rect.xMax - 100f);
            origin.y = Mathf.Clamp(origin.y + 24f + 34f * progress,
                _layer.rect.yMin + 26f, _layer.rect.yMax - 26f);
        }

        popup.Rect.anchoredPosition = origin;
        Color color = popup.Color;
        color.a = 1f - Mathf.InverseLerp(0.48f, 1f, progress);
        popup.Text.color = color;
    }

    private void ReleasePopup(Popup popup)
    {
        popup.Rect.gameObject.SetActive(false);
        _popupPool.Push(popup);
    }
}

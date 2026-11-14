using System.Collections;
using TMPro;
using UnityEngine;

public class MiningFeedback : MonoBehaviour
{
    [SerializeField] private Camera _worldCamera;
    [SerializeField] private RectTransform _durabilityBar;
    [SerializeField] private TMP_FontAsset _font;

    private const float PopupDuration = 0.65f;
    private RectTransform _layer;

    private void OnEnable()
    {
        GameEvents.OnCriticalHit += ShowCritical;
        GameEvents.OnDurabilityRegenerated += ShowRegeneration;
    }

    private void OnDisable()
    {
        GameEvents.OnCriticalHit -= ShowCritical;
        GameEvents.OnDurabilityRegenerated -= ShowRegeneration;
        StopAllCoroutines();
        if (_layer != null) Destroy(_layer.gameObject);
        _layer = null;
    }

    private void ShowCritical(Vector2 position, int damage) =>
        ShowPopup($"CRIT {damage}", new Color32(255, 196, 97, 255), position, false);

    private void ShowRegeneration(int amount) =>
        ShowPopup($"REGEN +{amount}", new Color32(134, 255, 171, 255), Vector2.zero, true);

    private void ShowPopup(string message, Color color, Vector2 position, bool atDurabilityBar)
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

        var popup = new GameObject(message, typeof(TextMeshProUGUI));
        popup.layer = gameObject.layer;
        var text = popup.GetComponent<TextMeshProUGUI>();
        var rect = text.rectTransform;
        rect.SetParent(_layer, false);
        rect.sizeDelta = new Vector2(260f, 32f);
        text.font = _font;
        text.fontSize = atDurabilityBar ? 19f : 24f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;
        text.text = message;
        text.raycastTarget = false;

        StartCoroutine(AnimatePopup(text, color, position, atDurabilityBar));
    }

    private IEnumerator AnimatePopup(TextMeshProUGUI text, Color color, Vector2 position, bool atDurabilityBar)
    {
        for (float elapsed = 0f; elapsed < PopupDuration; elapsed += Time.unscaledDeltaTime)
        {
            float progress = elapsed / PopupDuration;
            Vector2 origin;
            if (atDurabilityBar)
            {
                var bar = _durabilityBar.rect;
                origin = _layer.InverseTransformPoint(_durabilityBar.TransformPoint(new Vector2(bar.center.x, bar.yMin)));
                origin.y -= 18f + 10f * progress;
            }
            else
            {
                Vector3 screen = _worldCamera.WorldToScreenPoint(position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_layer, screen, null, out origin);
                origin.x = Mathf.Clamp(origin.x, _layer.rect.xMin + 100f, _layer.rect.xMax - 100f);
                origin.y = Mathf.Clamp(origin.y + 24f + 34f * progress,
                    _layer.rect.yMin + 26f, _layer.rect.yMax - 26f);
            }

            text.rectTransform.anchoredPosition = origin;
            color.a = 1f - Mathf.InverseLerp(0.48f, 1f, progress);
            text.color = color;
            yield return null;
        }

        Destroy(text.gameObject);
    }
}

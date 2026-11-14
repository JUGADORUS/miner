using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    [SerializeField] private ChunkGenerator _mine;
    [SerializeField] private TMP_FontAsset _font;

    private RectTransform _panel;
    private RectTransform _fill;
    private TextMeshProUGUI _healthLabel;
    private int _displayedHealth = -1;
    private int _displayedMaximum;

    private void LateUpdate()
    {
        Block boss = _mine.GetNearbyBoss();
        if (boss == null)
        {
            if (_panel != null) _panel.gameObject.SetActive(false);
            return;
        }

        if (_panel == null) CreatePanel();
        _panel.gameObject.SetActive(true);

        int health = boss.Health;
        int maximum = boss.MaximumHealth;
        if (health == _displayedHealth && maximum == _displayedMaximum) return;

        _displayedHealth = health;
        _displayedMaximum = maximum;
        _fill.anchorMax = new Vector2(Mathf.Clamp01((float)health / maximum), 1f);
        _healthLabel.SetText($"{health} / {maximum} HP");
    }

    private void CreatePanel()
    {
        _panel = AddImage(transform, "Boss Health", new Color32(27, 16, 20, 245));
        _panel.anchorMin = _panel.anchorMax = new Vector2(0.5f, 0f);
        _panel.pivot = new Vector2(0.5f, 0f);
        _panel.anchoredPosition = new Vector2(0f, 20f);
        _panel.sizeDelta = new Vector2(276f, 48f);

        var title = AddLabel("Boss Label", TextAlignmentOptions.MidlineLeft);
        title.text = "BOSS";
        title.color = new Color32(255, 174, 147, 255);
        SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(10f, -28f), new Vector2(70f, -4f));

        _healthLabel = AddLabel("Boss HP", TextAlignmentOptions.MidlineRight);
        _healthLabel.enableAutoSizing = true;
        _healthLabel.fontSizeMin = 10f;
        _healthLabel.fontSizeMax = 16f;
        SetRect(_healthLabel.rectTransform, new Vector2(0f, 1f), Vector2.one,
            new Vector2(70f, -28f), new Vector2(-10f, -4f));

        RectTransform track = AddImage(_panel, "Health Track", new Color32(70, 39, 48, 255));
        SetRect(track, Vector2.zero, new Vector2(1f, 0f),
            new Vector2(10f, 8f), new Vector2(-10f, 19f));
        _fill = AddImage(track, "Health Fill", new Color32(227, 66, 78, 255));
        SetRect(_fill, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
    }

    private TextMeshProUGUI AddLabel(string objectName, TextAlignmentOptions alignment)
    {
        var label = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        label.layer = gameObject.layer;
        label.transform.SetParent(_panel, false);
        var text = label.GetComponent<TextMeshProUGUI>();
        text.font = _font;
        text.fontSize = 16f;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        return text;
    }

    private static RectTransform AddImage(Transform parent, string objectName, Color color)
    {
        var obj = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        obj.layer = parent.gameObject.layer;
        obj.transform.SetParent(parent, false);
        var image = obj.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image.rectTransform;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}

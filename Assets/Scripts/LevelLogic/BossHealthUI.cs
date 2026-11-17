using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    [SerializeField] private ChunkGenerator _mine;
    [SerializeField] private TMP_FontAsset _font;

    private const float HitDuration = 0.28f;
    private const float HitAngle = 3f;

    private RectTransform _panel;
    private RectTransform _fill;
    private TextMeshProUGUI _title;
    private TextMeshProUGUI _healthLabel;
    private int _displayedHealth = -1;
    private int _displayedMaximum;
    private Block _displayedBoss;
    private float _hitElapsed = HitDuration;

    private void OnEnable()
    {
        MinerLocalization.Changed += RefreshLanguage;
        RefreshLanguage();
    }

    private void OnDisable() => MinerLocalization.Changed -= RefreshLanguage;

    private void RefreshLanguage()
    {
        if (_title == null)
            return;

        _title.text = MinerLocalization.Text("BOSS");
        _healthLabel.SetText(MinerLocalization.Format("{0} / {1} HP", _displayedHealth, _displayedMaximum));
    }

    private void LateUpdate()
    {
        Block boss = _mine.GetNearbyBoss();
        if (boss == null)
        {
            if (_panel != null) _panel.gameObject.SetActive(false);
            _displayedBoss = null;
            return;
        }

        if (_panel == null) CreatePanel();
        _panel.gameObject.SetActive(true);

        int health = boss.Health;
        int maximum = boss.MaximumHealth;
        if (boss != _displayedBoss)
        {
            _displayedBoss = boss;
            _hitElapsed = HitDuration;
            _panel.localRotation = Quaternion.identity;
        }
        else if (health < _displayedHealth)
        {
            _hitElapsed = 0f;
        }

        UpdateHitFeedback();
        if (health == _displayedHealth && maximum == _displayedMaximum) return;

        _displayedHealth = health;
        _displayedMaximum = maximum;
        _fill.anchorMax = new Vector2(Mathf.Clamp01((float)health / maximum), 1f);
        _healthLabel.SetText(MinerLocalization.Format("{0} / {1} HP", health, maximum));
    }

    private void UpdateHitFeedback()
    {
        if (_hitElapsed >= HitDuration) return;

        _hitElapsed = Mathf.Min(_hitElapsed + Time.unscaledDeltaTime, HitDuration);
        float progress = _hitElapsed / HitDuration;
        float fade = 1f - progress;
        float angle = Mathf.Sin(progress * Mathf.PI * 4f) * fade * HitAngle;
        _panel.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void CreatePanel()
    {
        _panel = AddImage(transform, "Boss Health", new Color32(27, 16, 20, 245));
        _panel.anchorMin = _panel.anchorMax = new Vector2(0.5f, 0f);
        _panel.pivot = new Vector2(0.5f, 0f);
        _panel.anchoredPosition = new Vector2(0f, 12f);
        _panel.sizeDelta = new Vector2(360f, 60f);

        _title = AddLabel("Boss Label", TextAlignmentOptions.MidlineLeft);
        _title.text = MinerLocalization.Text("BOSS");
        _title.enableAutoSizing = true;
        _title.fontSizeMin = 10f;
        _title.fontSizeMax = 20f;
        _title.color = new Color32(255, 174, 147, 255);
        SetRect(_title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(14f, -34f), new Vector2(90f, -6f));

        _healthLabel = AddLabel("Boss HP", TextAlignmentOptions.MidlineRight);
        _healthLabel.enableAutoSizing = true;
        _healthLabel.fontSizeMin = 10f;
        _healthLabel.fontSizeMax = 20f;
        SetRect(_healthLabel.rectTransform, new Vector2(0f, 1f), Vector2.one,
            new Vector2(90f, -34f), new Vector2(-14f, -6f));

        RectTransform track = AddImage(_panel, "Health Track", new Color32(70, 39, 48, 255));
        SetRect(track, Vector2.zero, new Vector2(1f, 0f),
            new Vector2(14f, 9f), new Vector2(-14f, 24f));
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
        text.fontSize = 20f;
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

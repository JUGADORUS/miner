using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(100)]
internal sealed class LandscapeUi : MonoBehaviour
{
    private const float TouchHeight = 44f;
    private const float Edge = 12f;
    private readonly List<Action> _restore = new();
    private readonly HashSet<RectTransform> _changedRects = new();
    private readonly Dictionary<Graphic, Vector4> _originalHits = new();
    private readonly Dictionary<Selectable, Graphic> _extraHits = new();
    private RectTransform _canvas;
    private float _scale;
    private float _deviceScale;
    private Vector2 _viewport;
    private bool _compact;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern float MinerCanvasCssHeight();
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        SceneManager.sceneLoaded -= AddToScene;
        SceneManager.sceneLoaded += AddToScene;
    }

    private static void AddToScene(Scene scene, LoadSceneMode mode)
    {
        if (scene.name is not ("Menu" or "Mine" or "BaseWorkshop")) return;
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var canvas in root.GetComponentsInChildren<Canvas>())
            {
                if (canvas.isRootCanvas && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    canvas.gameObject.AddComponent<LandscapeUi>();
            }
        }
    }

    private void Awake() => _canvas = (RectTransform)transform;

    private void LateUpdate()
    {
        float cssHeight = Screen.height;
#if UNITY_WEBGL && !UNITY_EDITOR
        float browserHeight = MinerCanvasCssHeight();
        if (browserHeight > 0f) cssHeight = browserHeight;
#endif
        if (Screen.height <= 0 || _canvas.rect.height <= 0) return;

        float deviceScale = Screen.height / cssHeight;
        var viewport = new Vector2(Screen.width / deviceScale, cssHeight);
        float scale = viewport.y / _canvas.rect.height;
        if (_viewport == viewport && Mathf.Approximately(_scale, scale)) return;

        foreach (Action restore in _restore) restore();
        _restore.Clear();
        _changedRects.Clear();
        foreach (var (graphic, padding) in _originalHits)
            if (graphic != null) graphic.raycastPadding = padding;
        _originalHits.Clear();
        _viewport = viewport;
        _scale = scale;
        _deviceScale = deviceScale;
        _compact = viewport.x >= viewport.y && viewport.y <= 540f;

        string scene = gameObject.scene.name;
        if (!_compact)
        {
            if (scene == "BaseWorkshop") KeepWorkshopHeaderVisible();
            return;
        }

        switch (scene)
        {
            case "Menu":
                LayoutMenu();
                LayoutSettings();
                FitCenteredChildren(Find("Developers"), new(384f, 232f));
                break;
            case "Mine":
                LayoutMine();
                LayoutSettings();
                break;
            case "BaseWorkshop":
                LayoutWorkshop();
                break;
        }

        Canvas.ForceUpdateCanvases();
        foreach (var control in GetComponentsInChildren<Selectable>(true))
            ExpandTouchArea(control);
    }

    internal static void FitNewItem(Selectable control)
    {
        if (control != null && control.GetComponentInParent<LandscapeUi>() is { } layout && layout._compact)
            layout.ExpandTouchArea(control);
    }

    private RectTransform Find(string path) => (RectTransform)transform.Find(path);

    private void LayoutMenu()
    {
        var buttons = Find("MainMenu/Buttons");
        float factor = Mathf.Clamp((_viewport.y - 67f) / buttons.rect.height, 0.45f, TouchHeight / 56f);
        Scale(buttons, factor / _scale);
        Pin(buttons, new(0f, 1f), new(0f, 1f), new(Edge, -60f));
        var title = Find("MainMenu/Title");
        Size(title, new(152f, 50f));
        Pin(title, new(0f, 1f), new(0f, 1f), new(Edge, -6f));
    }

    private void LayoutSettings()
    {
        var settings = Find("Settings");
        var designSize = new Vector2(640f, 416f);
        FitCenteredChildren(settings, designSize);

        var dropdown = settings.GetComponentInChildren<TMP_Dropdown>(true);
        var item = dropdown.template.GetComponentInChildren<Toggle>(true);
        float pixelsPerUnit = FitScale(designSize);
        var itemRect = (RectTransform)item.transform;
        Remember(dropdown.template);
        Remember(itemRect);
        itemRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, TouchHeight / pixelsPerUnit);
        var template = dropdown.template;
        template.anchorMin = new Vector2(template.anchorMin.x, 1f);
        template.anchorMax = new Vector2(template.anchorMax.x, 1f);
        template.pivot = new Vector2(template.pivot.x, 0f);
        template.anchoredPosition = new Vector2(template.anchoredPosition.x, 5f / pixelsPerUnit);
        template.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
            (Mathf.Min(3, dropdown.options.Count) * TouchHeight + 8f) / pixelsPerUnit);
    }

    private void LayoutMine()
    {
        var hud = Find("GameUI");
        if (hud == null) return;

        var pause = Find("GameUI/Pause");
        var dash = Find("GameUI/Dash Indicator");
        Size(pause, new(124f, TouchHeight));
        Pin(pause, Vector2.one, Vector2.one, new(-Edge, -8f));
        Size(dash, new(124f, 45f));
        Pin(dash, Vector2.one, Vector2.one, new(-Edge, -58f));
        FitText(dash.GetComponentInChildren<TMP_Text>(true), 14f);

        var baseButton = Find("GameUI/Go to base");
        Size(baseButton, new(124f, TouchHeight));
        Pin(baseButton, Vector2.zero, Vector2.zero, new(Edge, 8f));
        FitText(baseButton.GetComponentInChildren<TMP_Text>(true), 14f);
        var start = Find("GameUI/Start Prompt");
        Place(start, new(0f, -34f), new(300f, 38f));
        FitText(start.GetComponent<TMP_Text>(), 20f);
        var pointer = (RectTransform)start.Find("Tap Pointer");
        Size(pointer, new(29f, 29f));
        Pin(pointer, new(0.5f, 0f), new(0.5f, 0.5f), new(0f, -14f));

        var toggle = Find("GameUI/Run Shop Toggle");
        Size(toggle, new(160f, TouchHeight));
        Pin(toggle, new(1f, 0f), new(1f, 0f), new(-Edge, 8f));
        FitText(toggle.GetComponentInChildren<TMP_Text>(true), 15f);

        var shop = Find("GameUI/Run Shop Panel");
        float gap = 6f;
        float columnWidth = Mathf.Min(195f, (_viewport.x - Edge * 2f - gap * 3f) / 2f);
        var shopSize = new Vector2(2f * columnWidth + 3f * gap, 3f * TouchHeight + 4f * gap);
        Size(shop, shopSize);
        Pin(shop, new(1f, 0f), new(1f, 0f), new(-Edge, 58f));
        for (int i = 0; i < shop.childCount && i < 6; i++)
        {
            var button = (RectTransform)shop.GetChild(i);
            float x = (i % 2 == 0 ? -1f : 1f) * (columnWidth + gap) / 2f;
            float y = (1 - i / 2) * (TouchHeight + gap);
            Place(button, new(x, y), new(columnWidth, TouchHeight));
            if (button.GetComponentInChildren<TMP_Text>(true) is { } label)
                FitText(label, 14f);
        }

        FitPanel(Find("Pause Overlay/Pause Menu"));
        FitPanel(Find("Pause Overlay/Leave Run Confirmation"));
        var results = Find("GameOver/Game Over Overlay");
        FitCenteredChildren(results, new(440f, 460f));
        float bottom = -_viewport.y * 0.5f + 35f;
        PlaceResultButton("GoToBase", new(-66f, bottom), 126f);
        PlaceResultButton("PlayAgain", new(66f, bottom), 126f);
        PlaceResultButton("ReviveWithAd", new(0f, bottom + 49f), 258f);
    }

    private void PlaceResultButton(string name, Vector2 position, float width)
    {
        var button = Find("GameOver/Game Over Overlay/" + name);
        Scale(button, 1f);
        Place(button, position, new(width, TouchHeight));
    }

    private void LayoutWorkshop()
    {
        float width = _viewport.x - 2f * Edge;
        float height = _viewport.y - 96f;
        float centerY = -_viewport.y * 0.5f + 8f + height * 0.5f;
        var frame = Find("Workshop Frame");
        Place(frame, new(0f, centerY), new(width, height));
        Place(Find("Base Title"), new(-width * 0.5f + 103f, _viewport.y * 0.5f - 18f), new(206f, 32f));
        FitText(Find("Base Title").GetComponent<TMP_Text>(), 20f);
        var money = Find("Workshop Frame/MoneyFrame");
        Scale(money, 0.42f / _scale);
        Place(money, new(width * 0.5f - 65f, _viewport.y * 0.5f - centerY - 19f));
        FitText(money.GetComponentInChildren<TMP_Text>(true), 14f / money.localScale.x);

        var nav = Find("Menu");
        Place(nav, new(0f, _viewport.y * 0.5f - 58f), new(width - 8f, TouchHeight));
        foreach (RectTransform tab in nav)
            Remember(tab);
        var group = nav.GetComponent<HorizontalLayoutGroup>();
        var padding = group.padding;
        float spacing = group.spacing;
        bool controlW = group.childControlWidth, controlH = group.childControlHeight;
        bool expandW = group.childForceExpandWidth, expandH = group.childForceExpandHeight;
        _restore.Add(() =>
        {
            group.padding = padding; group.spacing = spacing;
            group.childControlWidth = controlW; group.childControlHeight = controlH;
            group.childForceExpandWidth = expandW; group.childForceExpandHeight = expandH;
        });
        group.padding = new RectOffset();
        group.spacing = 6f / _scale;
        group.childControlWidth = group.childControlHeight = true;
        group.childForceExpandWidth = group.childForceExpandHeight = true;
        foreach (var label in nav.GetComponentsInChildren<TMP_Text>(true))
            FitText(label, 14f);

        float half = width * 0.5f;
        float listScale = (half - 14f) / 704f;
        foreach (string path in new[] { "Inventory/Scroll View", "Forge/Scroll View" })
        {
            var list = Find(path);
            Place(list, new(-half * 0.5f, centerY), new Vector2(half - 14f, height - 18f) / (listScale / _scale));
            Scale(list, listScale / _scale);
        }

        var stats = Find("Statistics");
        Place(stats, new(half * 0.5f, centerY), new(half - 12f, height - 16f));
        var pickaxe = Find("Statistics/Pickaxe");
        Vector3 pickaxeOrigin = pickaxe.localPosition;
        Place(pickaxe, new(half * 0.5f - 38f, height * 0.5f - 34f), new(42f, 42f));
        if (pickaxe.GetComponent<UIAnimation>() is { } animation)
        {
            animation.SetOrigin(pickaxe.localPosition);
            _restore.Add(() => animation.SetOrigin(pickaxeOrigin));
        }
        var title = Find("Statistics/Text (TMP)").GetComponent<TMP_Text>();
        Place(title.rectTransform, new(-19f, height * 0.5f - 32f), new(half - 66f, 32f));
        FitText(title, 17f);
        float line = (height - 61f) / 7f;
        float top = height * 0.5f - 55f;
        foreach (string key in new[] { "Damage", "Durability", "RangeOfDigging", "RewardMultiplyer", "Regeneration", "Critical Chance", "Critical Damage" })
        {
            var label = Find("Statistics/" + key).GetComponent<TMP_Text>();
            Place(label.rectTransform, new(0f, top - line * 0.5f), new(half - 28f, line));
            FitText(label, 14f);
            top -= line;
        }

        var enchantTitle = Find("EnchantmentTable/Enchantment Table Title");
        Place(enchantTitle, new(-half * 0.5f, centerY + height * 0.5f - 25f), new(half - 24f, 32f));
        FitText(enchantTitle.GetComponent<TMP_Text>(), 17f);
        var enchantStatus = Find("EnchantmentTable/Equipped Enchantment");
        Place(enchantStatus, new(-half * 0.5f, centerY + 8f));
        Scale(enchantStatus, Mathf.Min(0.48f, (height - 92f) / 240f) / _scale);
        FitText(enchantStatus.GetComponent<TMP_Text>(), 13f / enchantStatus.localScale.x);
        var roll = Find("EnchantmentTable/Roll Enchantment");
        Place(roll, new(-half * 0.5f, centerY - height * 0.5f + 32f), new(half - 34f, TouchHeight));
        FitText(roll.GetComponentInChildren<TMP_Text>(true), 16f);
    }

    private void KeepWorkshopHeaderVisible()
    {
        var title = Find("Base Title");
        if (title.anchoredPosition.y + 54f <= _canvas.rect.yMax) return;
        float scale = Mathf.Clamp((_canvas.rect.height - 30f) / 960f, 0.3f, 1f);
        foreach (Transform child in _canvas)
        {
            if (child is not RectTransform rect || rect.name == "Workshop Cavern") continue;
            if (rect.anchorMin != rect.anchorMax)
            {
                foreach (Transform inner in rect)
                    if (inner is RectTransform fixedChild) ScaleCentered(fixedChild, scale);
                continue;
            }
            ScaleCentered(rect, scale);
        }
    }

    private void FitPanel(RectTransform panel)
    {
        if (panel != null)
            Scale(panel, FitScale(panel.rect.size) / _scale);
    }

    private void FitCenteredChildren(RectTransform container, Vector2 designSize)
    {
        if (container == null) return;
        float scale = FitScale(designSize) / _scale;
        foreach (Transform child in container)
            if (child is RectTransform rect && rect.anchorMin == rect.anchorMax)
                ScaleCentered(rect, scale);
    }

    private float FitScale(Vector2 designSize) => Mathf.Min(
        (_viewport.x - 2f * Edge) / designSize.x, (_viewport.y - 20f) / designSize.y, 1f);

    private void ScaleCentered(RectTransform rect, float scale)
    {
        Remember(rect);
        rect.localScale *= scale;
        rect.anchoredPosition *= scale;
    }

    private void FitText(TMP_Text label, float maxPixels)
    {
        bool auto = label.enableAutoSizing;
        float originalMin = label.fontSizeMin, originalMax = label.fontSizeMax, originalSize = label.fontSize;
        var wrap = label.textWrappingMode;
        _restore.Add(() =>
        {
            if (label == null) return;
            label.enableAutoSizing = auto; label.fontSize = originalSize;
            label.fontSizeMin = originalMin; label.fontSizeMax = originalMax; label.textWrappingMode = wrap;
        });
        label.enableAutoSizing = true;
        label.fontSizeMin = Mathf.Min(11f, maxPixels) / _scale;
        label.fontSizeMax = maxPixels / _scale;
        label.textWrappingMode = TextWrappingModes.NoWrap;
    }

    private void ExpandTouchArea(Selectable control)
    {
        if (control is Scrollbar || control.name == "DurabilitySlider") return;
        var rect = (RectTransform)control.transform;
        float pixelsPerLocalUnit = Mathf.Abs(rect.lossyScale.y) / _deviceScale;
        if (pixelsPerLocalUnit <= 0f) return;
        float target = control.GetComponentInParent<ScrollRect>(true) != null ? 40f : TouchHeight;
        float padding = (target / pixelsPerLocalUnit - rect.rect.height) * 0.5f;
        if (padding <= 0.1f) return;

        var hit = control.GetComponent<Graphic>();
        if (hit == null || _extraHits.ContainsKey(control))
        {
            if (!_extraHits.TryGetValue(control, out hit))
            {
                hit = control.gameObject.AddComponent<Image>();
                hit.color = Color.clear;
                _extraHits.Add(control, hit);
            }
            hit.enabled = true;
            _restore.Add(() => { if (hit != null) hit.enabled = false; });
        }

        if (_originalHits.ContainsKey(hit)) return;
        if (_originalHits.Count > 100)
        {
            var destroyed = new List<Graphic>();
            foreach (var graphic in _originalHits.Keys)
                if (graphic == null) destroyed.Add(graphic);
            foreach (var graphic in destroyed) _originalHits.Remove(graphic);
        }
        Vector4 original = hit.raycastPadding;
        _originalHits.Add(hit, original);
        hit.raycastPadding = new Vector4(original.x, Mathf.Min(original.y, -padding), original.z, Mathf.Min(original.w, -padding));
    }

    private void Place(RectTransform rect, Vector2 center, Vector2? size = null)
    {
        Pin(rect, Vector2.one * 0.5f, Vector2.one * 0.5f, center);
        if (size.HasValue) Size(rect, size.Value);
    }

    private void Pin(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position)
    {
        Remember(rect);
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position / _scale;
    }

    private void Size(RectTransform rect, Vector2 size)
    {
        Remember(rect);
        rect.sizeDelta = size / _scale;
    }

    private void Scale(RectTransform rect, float scale)
    {
        Remember(rect);
        rect.localScale = new Vector3(scale, scale, 1f);
    }

    private void Remember(RectTransform rect)
    {
        if (!_changedRects.Add(rect)) return;
        Vector2 anchorMin = rect.anchorMin, anchorMax = rect.anchorMax, pivot = rect.pivot;
        Vector2 size = rect.sizeDelta, position = rect.anchoredPosition;
        Vector3 scale = rect.localScale;
        _restore.Add(() =>
        {
            if (rect == null) return;
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.pivot = pivot;
            rect.sizeDelta = size; rect.anchoredPosition = position; rect.localScale = scale;
        });
    }
}

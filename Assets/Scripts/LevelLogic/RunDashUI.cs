using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RunDashUI : MonoBehaviour
{
    [SerializeField] private GameManager _run;
    [SerializeField] private Camera _worldCamera;
    [SerializeField] private TMP_FontAsset _font;
    [SerializeField] private Sprite _frame;

    private readonly List<RaycastResult> _pointerHits = new();
    private readonly Image[] _segments = new Image[GameManager.DashHitsRequired];
    private RectTransform _indicator;
    private TextMeshProUGUI _label;
    private int _shownCharge = -1;

    private void Awake()
    {
        var frame = AddImage(transform, "Dash Indicator");
        frame.sprite = _frame;
        _indicator = frame.rectTransform;
        _indicator.anchorMin = _indicator.anchorMax = Vector2.one;
        _indicator.anchoredPosition = new Vector2(-89f, -85f);
        _indicator.sizeDelta = new Vector2(146f, 52f);

        var title = new GameObject("Dash Status", typeof(RectTransform), typeof(TextMeshProUGUI));
        title.layer = gameObject.layer;
        title.transform.SetParent(_indicator, false);
        _label = title.GetComponent<TextMeshProUGUI>();
        _label.rectTransform.anchoredPosition = new Vector2(0f, 8f);
        _label.rectTransform.sizeDelta = new Vector2(134f, 22f);
        _label.font = _font;
        _label.fontSize = 15f;
        _label.alignment = TextAlignmentOptions.Center;
        _label.raycastTarget = false;

        for (int index = 0; index < _segments.Length; index++)
        {
            Image segment = AddImage(_indicator, "Charge Segment");
            segment.rectTransform.anchoredPosition = new Vector2((index - 1) * 32f, -12f);
            segment.rectTransform.sizeDelta = new Vector2(27f, 6f);
            _segments[index] = segment;
        }

        RefreshIndicator();
    }

    private void LateUpdate()
    {
        // Evaluate after pause and UI input have had a chance to handle this frame.
        if (Time.timeScale > 0f && TryGetPress(out Vector2 screenPoint) && !PressedControl(screenPoint))
        {
            var plane = new Plane(Vector3.forward, Vector3.zero);
            Ray ray = _worldCamera.ScreenPointToRay(screenPoint);
            if (plane.Raycast(ray, out float distance)) _run.TryDash(ray.GetPoint(distance));
        }

        RefreshIndicator();
    }

    private void RefreshIndicator()
    {
        _indicator.gameObject.SetActive(_run.CanBuyRunUpgrades);
        int charge = _run.DashChargeHits;
        if (_shownCharge == charge) return;

        _shownCharge = charge;
        bool ready = charge == _segments.Length;
        var chargedColor = new Color32(255, 188, 100, 255);
        _label.text = ready ? "Dash ready" : $"Dash {charge}/{_segments.Length}";
        _label.color = ready ? chargedColor : new Color32(235, 211, 190, 255);
        for (int index = 0; index < _segments.Length; index++)
            _segments[index].color = index < charge ? chargedColor : new Color32(77, 55, 66, 255);
    }

    private bool PressedControl(Vector2 screenPoint)
    {
        var events = EventSystem.current;
        if (events == null) return false;

        events.RaycastAll(new PointerEventData(events) { position = screenPoint }, _pointerHits);
        return _pointerHits.Exists(hit => hit.gameObject.GetComponentInParent<Selectable>() != null);
    }

    private static bool TryGetPress(out Vector2 screenPoint)
    {
        var touch = Touchscreen.current?.primaryTouch;
        if (touch != null && touch.press.wasPressedThisFrame)
        {
            screenPoint = touch.position.ReadValue();
            return true;
        }

        var mouse = Mouse.current;
        screenPoint = mouse != null ? mouse.position.ReadValue() : Vector2.zero;
        return mouse != null && mouse.leftButton.wasPressedThisFrame;
    }

    private static Image AddImage(Transform parent, string objectName)
    {
        var obj = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        obj.layer = parent.gameObject.layer;
        obj.transform.SetParent(parent, false);
        var image = obj.GetComponent<Image>();
        image.raycastTarget = false;
        return image;
    }
}

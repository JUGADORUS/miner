using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RunPauseMenu : MonoBehaviour
{
    [SerializeField] private RectTransform _gameUI;
    [SerializeField] private GameObject _settingsMenu;
    [SerializeField] private TMP_FontAsset _font;
    [SerializeField] private Sprite _panelFrame;
    [SerializeField] private Sprite _buttonSprite;
    [SerializeField] private Sprite _buttonHoverSprite;
    [SerializeField] private Sprite _buttonPressedSprite;

    private GameObject _overlay;
    private GameObject _menu;
    private GameObject _confirmation;
    private TextMeshProUGUI _warning;
    private Button _continueButton;
    private Button _cancelButton;
    private Button _pauseButton;
    private TextMeshProUGUI _startPrompt;
    private GameObject _startSurface;
    private GameManager _run;
    private CanvasGroup _gameControls;
    private bool _paused;
    private bool _ended;
    private float _previousTimeScale;

    private void Awake()
    {
        CreateMenu();
        GameEvents.OnSessionEnded += EndRun;
        GameEvents.OnSessionRevived += ReopenRunControls;
        MinerLocalization.Changed += RefreshWarning;
    }

    private void Start()
    {
        _run = GameManager.Instance;
        _run.RunStarted += HideStartPrompt;
        if (_run.HasStarted) HideStartPrompt();
        else Select(_startSurface);
    }

    private void HideStartPrompt()
    {
        _startPrompt.gameObject.SetActive(false);
        _startSurface.SetActive(false);
        Select(null);
    }

    private void OnDestroy()
    {
        GameEvents.OnSessionEnded -= EndRun;
        GameEvents.OnSessionRevived -= ReopenRunControls;
        MinerLocalization.Changed -= RefreshWarning;
        if (_run != null) _run.RunStarted -= HideStartPrompt;
        if (_paused) Time.timeScale = _previousTimeScale;
    }

    private void Update()
    {
        if (_startPrompt.isActiveAndEnabled && !_paused)
        {
            float pulse = 0.5f + 0.5f * Mathf.Cos(Time.unscaledTime * Mathf.PI);
            _startPrompt.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.95f, 1.08f, pulse);
        }

        if (_ended || Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;

        if (_settingsMenu.activeSelf) CloseSettings();
        else if (_confirmation.activeSelf) ShowMenu();
        else if (_paused) Resume();
        else Pause();
    }

    private void Pause()
    {
        if (_paused || _ended) return;
        _previousTimeScale = Time.timeScale;
        _paused = true;
        Time.timeScale = 0f;
        _gameControls.interactable = false;
        _overlay.SetActive(true);
        ShowMenu();
    }

    private void Resume()
    {
        if (!_paused) return;
        _settingsMenu.SetActive(false);
        _confirmation.SetActive(false);
        _overlay.SetActive(false);
        _paused = false;
        Time.timeScale = _previousTimeScale;
        _gameControls.interactable = true;
        Select(_ended ? null : _run.HasStarted ? _pauseButton.gameObject : _startSurface);
    }

    private void ShowMenu()
    {
        _confirmation.SetActive(false);
        _menu.SetActive(true);
        Select(_continueButton.gameObject);
    }

    private void OpenSettings()
    {
        _menu.SetActive(false);
        _settingsMenu.SetActive(true);
        Select(null);
    }

    public void CloseSettings()
    {
        _settingsMenu.SetActive(false);
        if (_paused) ShowMenu();
    }

    private void GoToBase()
    {
        if (!_run.HasStarted)
        {
            LeaveRun();
            return;
        }

        Pause();
        RefreshWarning();
        _menu.SetActive(false);
        _confirmation.SetActive(true);
        Select(_cancelButton.gameObject);
    }

    private void RefreshWarning()
    {
        _warning.text = MinerLocalization.Text("Go to base?\nYour earned coins will be saved.");
    }

    private void LeaveRun()
    {
        _run.BankRunReward();
        Resume();
        SceneManager.LoadScene("BaseWorkshop");
    }

    private void EndRun(int reward)
    {
        _ended = true;
        _pauseButton.gameObject.SetActive(false);
        Resume();
    }

    private void ReopenRunControls()
    {
        _ended = false;
        _pauseButton.gameObject.SetActive(true);
    }

    private void CreateMenu()
    {
        _gameControls = _gameUI.gameObject.AddComponent<CanvasGroup>();
        var surface = AddImage(_gameUI, "Start Surface", Color.clear);
        surface.rectTransform.SetAsFirstSibling();
        surface.rectTransform.anchorMin = Vector2.zero;
        surface.rectTransform.anchorMax = Vector2.one;
        surface.rectTransform.offsetMin = surface.rectTransform.offsetMax = Vector2.zero;
        var surfaceButton = surface.gameObject.AddComponent<Button>();
        surfaceButton.navigation = new Navigation { mode = Navigation.Mode.None };
        surfaceButton.transition = Selectable.Transition.None;
        surfaceButton.onClick.AddListener(() => _run.TryStartRun());
        _startSurface = surface.gameObject;
        _startPrompt = AddLabel(_gameUI, "Tap to start", new Vector2(0f, -54f), new Vector2(440f, 54f), 30f);
        _startPrompt.gameObject.name = "Start Prompt";
        AddTapPointer(_startPrompt.rectTransform);

        var baseButton = AddButton(_gameUI, "Go to base", Vector2.zero, new Vector2(184f, 52f), GoToBase);
        var baseRect = (RectTransform)baseButton.transform;
        baseRect.anchorMin = baseRect.anchorMax = baseRect.pivot = Vector2.zero;
        baseRect.anchoredPosition = new Vector2(16f, 18f);
        _pauseButton = AddButton(_gameUI, "Pause", Vector2.zero, new Vector2(184f, 52f), Pause);
        var pauseRect = (RectTransform)_pauseButton.transform;
        pauseRect.anchorMin = pauseRect.anchorMax = Vector2.one;
        pauseRect.pivot = Vector2.one;
        pauseRect.anchoredPosition = new Vector2(-16f, -14f);

        var overlay = AddImage(_settingsMenu.transform.parent, "Pause Overlay", new Color32(12, 8, 13, 180));
        var overlayRect = overlay.rectTransform;
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = overlayRect.offsetMax = Vector2.zero;
        overlayRect.SetSiblingIndex(_settingsMenu.transform.GetSiblingIndex());
        _overlay = overlay.gameObject;

        var menu = AddPanel(overlayRect, "Pause Menu", new Vector2(396f, 340f));
        _menu = menu.gameObject;
        AddLabel(menu, "Paused", new Vector2(0f, 106f), new Vector2(270f, 40f), 32);
        var size = new Vector2(198f, 50.4f);
        _continueButton = AddButton(menu, "Continue", new Vector2(0f, 49f), size, Resume);
        AddButton(menu, "Settings", new Vector2(0f, -3f), size, OpenSettings);
        AddButton(menu, "Go to base", new Vector2(0f, -55f), size, GoToBase);

        var confirmation = AddPanel(overlayRect, "Leave Run Confirmation", new Vector2(374f, 321f));
        _confirmation = confirmation.gameObject;
        AddLabel(confirmation, "Leave run?", new Vector2(0f, 79f), new Vector2(300f, 36f), 28);
        _warning = AddLabel(confirmation, "", new Vector2(0f, 14f), new Vector2(300f, 72f), 19);
        var choiceSize = new Vector2(135f, 34.4f);
        _cancelButton = AddButton(confirmation, "Cancel", new Vector2(-75f, -69f), choiceSize, ShowMenu);
        AddButton(confirmation, "Leave run", new Vector2(75f, -69f), choiceSize, LeaveRun);

        _confirmation.SetActive(false);
        _overlay.SetActive(false);
    }

    private static void AddTapPointer(RectTransform prompt)
    {
        var icon = new GameObject("Tap Pointer", typeof(RectTransform), typeof(RawImage));
        icon.layer = prompt.gameObject.layer;
        var rect = (RectTransform)icon.transform;
        rect.SetParent(prompt, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, -21f);
        rect.sizeDelta = new Vector2(42f, 42f);

        var image = icon.GetComponent<RawImage>();
        image.raycastTarget = false;
        image.texture = Resources.Load<Texture2D>("TapPointer");
        if (image.texture != null)
            image.texture.filterMode = FilterMode.Point;
    }

    private void Select(GameObject target)
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(target);
    }

    private RectTransform AddPanel(Transform parent, string panelName, Vector2 size)
    {
        var panel = AddImage(parent, panelName, Color.white);
        panel.sprite = _panelFrame;
        panel.preserveAspect = true;
        panel.rectTransform.sizeDelta = size;
        return panel.rectTransform;
    }

    private Button AddButton(Transform parent, string label, Vector2 position, Vector2 size,
        UnityEngine.Events.UnityAction action)
    {
        var image = AddImage(parent, label, Color.white);
        image.sprite = _buttonSprite;
        var rect = image.rectTransform;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.SpriteSwap;
        button.spriteState = new SpriteState
        {
            highlightedSprite = _buttonHoverSprite,
            selectedSprite = _buttonHoverSprite,
            pressedSprite = _buttonPressedSprite
        };
        button.onClick.AddListener(action);
        AddLabel(rect, label, Vector2.zero, size, size.y >= 52f ? 24f : 20f);
        return button;
    }

    private TextMeshProUGUI AddLabel(Transform parent, string text, Vector2 position, Vector2 size, float fontSize)
    {
        var obj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.layer = parent.gameObject.layer;
        obj.transform.SetParent(parent, false);
        var label = obj.GetComponent<TextMeshProUGUI>();
        label.rectTransform.anchoredPosition = position;
        label.rectTransform.sizeDelta = size;
        label.font = _font;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color32(255, 223, 177, 255);
        label.raycastTarget = false;
        label.text = text;
        return label;
    }

    private static Image AddImage(Transform parent, string objectName, Color color)
    {
        var obj = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        obj.layer = parent.gameObject.layer;
        obj.transform.SetParent(parent, false);
        var image = obj.GetComponent<Image>();
        image.color = color;
        return image;
    }
}

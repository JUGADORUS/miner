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
    private CanvasGroup _gameControls;
    private string _destination;
    private bool _paused;
    private bool _ended;
    private float _previousTimeScale;

    private void Awake()
    {
        CreateMenu();
        GameEvents.OnSessionEnded += EndRun;
    }

    private void OnDestroy()
    {
        GameEvents.OnSessionEnded -= EndRun;
        if (_paused) Time.timeScale = _previousTimeScale;
    }

    private void Update()
    {
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
        _destination = null;
        _paused = false;
        Time.timeScale = _previousTimeScale;
        _gameControls.interactable = true;
        Select(_ended ? null : _pauseButton.gameObject);
    }

    private void ShowMenu()
    {
        _destination = null;
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

    private void ConfirmExit(string sceneName, string destinationLabel)
    {
        _destination = sceneName;
        _warning.text = $"Go to {destinationLabel}?\nRun progress will not be saved.";
        _menu.SetActive(false);
        _confirmation.SetActive(true);
        Select(_cancelButton.gameObject);
    }

    private void LeaveRun()
    {
        if (!_paused || _destination == null) return;

        string sceneName = _destination;
        Resume();
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    private void EndRun(int reward)
    {
        _ended = true;
        _pauseButton.gameObject.SetActive(false);
        Resume();
    }

    private void CreateMenu()
    {
        _gameControls = _gameUI.gameObject.AddComponent<CanvasGroup>();
        _pauseButton = AddButton(_gameUI, "Pause", new Vector2(-69f, -36f), new Vector2(110f, 28f), Pause);
        var pauseRect = (RectTransform)_pauseButton.transform;
        pauseRect.anchorMin = pauseRect.anchorMax = Vector2.one;

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
        AddButton(menu, "Shop", new Vector2(0f, -55f), size, () => ConfirmExit("BaseWorkshop", "the shop"));
        AddButton(menu, "Main Menu", new Vector2(0f, -107f), size, () => ConfirmExit("Menu", "the main menu"));

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
        AddLabel(rect, label, Vector2.zero, size, size.y < 32f ? 16f : 20f);
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

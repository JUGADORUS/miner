using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class ButtonNotifier : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler,
    IPointerDownHandler, IPointerUpHandler, ISubmitHandler
{
    private Selectable _control;
    private Slider _slider;
    private bool _adjusting;
    private float _nextAdjustmentSound;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        SceneManager.sceneLoaded -= InstallScene;
        SceneManager.sceneLoaded += InstallScene;
    }

    private static void InstallScene(Scene scene, LoadSceneMode mode)
    {
        foreach (var root in scene.GetRootGameObjects())
            InstallIn(root.transform);
    }

    internal static void InstallIn(Transform root)
    {
        foreach (var control in root.GetComponentsInChildren<Selectable>(true))
        {
            if (control is not (Button or Toggle or TMP_Dropdown or Slider) || control.name == "DurabilitySlider")
                continue;

            if (!control.TryGetComponent<ButtonNotifier>(out var notifier))
                notifier = control.gameObject.AddComponent<ButtonNotifier>();

            // Shop rows can replace their button listeners while they are being populated.
            if (notifier.isActiveAndEnabled)
                notifier.BindButton();
        }
    }

    private void Awake()
    {
        _control = GetComponent<Selectable>();
        _slider = _control as Slider;
    }

    private void OnEnable()
    {
        BindButton();
        if (_slider != null)
            _slider.onValueChanged.AddListener(HandleSliderChanged);
    }

    private void OnDisable()
    {
        if (_control is Button button)
            button.onClick.RemoveListener(PlayClick);
        if (_slider != null)
            _slider.onValueChanged.RemoveListener(HandleSliderChanged);

        _adjusting = false;
    }

    private void BindButton()
    {
        if (_control is not Button button) return;
        button.onClick.RemoveListener(PlayClick);
        button.onClick.AddListener(PlayClick);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        bool touch = eventData is ExtendedPointerEventData pointer
            ? pointer.pointerType == UIPointerType.Touch
            : eventData.pointerId >= 0;
        if (touch || !CanInteract()) return;
        if (TryGetComponent<Graphic>(out var graphic) && graphic.color.a == 0f) return;

        GameEvents.OnButtonHovered?.Invoke(gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            PlayChoice();
    }

    public void OnSubmit(BaseEventData eventData) => PlayChoice();

    private void PlayChoice()
    {
        if (_control is (Toggle or TMP_Dropdown) && CanInteract())
            PlayClick();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_slider == null || eventData.button != PointerEventData.InputButton.Left || !CanInteract()) return;

        _adjusting = true;
        PlayAdjustment();
    }

    public void OnPointerUp(PointerEventData eventData) => _adjusting = false;

    private void HandleSliderChanged(float value)
    {
        if (_adjusting && Time.unscaledTime >= _nextAdjustmentSound)
            PlayAdjustment();
    }

    private void PlayAdjustment()
    {
        _nextAdjustmentSound = Time.unscaledTime + 0.12f;
        PlayClick();
    }

    private bool CanInteract() => _control != null && _control.isActiveAndEnabled && _control.IsInteractable();

    private void PlayClick() => GameEvents.OnButtonClicked?.Invoke(gameObject);
}

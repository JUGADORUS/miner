using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _moneyText;
    [SerializeField] private TextMeshProUGUI _depthText;
    [SerializeField] private Slider _durabilitySlider;
    [SerializeField] private GameObject _settingsMenu;

    private int _maxDepth;

    private void Awake()
    {
        GameEvents.OnMoneyChanged += UpdateMoneyText;
        GameEvents.OnBlockDestroyed += HandleBlockDestroyed;
        GameEvents.OnDurabilityChanged += UpdateDurabilitySlider;
    }

    private void Start()
    {
        UpdateMoneyText(PlayerPrefs.GetInt("PlayerMoney", 0));
        _depthText.text = "0";
    }

    private void OnDestroy()
    {
        GameEvents.OnMoneyChanged -= UpdateMoneyText;
        GameEvents.OnBlockDestroyed -= HandleBlockDestroyed;
        GameEvents.OnDurabilityChanged -= UpdateDurabilitySlider;
    }

    private void Update()
    {
        if (Keyboard.current == null) return;
        if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;

        if (_settingsMenu.activeSelf)
        {
            CloseSettings();
        }
        else
        {
            _settingsMenu.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void CloseSettings()
    {
        _settingsMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    private void UpdateMoneyText(int money)
    {
        _moneyText.text = money.ToString();
    }

    private void HandleBlockDestroyed(Block block, Vector2 position)
    {
        int depth = Mathf.Abs((int)position.y);
        if (depth > _maxDepth)
        {
            _maxDepth = depth;
            _depthText.text = _maxDepth.ToString();
        }
    }

    private void UpdateDurabilitySlider(int durability, int maxDurability)
    {
        _durabilitySlider.value = (float)durability / maxDurability;
    }
}
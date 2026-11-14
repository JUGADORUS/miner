using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Upgrade = GameManager.RunUpgrade;

public class RunShopUI : MonoBehaviour
{
    [SerializeField] private GameManager _run;
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _toggle;
    [SerializeField] private TextMeshProUGUI _balance;
    [SerializeField] private Button _damage;
    [SerializeField] private TextMeshProUGUI _damageLabel;
    [SerializeField] private Button _capacity;
    [SerializeField] private TextMeshProUGUI _capacityLabel;
    [SerializeField] private Button _repair;
    [SerializeField] private TextMeshProUGUI _repairLabel;

    private (Button Button, TextMeshProUGUI Label) _regeneration;
    private (Button Button, TextMeshProUGUI Label) _criticalChance;
    private (Button Button, TextMeshProUGUI Label) _criticalDamage;

    private void Awake()
    {
        _regeneration = AddOffer(Upgrade.Regeneration, 1);
        _criticalChance = AddOffer(Upgrade.CriticalChance, 2);
        _criticalDamage = AddOffer(Upgrade.CriticalDamage, 3);
    }

    private void Start()
    {
        _toggle.onClick.AddListener(Toggle);
        _damage.onClick.AddListener(() => _run.TryBuyRunUpgrade(Upgrade.Damage));
        _capacity.onClick.AddListener(() => _run.TryBuyRunUpgrade(Upgrade.Capacity));
        _repair.onClick.AddListener(() => _run.TryBuyRunUpgrade(Upgrade.Repair));
    }

    private void OnEnable()
    {
        _run.RunShopChanged += Refresh;
        GameEvents.OnDurabilityChanged += RefreshDurability;
        Refresh();
    }

    private void OnDisable()
    {
        _run.RunShopChanged -= Refresh;
        GameEvents.OnDurabilityChanged -= RefreshDurability;
    }

    private void Toggle()
    {
        _panel.SetActive(!_panel.activeSelf);
        Refresh();
    }

    private void RefreshDurability(int current, int maximum)
    {
        if (_panel.activeSelf)
            RefreshRepairOffer();
    }

    private void Refresh()
    {
        _balance.text = $"Run shop   {_run.RunCredits} RP";
        _toggle.interactable = _run.CanBuyRunUpgrades;
        if (!_toggle.interactable) _panel.SetActive(false);
        if (!_panel.activeSelf) return;

        SetOffer(_damage, _damageLabel, Upgrade.Damage, "+1 Damage");
        SetOffer(_capacity, _capacityLabel, Upgrade.Capacity, "+8 Max durability");
        RefreshRepairOffer();
        SetOffer(_regeneration.Button, _regeneration.Label, Upgrade.Regeneration,
            $"Regen +{GameManager.RunRegenerationBonus}/s");
        SetOffer(_criticalChance.Button, _criticalChance.Label, Upgrade.CriticalChance,
            $"Crit chance +{GameManager.RunCriticalChanceBonus:P0}");
        SetOffer(_criticalDamage.Button, _criticalDamage.Label, Upgrade.CriticalDamage,
            $"Crit damage +{GameManager.RunCriticalDamageBonus:0.##}x");
    }

    private void RefreshRepairOffer()
    {
        _repair.interactable = _run.CanBuyRunUpgrade(Upgrade.Repair);
        string label = _run.HasFullDurability
            ? "Full durability"
            : $"Repair +12   {_run.GetRunUpgradeCost(Upgrade.Repair)} RP";
        if (_repairLabel.text != label) _repairLabel.text = label;
    }

    private (Button, TextMeshProUGUI) AddOffer(Upgrade upgrade, int rowOffset)
    {
        Button button = Instantiate(_repair, _panel.transform);
        button.name = upgrade.ToString();
        button.onClick.AddListener(() => _run.TryBuyRunUpgrade(upgrade));
        var repairRect = (RectTransform)_repair.transform;
        var capacityRect = (RectTransform)_capacity.transform;
        var rect = (RectTransform)button.transform;
        Vector2 rowStep = repairRect.anchoredPosition - capacityRect.anchoredPosition;
        rect.anchoredPosition = repairRect.anchoredPosition + rowStep * rowOffset;

        var label = button.GetComponentInChildren<TextMeshProUGUI>(true);
        label.enableAutoSizing = true;
        label.fontSizeMin = 12f;
        label.fontSizeMax = _repairLabel.fontSize;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        return (button, label);
    }

    private void SetOffer(Button button, TextMeshProUGUI label, Upgrade upgrade, string description)
    {
        button.interactable = _run.CanBuyRunUpgrade(upgrade);
        label.text = $"{description}   {_run.GetRunUpgradeCost(upgrade)} RP";
    }
}

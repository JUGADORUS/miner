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
            _repair.interactable = _run.CanBuyRunUpgrade(Upgrade.Repair);
    }

    private void Refresh()
    {
        _balance.text = $"Run shop   {_run.RunCredits} RP";
        _toggle.interactable = _run.CanBuyRunUpgrades;
        if (!_toggle.interactable) _panel.SetActive(false);
        if (!_panel.activeSelf) return;

        SetOffer(_damage, _damageLabel, Upgrade.Damage, "+1 Damage");
        SetOffer(_capacity, _capacityLabel, Upgrade.Capacity, "+8 Max durability");
        SetOffer(_repair, _repairLabel, Upgrade.Repair, "Repair +12");
    }

    private void SetOffer(Button button, TextMeshProUGUI label, Upgrade upgrade, string description)
    {
        button.interactable = _run.CanBuyRunUpgrade(upgrade);
        label.text = $"{description}   {_run.GetRunUpgradeCost(upgrade)} RP";
    }
}

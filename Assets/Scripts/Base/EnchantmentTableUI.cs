using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnchantmentTableUI : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private PickaxeDatabase _pickaxes;
    [SerializeField] private TextMeshProUGUI _status;
    [SerializeField] private TextMeshProUGUI _price;
    [SerializeField] private Button _rollButton;

    private PickaxeData _pickaxe;
    private bool _revealing;

    private void OnEnable()
    {
        _pickaxe = _pickaxes.GetById(ShopUI.GetEquippedId());
        _rollButton.onClick.AddListener(Roll);
        GameEvents.OnMoneyChanged += RefreshPrice;
        ShowCurrent();
        RefreshPrice(_player.Money);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _revealing = false;
        _status.rectTransform.localScale = Vector3.one;
        _rollButton.onClick.RemoveListener(Roll);
        GameEvents.OnMoneyChanged -= RefreshPrice;
    }

    private void Roll()
    {
        if (_revealing || _pickaxe == null || !PickaxeEnchantments.TryRoll(_player, _pickaxe)) return;
        _revealing = true;
        RefreshPrice(_player.Money);
        StartCoroutine(Reveal());
    }

    private IEnumerator Reveal()
    {
        for (int tick = 0; tick < 6; tick++)
        {
            EnchantmentKind candidate = (EnchantmentKind)(tick % ((int)EnchantmentKind.Luck + 1));
            _status.text = $"{_pickaxe.Name}\n<color=#D9A8FF>{candidate}</color>";
            yield return new WaitForSecondsRealtime(0.05f + tick * 0.025f);
        }

        ShowCurrent();
        for (float elapsed = 0f; elapsed < 0.28f; elapsed += Time.unscaledDeltaTime)
        {
            float pulse = Mathf.Sin(Mathf.PI * elapsed / 0.28f);
            _status.rectTransform.localScale = Vector3.one * (1f + pulse * 0.12f);
            yield return null;
        }

        _status.rectTransform.localScale = Vector3.one;
        _revealing = false;
        RefreshPrice(_player.Money);
    }

    private void ShowCurrent()
    {
        if (_pickaxe == null)
        {
            _status.text = "Equip a pickaxe";
            return;
        }

        int level = PickaxeEnchantments.GetLevel(_pickaxe.ID);
        string rank = level == 3 ? "III" : level == 2 ? "II" : "I";
        EnchantmentKind kind = PickaxeEnchantments.GetKind(_pickaxe.ID);
        string color = kind switch
        {
            EnchantmentKind.Dynamite => "#FFAA73",
            EnchantmentKind.Luck => "#FFE08C",
            _ => "#9DDAFF"
        };
        string equipped = level == 0
            ? "No enchantment"
            : $"<color={color}>{kind} {rank}</color>";
        _status.text = $"{_pickaxe.Name}\n{equipped}";
    }

    private void RefreshPrice(int balance)
    {
        bool available = _pickaxe != null;
        int cost = available ? PickaxeEnchantments.GetCost(_pickaxe) : 0;
        _rollButton.interactable = available && !_revealing && balance >= cost;
        _price.text = _revealing ? "Rolling..." : available ? $"Roll  /  {cost} coins" : "Roll";
    }
}

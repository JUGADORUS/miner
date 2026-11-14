using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnchantmentTableUI : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private PickaxeDatabase _pickaxes;
    [SerializeField] private ShopPickaxeStatisticsPanel _statisticsPanel;
    [SerializeField] private TextMeshProUGUI _status;
    [SerializeField] private TextMeshProUGUI _price;
    [SerializeField] private Button _rollButton;

    private PickaxeData _pickaxe;
    private bool _revealing;
    private Image _preview;
    private Image _previewOutline;
    private Sprite _coloredOutline;

    private void OnEnable()
    {
        _pickaxe = _pickaxes.GetById(ShopUI.GetEquippedId());
        _statisticsPanel.UpdateInfo(_pickaxe);
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

    private void OnDestroy() => EnchantmentOutlineSprite.Release(_coloredOutline);

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
            EnchantmentKind candidate = (EnchantmentKind)(tick % PickaxeEnchantments.KindCount);
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
        RefreshPreview();
        if (_pickaxe == null)
        {
            _status.text = "Equip a pickaxe";
            return;
        }

        var equipped = new List<string>(PickaxeEnchantments.KindCount);
        for (int index = 0; index < PickaxeEnchantments.KindCount; index++)
        {
            var kind = (EnchantmentKind)index;
            int level = PickaxeEnchantments.GetLevel(_pickaxe.ID, kind);
            if (level > 0) equipped.Add(FormatEnchantment(kind, level));
        }

        string enchantments = equipped.Count == 0 ? "No enchantment" : string.Join("\n", equipped);
        _status.text = $"{_pickaxe.Name}\n{enchantments}";
    }

    private void RefreshPreview()
    {
        if (_preview == null)
        {
            _preview = CreatePreviewImage(_status.transform, "Pickaxe Preview");
            _preview.rectTransform.anchorMin = _preview.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            _preview.rectTransform.anchoredPosition = new Vector2(-60f, 0f);
            _preview.rectTransform.sizeDelta = new Vector2(128f, 128f);
            _previewOutline = CreatePreviewImage(_preview.transform, "Enchantment Outline");
        }

        EnchantmentOutlineSprite.Release(_coloredOutline);
        _coloredOutline = null;
        _preview.sprite = _pickaxe != null ? _pickaxe.icon : null;
        _preview.gameObject.SetActive(_preview.sprite != null);
        if (_preview.sprite != null)
        {
            _coloredOutline = EnchantmentOutlineSprite.Create(_pickaxe.EnchantmentOutline,
                EnchantmentOutlineSprite.GetEquippedColors(_pickaxe.ID));
        }

        _previewOutline.sprite = _coloredOutline;
        _previewOutline.enabled = _coloredOutline != null;
        if (_coloredOutline != null)
        {
            Vector2 size = _preview.sprite.rect.size;
            Vector2 scale = _coloredOutline.rect.size / size;
            _previewOutline.rectTransform.sizeDelta = _preview.rectTransform.sizeDelta * scale;
        }
    }

    private static Image CreatePreviewImage(Transform parent, string objectName)
    {
        var obj = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        obj.layer = parent.gameObject.layer;
        obj.transform.SetParent(parent, false);
        var image = obj.GetComponent<Image>();
        image.raycastTarget = false;
        image.preserveAspect = true;
        return image;
    }

    private static string FormatEnchantment(EnchantmentKind kind, int level)
    {
        string rank = level == 3 ? "III" : level == 2 ? "II" : "I";
        string color = ColorUtility.ToHtmlStringRGB(PickaxeEnchantments.GetColor(kind));
        return $"<color=#{color}>{kind} {rank}</color>";
    }

    private void RefreshPrice(int balance)
    {
        bool available = _pickaxe != null;
        int cost = available ? PickaxeEnchantments.GetCost(_pickaxe) : 0;
        _rollButton.interactable = available && !_revealing && balance >= cost;
        _price.text = _revealing ? "Rolling..." : available ? $"Roll  /  {cost} coins" : "Roll";
    }
}

using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PickaxeEnchantmentOutline : MonoBehaviour
{
    [SerializeField] private Pickaxe _pickaxe;
    [SerializeField] private Material _unlitSpriteMaterial;

    private Sprite _coloredOutline;

    private void Start()
    {
        PickaxeData data = _pickaxe.data;
        SpriteRenderer icon = _pickaxe.Icon;
        if (data == null || icon == null || icon.sprite == null || data.EnchantmentOutline == null) return;

        Color32[] colors = EnchantmentOutlineSprite.GetEquippedColors(data.ID);
        if (colors.Length == 0) return;

        var outline = new GameObject("Enchantment Outline", typeof(SpriteRenderer));
        outline.transform.SetParent(icon.transform, false);
        _coloredOutline = EnchantmentOutlineSprite.Create(data.EnchantmentOutline, colors);
        var renderer = outline.GetComponent<SpriteRenderer>();
        renderer.sprite = _coloredOutline;
        renderer.flipX = icon.flipX;
        renderer.flipY = icon.flipY;
        renderer.sharedMaterial = _unlitSpriteMaterial;
        renderer.sortingLayerID = icon.sortingLayerID;
        renderer.sortingOrder = icon.sortingOrder + 1;

        for (int index = 0; index < colors.Length; index++)
        {
            var glow = new GameObject("Enchantment Glow", typeof(Light2D));
            glow.transform.SetParent(outline.transform, false);
            float offset = (index - (colors.Length - 1) * 0.5f) * 0.48f;
            glow.transform.localPosition = new Vector3(
                icon.flipX ? -offset * 0.5f : offset * 0.5f, icon.flipY ? -offset : offset);

            var light = glow.GetComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.targetSortingLayers = new[] { icon.sortingLayerID };
            light.color = colors[index];
            light.intensity = 0.85f / colors.Length;
            light.pointLightInnerRadius = 0.15f;
            light.pointLightOuterRadius = 1.8f;
        }
    }

    private void OnDestroy() => EnchantmentOutlineSprite.Release(_coloredOutline);
}

using System.Collections.Generic;
using UnityEngine;

public class PickaxeEnchantmentOutline : MonoBehaviour
{
    [SerializeField] private Pickaxe _pickaxe;
    [SerializeField] private Material _unlitSpriteMaterial;

    private const int OutlineSamples = 16;

    private void Start()
    {
        SpriteRenderer icon = _pickaxe.Icon;
        if (_pickaxe.data == null || icon.sprite == null) return;

        var colors = new List<Color32>(PickaxeEnchantments.KindCount);
        for (int index = 0; index < PickaxeEnchantments.KindCount; index++)
        {
            var kind = (EnchantmentKind)index;
            if (PickaxeEnchantments.GetLevel(_pickaxe.data.ID, kind) > 0)
                colors.Add(PickaxeEnchantments.GetColor(kind));
        }

        if (colors.Count == 0) return;

        var outline = new GameObject("Enchantment Outline");
        outline.transform.SetParent(icon.transform, false);
        float radius = 1.25f / icon.sprite.pixelsPerUnit;
        int samples = OutlineSamples + OutlineSamples % 3;
        for (int index = 0; index < samples; index++)
        {
            float angle = index * (2f * Mathf.PI / samples);
            var part = new GameObject("Outline Segment", typeof(SpriteRenderer));
            part.transform.SetParent(outline.transform, false);
            part.transform.localPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            var renderer = part.GetComponent<SpriteRenderer>();
            renderer.sprite = icon.sprite;
            renderer.flipX = icon.flipX;
            renderer.flipY = icon.flipY;
            renderer.sharedMaterial = _unlitSpriteMaterial;
            renderer.sortingLayerID = icon.sortingLayerID;
            renderer.sortingOrder = icon.sortingOrder - 1;
            renderer.color = colors[index * colors.Count / samples];
        }
    }
}

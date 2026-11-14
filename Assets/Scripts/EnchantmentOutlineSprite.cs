using System.Collections.Generic;
using UnityEngine;

internal static class EnchantmentOutlineSprite
{
    internal static Color32[] GetEquippedColors(int pickaxeId)
    {
        var colors = new List<Color32>(PickaxeEnchantments.KindCount);
        for (int index = 0; index < PickaxeEnchantments.KindCount; index++)
        {
            var kind = (EnchantmentKind)index;
            if (PickaxeEnchantments.GetLevel(pickaxeId, kind) > 0)
                colors.Add(PickaxeEnchantments.GetColor(kind));
        }
        return colors.ToArray();
    }

    internal static Sprite Create(Sprite mask, Color32[] colors)
    {
        if (mask == null || colors.Length == 0) return null;

        int width = mask.texture.width;
        int height = mask.texture.height;
        Color32[] pixels = mask.texture.GetPixels32();
        int first = width + height;
        int last = 0;
        // Measure along the diagonal of the pickaxe to divide the outline into colored sections.
        for (int index = 0; index < pixels.Length; index++)
        {
            if (pixels[index].a == 0) continue;
            int diagonal = index % width + index / width;
            first = Mathf.Min(first, diagonal);
            last = Mathf.Max(last, diagonal);
        }

        for (int index = 0; index < pixels.Length; index++)
        {
            byte alpha = pixels[index].a;
            if (alpha == 0) continue;
            int diagonal = index % width + index / width;
            Color32 color = colors[(diagonal - first) * colors.Length / (last - first + 1)];
            color.a = alpha;
            pixels[index] = color;
        }

        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        var size = new Vector2(width, height);
        return Sprite.Create(texture, new Rect(Vector2.zero, size), mask.pivot / size,
            mask.pixelsPerUnit, 0, SpriteMeshType.FullRect);
    }

    internal static void Release(Sprite outline)
    {
        if (outline == null) return;
        Object.Destroy(outline.texture);
        Object.Destroy(outline);
    }
}

using UnityEngine;
using UnityEditor;

#nullable enable

public abstract class SpritedAssetEditor<TAsset> : Editor
    where TAsset: ISpritedAsset
{
    private Sprite? lastSprite;
    private Texture2D? lastPreview;

    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        Texture2D? result = null;
        if (target is TAsset { PreviewSprite: { } previewSprite })
        {
            result = TextureFromSprite(previewSprite);
        }
        return result ?? base.RenderStaticPreview(assetPath, subAssets, width, height);
    }

    private Texture2D? TextureFromSprite(Sprite sprite)
    {
        if (lastSprite == sprite && lastPreview != null)
        {
            return lastPreview;
        }
        lastPreview = null;
        lastSprite = null;

        var croppedTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        var pixels = sprite.texture.GetPixels((int)sprite.rect.x,
                                                (int)sprite.rect.y,
                                                (int)sprite.rect.width,
                                                (int)sprite.rect.height);
        var required = croppedTexture.width * croppedTexture.height;
        var provided = pixels.Length;
        if (provided < required)
        {
            Debug.LogWarning($"[ItemEditor] Invalid size: {provided} < {required}");
            return null;
        }
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();

        lastPreview = croppedTexture;
        lastSprite = sprite;
        return croppedTexture;
    }
}

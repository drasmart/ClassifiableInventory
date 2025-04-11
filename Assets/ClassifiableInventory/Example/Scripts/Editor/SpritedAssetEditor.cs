using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class SpritedAssetEditor<T> : Editor where T: SpritedAsset
{
    private Sprite lastSprite = null;
    private Texture2D lastPreview = null;

    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        var item = target as SpritedAsset;
        Texture2D result = null;
        var previewSprite = item?.previewSprite;
        if (previewSprite)
        {
            result = TextureFromSprite(previewSprite);
        }
        if (!result)
        {
            return base.RenderStaticPreview(assetPath, subAssets, width, height);
        }
        return result;
    }

    private Texture2D TextureFromSprite(Sprite sprite)
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
            Debug.LogWarning("[ItemEditor] Invalid size: " + provided.ToString() + " < " + required.ToString());
            return null;
        }
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();

        lastPreview = croppedTexture;
        lastSprite = sprite;
        return croppedTexture;
    }
}

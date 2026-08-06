using UnityEngine;
using UnityEditor;

public class SpriteImportSettings : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        TextureImporter textureImporter = (TextureImporter)assetImporter;
        
        // Only apply to sprites
        if (textureImporter.textureType == TextureImporterType.Sprite)
        {
            textureImporter.spriteMeshType = SpriteMeshType.FullRect;
            // Optional: also set other defaults
            // textureImporter.spritePixelsPerUnit = 100;
            // textureImporter.filterMode = FilterMode.Point;
        }
    }
}

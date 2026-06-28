using UnityEditor;
using UnityEngine;
using System.IO;

public static class TextureAtlasGenerator
{
    private static Texture2D CreateAtlas(Texture2D tex1, Texture2D tex2)
    {
        int height = Mathf.Max(tex1.height, tex2.height);
        int width1 = Mathf.RoundToInt(tex1.width * 0.5f);
        int width2 = Mathf.RoundToInt(tex2.width * 0.5f);
        
        Texture2D atlas = new Texture2D(width1 + width2, height, TextureFormat.RGBA32, false);
        
        Color[] pixels1 = GetCompressedPixels(tex1, width1, height);
        atlas.SetPixels(0, 0, width1, height, pixels1);

        Color[] pixels2 = GetCompressedPixels(tex2, width2, height);
        atlas.SetPixels(width1, 0, width2, height, pixels2);

        atlas.Apply();

        return atlas;
    }
    
    private static Color[] GetCompressedPixels(Texture2D source, int targetWidth, int targetHeight)
    {
        Color[] pixels = new Color[targetWidth * targetHeight];

        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                float u = (float)x / targetWidth;
                float v = (float)y / targetHeight;

                int sourceX = Mathf.Clamp(Mathf.FloorToInt(u * source.width), 0, source.width - 1);
                int sourceY = Mathf.Clamp(Mathf.FloorToInt(v * source.height), 0, source.height - 1);

                pixels[y * targetWidth + x] = source.GetPixel(sourceX, sourceY);
            }
        }
        return pixels;
    }

    public static Texture2D GenerateAndSave(Texture2D _texture1, Texture2D _texture2, string textureName, string savingPath)
    {
        Texture2D res = CreateAtlas(_texture1, _texture2);
        WriteTextureToDisk(res, textureName, savingPath);
        return res;
    }
    
    private static void WriteTextureToDisk(Texture2D texture, string textureName, string savingPath)
    {
        byte[] texData = texture.EncodeToPNG();
        string texExtension = ".png";
        File.WriteAllBytes(savingPath + textureName + texExtension, texData);
        AssetDatabase.Refresh();
        TextureImporter importer =
            (TextureImporter)AssetImporter.GetAtPath(savingPath + textureName + texExtension);
        importer.textureType = TextureImporterType.Default;
        importer.textureShape = TextureImporterShape.Texture2D;
        importer.sRGBTexture = true;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.mipmapEnabled = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
        AssetDatabase.Refresh();
    }
}
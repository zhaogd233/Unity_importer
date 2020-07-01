using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class ImageAssetImporter : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        TextureImporter textureImporter = assetImporter as TextureImporter;

        if (IsFirstImport(textureImporter))
        {
            TextureImporterSettings textureImportSetting = new TextureImporterSettings();
            textureImporter.ReadTextureSettings(textureImportSetting);

            //textureImportSetting.spriteMeshType = SpriteMeshType.Tight;
            textureImportSetting.spriteExtrude                      = 1;
            textureImportSetting.spriteGenerateFallbackPhysicsShape = false;
            textureImporter.spritePixelsPerUnit                     = 100;
            textureImporter.SetTextureSettings(textureImportSetting);

            textureImporter.isReadable    = false;

            TextureImporterPlatformSettings settings        = textureImporter.GetPlatformTextureSettings("iPhone");
            bool                            isPowerOfTwo    = IsPowerOfTwo(textureImporter);
            TextureImporterFormat           defaultAlpha    = isPowerOfTwo ? TextureImporterFormat.PVRTC_RGBA4 : TextureImporterFormat.ASTC_RGBA_4x4;
            TextureImporterFormat           defaultNotAlpha = isPowerOfTwo ? TextureImporterFormat.PVRTC_RGB4 : TextureImporterFormat.ASTC_RGB_6x6;
            settings.overridden = true;
            settings.format     = textureImporter.DoesSourceTextureHaveAlpha() ? defaultAlpha : defaultNotAlpha;
            textureImporter.SetPlatformTextureSettings(settings);

            settings                      = textureImporter.GetPlatformTextureSettings("Android");
            settings.overridden           = true;
            settings.allowsAlphaSplitting = false;
            bool divisible4 = IsDivisibleOf4(textureImporter);
            defaultAlpha    = divisible4 ? TextureImporterFormat.ETC2_RGBA8Crunched : TextureImporterFormat.ASTC_RGBA_4x4;
            defaultNotAlpha = isPowerOfTwo ? TextureImporterFormat.ETC_RGB4Crunched : divisible4? TextureImporterFormat.ETC2_RGBA8Crunched:TextureImporterFormat.ASTC_RGB_6x6;
            settings.format = textureImporter.DoesSourceTextureHaveAlpha() ? defaultAlpha : defaultNotAlpha;
            textureImporter.SetPlatformTextureSettings(settings);

            settings                      = textureImporter.GetPlatformTextureSettings("Windows");
            settings.overridden           = true;
            settings.allowsAlphaSplitting = false;
            defaultAlpha                  = TextureImporterFormat.DXT5Crunched;
            defaultNotAlpha               = TextureImporterFormat.DXT1Crunched;
            settings.format               = textureImporter.DoesSourceTextureHaveAlpha() ? defaultAlpha : defaultNotAlpha;
            textureImporter.SetPlatformTextureSettings(settings);

            //Assets/Works/Resources/UI/Sprite/DialogCommon/坐标ICON.png
            if (textureImporter != null && textureImporter.assetPath.StartsWith("Assets/Works/Resources/UI"))
            {
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.mipmapEnabled = false;
            }

            textureImporter.userData = "exportSuccess";
        }
    }

    //被4整除
        bool IsDivisibleOf4(TextureImporter importer)
        {
            (int width, int height) = GetTextureImporterSize(importer);
            return (width % 4 == 0 && height % 4 == 0);
        }

        //2的整数次幂
        bool IsPowerOfTwo(TextureImporter importer)
        {
            (int width, int height) = GetTextureImporterSize(importer);
            return (width == height) && (width > 0) && ((width & (width - 1)) == 0);
        }

        //贴图不存在、meta文件不存在、图片尺寸发生修改需要重新导入
        bool IsFirstImport(TextureImporter importer)
        {
            return string.IsNullOrEmpty(importer.userData);
        }

        //获取导入图片的宽高
        (int, int) GetTextureImporterSize(TextureImporter importer)
        {
            if (importer != null)
            {
                object[]   args = new object[2];
                MethodInfo mi   = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
                mi.Invoke(importer, args);
                return ((int)args[0], (int)args[1]);
            }
            return (0, 0);
        }
}

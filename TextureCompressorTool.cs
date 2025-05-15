using UnityEngine;
using UnityEditor;

public class SuperSafeTextureOptimizer : EditorWindow
{
    int selectedResolution = 1024;
    int compressionQuality = 100;
    bool enableCrunchedCompression = true;

    [MenuItem("Tools/Super Safe Texture Optimizer")]
    static void Init()
    {
        SuperSafeTextureOptimizer window = (SuperSafeTextureOptimizer)EditorWindow.GetWindow(typeof(SuperSafeTextureOptimizer));
        window.titleContent = new GUIContent("Super Safe Texture Optimizer");
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Texture Optimization Settings", EditorStyles.boldLabel);

        selectedResolution = EditorGUILayout.IntPopup("Max Resolution", selectedResolution,
            new string[] { "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192" },
            new int[] { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 });

        compressionQuality = EditorGUILayout.IntSlider("Compression Quality", compressionQuality, 0, 100);

        enableCrunchedCompression = EditorGUILayout.Toggle("Enable Crunched Compression (Only Default Textures)", enableCrunchedCompression);

        GUILayout.Space(10);

        if (GUILayout.Button("Apply Super Safe Optimization"))
        {
            if (EditorUtility.DisplayDialog("Confirm", "Apply Super Safe Optimization to Textures?", "Yes", "Cancel"))
            {
                ApplySettings();
            }
        }
    }

    void ApplySettings()
    {
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D");
        int totalTextures = textureGuids.Length;
        int processed = 0;

        foreach (string guid in textureGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null)
            {
                EditorUtility.DisplayProgressBar("Optimizing Textures", $"Processing {path}", (float)processed / totalTextures);

                if (IsSpecialTexture(importer))
                {
                    processed++;
                    continue; // Special textures رد میشن
                }

                importer.isReadable = false;
                importer.maxTextureSize = selectedResolution;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.compressionQuality = compressionQuality;

                if (importer.textureType == TextureImporterType.Default && enableCrunchedCompression)
                    importer.crunchedCompression = true;
                else
                    importer.crunchedCompression = false;

                // ❌ DefaultPlatform رو دیگه تغییر نمیدیم
                // فقط Standalone/Android/iPhone

                ApplyStandaloneSettings(importer);
                ApplyPlatformSettings(importer, "Android");
                ApplyPlatformSettings(importer, "iPhone");

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                processed++;
            }
        }

        EditorUtility.ClearProgressBar();
        Debug.Log($"✅ Super Safe Optimization completed: {processed} textures updated!");
    }

    bool IsSpecialTexture(TextureImporter importer)
    {
        return importer.textureType == TextureImporterType.Sprite ||
               importer.textureType == TextureImporterType.NormalMap ||
               importer.textureType == TextureImporterType.Lightmap ||
               importer.textureType == TextureImporterType.Cookie ||
               importer.textureType == TextureImporterType.SingleChannel ||
               importer.textureType == TextureImporterType.GUI;
    }

    void ApplyStandaloneSettings(TextureImporter importer)
    {
        TextureImporterPlatformSettings standaloneSettings = new TextureImporterPlatformSettings
        {
            name = "Standalone",
            overridden = true,
            maxTextureSize = selectedResolution,
            textureCompression = TextureImporterCompression.CompressedHQ,
            format = TextureImporterFormat.DXT5
        };

        importer.SetPlatformTextureSettings(standaloneSettings);
    }

    void ApplyPlatformSettings(TextureImporter importer, string platformName)
    {
        TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings
        {
            name = platformName,
            overridden = true,
            maxTextureSize = selectedResolution,
            textureCompression = TextureImporterCompression.CompressedHQ,
            format = TextureImporterFormat.Automatic
        };

        importer.SetPlatformTextureSettings(platformSettings);
    }
}

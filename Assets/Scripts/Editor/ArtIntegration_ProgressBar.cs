using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ToyRepairShop.EditorTools
{
    /// <summary>
    /// One-time pass: crops the bar+star artwork out of the user-supplied
    /// "Progress bar.png" reference (a full shelf-scene illustration, not a
    /// clean sprite), imports it as a UI sprite, and wires it into the
    /// Workshop scene's ProgressBar/Fill Image (replacing the flat green
    /// placeholder color) as a Filled/Horizontal sprite so it fills the
    /// same way the old placeholder did. Crop bounds were verified via
    /// ProgressBarCropPreview before this was written.
    /// </summary>
    public static class ArtIntegration_ProgressBar
    {
        private const string SourcePath = "Assets/Art/Progress bar.png";
        private const string OutputPath = "Assets/Art/ProgressBar_Fill.png";
        private const string WorkshopScenePath = "Assets/Scenes/Workshop.unity";

        [MenuItem("Tools/ToyRepairShop/Art/Reskin Progress Bar")]
        public static void Run()
        {
            CropAndImportSprite();
            WireIntoScene();
        }

        private static void CropAndImportSprite()
        {
            TextureImporter sourceImporter = (TextureImporter)AssetImporter.GetAtPath(SourcePath);
            bool wasReadable = sourceImporter.isReadable;
            if (!wasReadable)
            {
                sourceImporter.isReadable = true;
                sourceImporter.SaveAndReimport();
            }

            Texture2D source = AssetDatabase.LoadAssetAtPath<Texture2D>(SourcePath);

            int imgH = source.height;
            const int topDownTop = 370;
            const int topDownBottom = 530;
            const int left = 60;
            const int right = 1420;

            int cropY = imgH - topDownBottom;
            int cropHeight = topDownBottom - topDownTop;
            int cropWidth = right - left;

            Color[] pixels = source.GetPixels(left, cropY, cropWidth, cropHeight);
            Texture2D crop = new Texture2D(cropWidth, cropHeight, TextureFormat.RGBA32, false);
            crop.SetPixels(pixels);
            crop.Apply();
            File.WriteAllBytes(OutputPath, crop.EncodeToPNG());

            if (!wasReadable)
            {
                sourceImporter.isReadable = false;
                sourceImporter.SaveAndReimport();
            }

            AssetDatabase.ImportAsset(OutputPath);

            TextureImporter outputImporter = (TextureImporter)AssetImporter.GetAtPath(OutputPath);
            outputImporter.textureType = TextureImporterType.Sprite;
            outputImporter.spriteImportMode = SpriteImportMode.Single;
            outputImporter.alphaIsTransparency = true;
            outputImporter.mipmapEnabled = false;
            outputImporter.filterMode = FilterMode.Bilinear;
            outputImporter.SaveAndReimport();

            Debug.Log($"ArtIntegration_ProgressBar: cropped and imported {OutputPath} ({cropWidth}x{cropHeight}).");
        }

        private static void WireIntoScene()
        {
            Scene scene = EditorSceneManager.OpenScene(WorkshopScenePath, OpenSceneMode.Single);

            GameObject fillGO = GameObject.Find("Fill");
            if (fillGO == null)
            {
                Debug.LogError("ArtIntegration_ProgressBar: no 'Fill' found in Workshop scene.");
                return;
            }

            Image fillImage = fillGO.GetComponent<Image>();
            if (fillImage == null)
            {
                Debug.LogError("ArtIntegration_ProgressBar: 'Fill' has no Image component.");
                return;
            }

            Sprite fillSprite = AssetDatabase.LoadAssetAtPath<Sprite>(OutputPath);
            if (fillSprite == null)
            {
                Debug.LogError($"ArtIntegration_ProgressBar: could not load sprite at {OutputPath}.");
                return;
            }

            fillImage.sprite = fillSprite;
            fillImage.color = Color.white;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 0f;

            EditorUtility.SetDirty(fillImage);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Progress Bar Reskin", "Cropped the bar artwork and wired it into the Fill image.", "OK");
            Debug.Log("ArtIntegration_ProgressBar: done.");
        }
    }
}

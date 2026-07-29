using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ToyRepairShop.EditorTools
{
    /// <summary>
    /// One-time art pass: adds BG.png as a full-screen background image
    /// behind the Workshop scene's Canvas content (first sibling, so
    /// everything else draws on top of it).
    /// </summary>
    public static class ArtIntegration_WorkshopBackground
    {
        private const string ScenePath = "Assets/Scenes/Workshop.unity";
        private const string BackgroundSpritePath = "Assets/Art/BG.png";

        [MenuItem("Tools/ToyRepairShop/Art/Add Workshop Background")]
        public static void Run()
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(BackgroundSpritePath);
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }

            Sprite backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundSpritePath);
            if (backgroundSprite == null)
            {
                Debug.LogError($"ArtIntegration_WorkshopBackground: could not load sprite at '{BackgroundSpritePath}'.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            GameObject canvasGO = GameObject.Find("Canvas");
            if (canvasGO == null)
            {
                Debug.LogError("ArtIntegration_WorkshopBackground: no 'Canvas' GameObject in the open scene.");
                return;
            }

            Transform existing = canvasGO.transform.Find("BackgroundImage");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            GameObject bgGO = new GameObject("BackgroundImage", typeof(RectTransform));
            bgGO.transform.SetParent(canvasGO.transform, false);
            bgGO.transform.SetAsFirstSibling();

            RectTransform rect = bgGO.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = bgGO.AddComponent<Image>();
            image.sprite = backgroundSprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = false; // BG.png's aspect ratio is close enough to the 1080x1920 canvas to stretch cleanly
            image.raycastTarget = false;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Workshop Background", "Added BG.png as the Workshop scene's background.", "OK");
            Debug.Log("ArtIntegration_WorkshopBackground: done.");
        }
    }
}

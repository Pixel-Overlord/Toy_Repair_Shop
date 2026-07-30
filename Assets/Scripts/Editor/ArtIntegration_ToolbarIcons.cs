using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ToyRepairShop.EditorTools
{
    /// <summary>
    /// One-time art pass: adds an icon Image to each of the Workshop
    /// scene's 5 toolbar buttons (built by Stage5WorkshopSceneSetup),
    /// using sprites already sliced from Atlas_Tools_01 by
    /// ArtIntegration_ToysAndTools, and shrinks each button's text label
    /// to make room. Run the Toys/Tools slice tool first.
    /// </summary>
    public static class ArtIntegration_ToolbarIcons
    {
        private const string ScenePath = "Assets/Scenes/Workshop.unity";
        private const string ToolsAtlasPath = "Assets/Art/Atlas_Tools_01.png";

        private static readonly (string ButtonName, string SpriteName)[] ButtonIcons =
        {
            ("SpongeButton", "Sponge"),
            ("ClothButton", "Towel"),
            ("NeedleButton", "NeedleThread"),
            ("PaintRollerButton", "Paintbrush"),
            ("ScrewdriverButton", "Screwdriver"),
        };

        [MenuItem("Tools/ToyRepairShop/Art/Add Toolbar Icons")]
        public static void Run()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            int applied = 0;
            foreach ((string buttonName, string spriteName) in ButtonIcons)
            {
                GameObject buttonGO = GameObject.Find(buttonName);
                if (buttonGO == null)
                {
                    Debug.LogWarning($"ArtIntegration_ToolbarIcons: could not find '{buttonName}' in the open scene. Run the Stage 5 scene setup tool first.");
                    continue;
                }

                Sprite icon = AtlasGridSlicer.LoadSprite(ToolsAtlasPath, spriteName);
                if (icon == null)
                {
                    continue;
                }

                ApplyIcon(buttonGO, icon);
                applied++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog(
                "Toolbar Icons",
                $"Added icons to {applied}/{ButtonIcons.Length} toolbar buttons and saved the scene.",
                "OK");

            Debug.Log("ArtIntegration_ToolbarIcons: done.");
        }

        private static void ApplyIcon(GameObject buttonGO, Sprite icon)
        {
            Transform existingIcon = buttonGO.transform.Find("Icon");
            if (existingIcon != null)
            {
                Object.DestroyImmediate(existingIcon.gameObject);
            }

            // Shrink the existing text label to the bottom third of the button.
            Transform labelTransform = buttonGO.transform.Find("Label");
            if (labelTransform != null && labelTransform.GetComponent<RectTransform>() is RectTransform labelRect)
            {
                labelRect.anchorMin = new Vector2(0f, 0f);
                labelRect.anchorMax = new Vector2(1f, 0.32f);
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;

                if (labelTransform.GetComponent<TMPro.TMP_Text>() is TMPro.TMP_Text label)
                {
                    label.fontSize = 24;
                }
            }

            GameObject iconGO = new GameObject("Icon", typeof(RectTransform));
            iconGO.transform.SetParent(buttonGO.transform, false);
            iconGO.transform.SetAsFirstSibling();

            RectTransform iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.15f, 0.34f);
            iconRect.anchorMax = new Vector2(0.85f, 1f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            Image iconImage = iconGO.AddComponent<Image>();
            iconImage.sprite = icon;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
        }
    }
}

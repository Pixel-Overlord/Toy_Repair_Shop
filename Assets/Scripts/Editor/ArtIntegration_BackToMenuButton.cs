using System.Reflection;
using ToyRepairShop.Managers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ToyRepairShop.EditorTools
{
    /// <summary>
    /// One-time pass: adds a "Back to Menu" button to the Workshop scene's
    /// top HUD bar (next to the coin display) and wires it into
    /// WorkshopController's _backToMenuButton field. Same batch-mode-
    /// runnable, idempotent pattern as the other ArtIntegration_* tools.
    /// </summary>
    public static class ArtIntegration_BackToMenuButton
    {
        private const string WorkshopScenePath = "Assets/Scenes/Workshop.unity";
        private const string UiAtlasPath = "Assets/Art/Atlas_UI_01.png";

        [MenuItem("Tools/ToyRepairShop/Art/Add Back To Menu Button")]
        public static void Run()
        {
            Scene scene = EditorSceneManager.OpenScene(WorkshopScenePath, OpenSceneMode.Single);

            GameObject topGO = GameObject.Find("Top");
            if (topGO == null)
            {
                Debug.LogError("ArtIntegration_BackToMenuButton: no 'Top' found in Workshop scene.");
                return;
            }

            Transform existing = topGO.transform.Find("BackToMenuButton");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            // ToyNameArea occupies the full right half of the Top bar
            // (anchors 0.5-1) - shrink it to free up a corner for the
            // button instead of overlapping the toy name text.
            Transform toyNameArea = topGO.transform.Find("ToyNameArea");
            if (toyNameArea is RectTransform toyNameRect)
            {
                toyNameRect.anchorMax = new Vector2(0.86f, toyNameRect.anchorMax.y);
            }

            Image background = CreateImage(topGO.transform, "BackToMenuButton");
            RectTransform rect = background.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(90f, 90f);
            rect.anchoredPosition = new Vector2(-70f, 0f);

            background.sprite = AtlasGridSlicer.LoadSprite(UiAtlasPath, "Icon_Home");
            background.preserveAspect = true;

            Button button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;

            GameObject controllerGO = GameObject.Find("WorkshopController");
            if (controllerGO == null)
            {
                Debug.LogError("ArtIntegration_BackToMenuButton: no 'WorkshopController' found in Workshop scene.");
                return;
            }

            WorkshopController controller = controllerGO.GetComponent<WorkshopController>();
            if (controller == null)
            {
                Debug.LogError("ArtIntegration_BackToMenuButton: 'WorkshopController' GameObject has no WorkshopController component.");
                return;
            }

            SetPrivateField(controller, "_backToMenuButton", button);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Back To Menu Button", "Added the Back to Menu button and wired it into WorkshopController.", "OK");
            Debug.Log("ArtIntegration_BackToMenuButton: done.");
        }

        private static Image CreateImage(Transform parent, string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = Color.white;
            return image;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                Debug.LogError($"ArtIntegration_BackToMenuButton: field '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }

            field.SetValue(target, value);
        }
    }
}

using System.Reflection;
using ToyRepairShop.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ToyRepairShop.EditorTools
{
    /// <summary>
    /// One-time pass: adds the "what to do next" instruction text element
    /// to the Workshop scene's Bottom panel (in the gap between Toolbar
    /// and ProgressBar) and wires it into RepairHUDView's _instructionText
    /// field. Same batch-mode-runnable, idempotent pattern as the other
    /// ArtIntegration_* tools.
    /// </summary>
    public static class ArtIntegration_InteractionHint
    {
        private const string WorkshopScenePath = "Assets/Scenes/Workshop.unity";

        [MenuItem("Tools/ToyRepairShop/Art/Add Interaction Hint Text")]
        public static void Run()
        {
            Scene scene = EditorSceneManager.OpenScene(WorkshopScenePath, OpenSceneMode.Single);

            GameObject bottomGO = GameObject.Find("Bottom");
            if (bottomGO == null)
            {
                Debug.LogError("ArtIntegration_InteractionHint: no 'Bottom' found in Workshop scene.");
                return;
            }

            Transform existing = bottomGO.transform.Find("InstructionText");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            RectTransform textRect = CreateUIObject("InstructionText", bottomGO.transform);
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 0f);
            textRect.pivot = new Vector2(0.5f, 0f);
            textRect.sizeDelta = new Vector2(-40f, 80f);
            textRect.anchoredPosition = new Vector2(0f, 130f);

            TextMeshProUGUI instructionText = textRect.gameObject.AddComponent<TextMeshProUGUI>();
            instructionText.text = string.Empty;
            instructionText.fontSize = 40;
            instructionText.alignment = TextAlignmentOptions.Center;
            instructionText.color = Color.black;
            instructionText.fontStyle = FontStyles.Bold;

            GameObject hudInfoGO = GameObject.Find("HUDInfo");
            if (hudInfoGO == null)
            {
                Debug.LogError("ArtIntegration_InteractionHint: no 'HUDInfo' found in Workshop scene.");
                return;
            }

            RepairHUDView repairHUD = hudInfoGO.GetComponent<RepairHUDView>();
            if (repairHUD == null)
            {
                Debug.LogError("ArtIntegration_InteractionHint: no RepairHUDView on 'HUDInfo'.");
                return;
            }

            SetPrivateField(repairHUD, "_instructionText", instructionText);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Interaction Hint", "Added the instruction text element and wired it into RepairHUDView.", "OK");
            Debug.Log("ArtIntegration_InteractionHint: done.");
        }

        private static RectTransform CreateUIObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                Debug.LogError($"ArtIntegration_InteractionHint: field '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }

            field.SetValue(target, value);
        }
    }
}

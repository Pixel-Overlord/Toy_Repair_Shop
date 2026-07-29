using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace ToyRepairShop.EditorTools
{
    /// <summary>
    /// Corrects the Toys/Tools atlas slicing: AtlasGridSlicer assumed a
    /// perfectly uniform grid, but these atlases have inconsistent
    /// padding between icons, which cut some icons in half. This uses
    /// AlphaBoundsSlicer (content-aware, per-icon pixel bounds) instead,
    /// re-wires the sample ToyData/ToolData assets, redoes the toolbar
    /// icons, and writes a contact-sheet PNG per atlas to
    /// Assets/Art/_Debug so the slicing can be checked without opening
    /// the Sprite Editor.
    /// </summary>
    public static class ArtIntegration_FixSlicing
    {
        private const string ToysAtlasPath = "Assets/Art/Atlas_Toys_01.png";
        private const string ToolsAtlasPath = "Assets/Art/Atlas_Tools_01.png";
        private const string ScenePath = "Assets/Scenes/Workshop.unity";

        private static readonly string[] ToySpriteNames =
        {
            "TeddyBear_Broken", "TeddyBear_Repaired", "Robot_Broken", "Robot_Repaired", "Car_Broken", "Car_Repaired",
            "Train_Broken", "Train_Repaired", "Duck_Broken", "Duck_Repaired", "Dino_Broken", "Dino_Repaired",
            "Plane_Broken", "Plane_Repaired", "Rocket_Broken", "Rocket_Repaired", "Doll_Broken", "Doll_Repaired",
        };

        private static readonly string[] ToolSpriteNames =
        {
            "Sponge", "SoapBar", "CleaningBrush", "GlueBottle", "Towel", "NeedleThread", "PaintTube_Pink", "Paintbrush",
            "SprayBottle", "MagnifyingGlass", "Pliers", "Screwdriver", "Tweezers", "Tape", "Mallet", "Hairdryer",
            "AirBlower", "PolishJar", "CottonSwab", "OilCan", "SoapBowl", "Vacuum", "Bucket", "Gloves",
            "Duster", "ScrubBrush", "FoldedCloth", "GlueGun", "PaintTube_Green", "HeatGun", "Polisher", "UtilityKnife",
        };

        private static readonly (string ButtonName, string SpriteName)[] ButtonIcons =
        {
            ("SpongeButton", "Sponge"),
            ("ClothButton", "Towel"),
            ("NeedleButton", "NeedleThread"),
            ("PaintRollerButton", "Paintbrush"),
            ("ScrewdriverButton", "Screwdriver"),
        };

        [MenuItem("Tools/ToyRepairShop/Art/Fix Atlas Slicing (Alpha-Bounds)")]
        public static void Run()
        {
            List<RectInt> toyBlobs = AlphaBoundsSlicer.DetectBlobs(ToysAtlasPath, alphaThreshold: 40, minSize: 40);
            AlphaBoundsSlicer.WriteContactSheet(ToysAtlasPath, toyBlobs, "Assets/Art/_Debug/Atlas_Toys_01_contactsheet.png", columns: 6);

            List<RectInt> toolBlobs = AlphaBoundsSlicer.DetectBlobs(ToolsAtlasPath, alphaThreshold: 40, minSize: 30);
            AlphaBoundsSlicer.WriteContactSheet(ToolsAtlasPath, toolBlobs, "Assets/Art/_Debug/Atlas_Tools_01_contactsheet.png", columns: 8);

            bool toysOk = toyBlobs.Count == ToySpriteNames.Length;
            bool toolsOk = toolBlobs.Count == ToolSpriteNames.Length;

            if (toysOk)
            {
                AlphaBoundsSlicer.ApplySprites(ToysAtlasPath, toyBlobs, ToySpriteNames);
                WireToyData("Assets/ScriptableObjects/Toys/BrokenTeddy.asset", "TeddyBear_Broken", "TeddyBear_Repaired");
                WireToyData("Assets/ScriptableObjects/Toys/BrokenRobot.asset", "Robot_Broken", "Robot_Repaired");
                WireToyData("Assets/ScriptableObjects/Toys/BrokenToyCar.asset", "Car_Broken", "Car_Repaired");
            }

            if (toolsOk)
            {
                AlphaBoundsSlicer.ApplySprites(ToolsAtlasPath, toolBlobs, ToolSpriteNames);
                WireToolData("Assets/ScriptableObjects/Tools/Soap.asset", "SoapBar");
                WireToolData("Assets/ScriptableObjects/Tools/Sponge.asset", "Sponge");
                WireToolData("Assets/ScriptableObjects/Tools/Needle.asset", "NeedleThread");
                WireToolData("Assets/ScriptableObjects/Tools/Brush.asset", "CleaningBrush");

                Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                int applied = 0;
                foreach ((string buttonName, string spriteName) in ButtonIcons)
                {
                    GameObject buttonGO = GameObject.Find(buttonName);
                    if (buttonGO == null)
                    {
                        Debug.LogWarning($"ArtIntegration_FixSlicing: could not find '{buttonName}' in the open scene.");
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
                Debug.Log($"ArtIntegration_FixSlicing: reapplied icons to {applied}/{ButtonIcons.Length} toolbar buttons.");
            }

            AssetDatabase.SaveAssets();

            string message = $"Toys: detected {toyBlobs.Count} blobs (expected {ToySpriteNames.Length}) - {(toysOk ? "applied" : "SKIPPED, count mismatch")}.\n" +
                              $"Tools: detected {toolBlobs.Count} blobs (expected {ToolSpriteNames.Length}) - {(toolsOk ? "applied" : "SKIPPED, count mismatch")}.\n\n" +
                              "Contact sheets written to Assets/Art/_Debug/ for visual verification.";

            EditorUtility.DisplayDialog("Fix Atlas Slicing", message, "OK");
            Debug.Log("ArtIntegration_FixSlicing: " + message.Replace("\n", " "));
        }

        private static void WireToyData(string assetPath, string brokenSpriteName, string repairedSpriteName)
        {
            var toyData = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (toyData == null)
            {
                Debug.LogError($"ArtIntegration_FixSlicing: could not load ToyData at '{assetPath}'.");
                return;
            }

            Sprite broken = AtlasGridSlicer.LoadSprite(ToysAtlasPath, brokenSpriteName);
            Sprite repaired = AtlasGridSlicer.LoadSprite(ToysAtlasPath, repairedSpriteName);

            var serialized = new SerializedObject(toyData);
            serialized.FindProperty("_brokenSprite").objectReferenceValue = broken;
            serialized.FindProperty("_repairedSprite").objectReferenceValue = repaired;
            serialized.FindProperty("_thumbnailSprite").objectReferenceValue = broken;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(toyData);
        }

        private static void WireToolData(string assetPath, string iconSpriteName)
        {
            var toolData = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (toolData == null)
            {
                Debug.LogError($"ArtIntegration_FixSlicing: could not load ToolData at '{assetPath}'.");
                return;
            }

            Sprite icon = AtlasGridSlicer.LoadSprite(ToolsAtlasPath, iconSpriteName);

            var serialized = new SerializedObject(toolData);
            serialized.FindProperty("_icon").objectReferenceValue = icon;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(toolData);
        }

        private static void ApplyIcon(GameObject buttonGO, Sprite icon)
        {
            Transform existingIcon = buttonGO.transform.Find("Icon");
            if (existingIcon != null)
            {
                Object.DestroyImmediate(existingIcon.gameObject);
            }

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

            var iconImage = iconGO.AddComponent<UnityEngine.UI.Image>();
            iconImage.sprite = icon;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
        }
    }
}

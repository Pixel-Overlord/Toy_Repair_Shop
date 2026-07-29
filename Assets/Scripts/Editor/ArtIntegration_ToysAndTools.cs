using UnityEditor;
using UnityEngine;

namespace ToyRepairShop.EditorTools
{
    /// <summary>
    /// One-time art pass: slices Atlas_Toys_01 (6x3 grid) and
    /// Atlas_Tools_01 (8x4 grid) into named sprites via AtlasGridSlicer,
    /// then wires the resulting sprites into the existing sample ToyData
    /// and ToolData assets' Broken/Repaired/Icon fields using
    /// SerializedObject, so the assignment is a normal, undo-friendly
    /// asset edit rather than raw file writes.
    /// </summary>
    public static class ArtIntegration_ToysAndTools
    {
        private const string ToysAtlasPath = "Assets/Art/Atlas_Toys_01.png";
        private const string ToolsAtlasPath = "Assets/Art/Atlas_Tools_01.png";

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

        [MenuItem("Tools/ToyRepairShop/Art/Slice Toys and Tools Atlases + Wire Sample Assets")]
        public static void Run()
        {
            AtlasGridSlicer.SliceGrid(ToysAtlasPath, 6, 3, ToySpriteNames);
            AtlasGridSlicer.SliceGrid(ToolsAtlasPath, 8, 4, ToolSpriteNames);

            WireToyData("Assets/ScriptableObjects/Toys/BrokenTeddy.asset", "TeddyBear_Broken", "TeddyBear_Repaired");
            WireToyData("Assets/ScriptableObjects/Toys/BrokenRobot.asset", "Robot_Broken", "Robot_Repaired");
            WireToyData("Assets/ScriptableObjects/Toys/BrokenToyCar.asset", "Car_Broken", "Car_Repaired");

            WireToolData("Assets/ScriptableObjects/Tools/Soap.asset", "SoapBar");
            WireToolData("Assets/ScriptableObjects/Tools/Sponge.asset", "Sponge");
            WireToolData("Assets/ScriptableObjects/Tools/Needle.asset", "NeedleThread");
            WireToolData("Assets/ScriptableObjects/Tools/Brush.asset", "CleaningBrush");

            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog(
                "Art Integration",
                "Sliced Atlas_Toys_01 and Atlas_Tools_01 and wired Broken/Repaired sprites into the 3 sample ToyData assets and icons into the 4 sample ToolData assets.",
                "OK");

            Debug.Log("ArtIntegration_ToysAndTools: done.");
        }

        private static void WireToyData(string assetPath, string brokenSpriteName, string repairedSpriteName)
        {
            var toyData = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (toyData == null)
            {
                Debug.LogError($"ArtIntegration_ToysAndTools: could not load ToyData at '{assetPath}'.");
                return;
            }

            Sprite broken = AtlasGridSlicer.LoadSprite(ToysAtlasPath, brokenSpriteName);
            Sprite repaired = AtlasGridSlicer.LoadSprite(ToysAtlasPath, repairedSpriteName);

            var serialized = new SerializedObject(toyData);
            serialized.FindProperty("_brokenSprite").objectReferenceValue = broken;
            serialized.FindProperty("_repairedSprite").objectReferenceValue = repaired;
            // Thumbnail shows the player what needs fixing, so it uses the broken sprite.
            serialized.FindProperty("_thumbnailSprite").objectReferenceValue = broken;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(toyData);
        }

        private static void WireToolData(string assetPath, string iconSpriteName)
        {
            var toolData = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (toolData == null)
            {
                Debug.LogError($"ArtIntegration_ToysAndTools: could not load ToolData at '{assetPath}'.");
                return;
            }

            Sprite icon = AtlasGridSlicer.LoadSprite(ToolsAtlasPath, iconSpriteName);

            var serialized = new SerializedObject(toolData);
            serialized.FindProperty("_icon").objectReferenceValue = icon;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(toolData);
        }
    }
}

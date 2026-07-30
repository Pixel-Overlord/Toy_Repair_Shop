using System.Collections.Generic;
using System.Reflection;
using ToyRepairShop.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ToyRepairShop.EditorTools
{
    /// <summary>
    /// One-time art pass: slices Atlas_UI_01 (28 labeled elements,
    /// alpha-bounds at a high threshold to avoid the touching-button
    /// merges seen at lower thresholds - verified via contact sheet),
    /// then builds the MainMenu scene's Canvas from scratch (it had none
    /// - Stage 2 left it for manual assembly, which never happened) and
    /// re-skins the Workshop reward popup, HUD info panel, and coin
    /// display with the sliced UI art.
    /// </summary>
    public static class ArtIntegration_UIAtlas
    {
        private const string UiAtlasPath = "Assets/Art/Atlas_UI_01.png";
        private const string BackgroundSpritePath = "Assets/Art/BG.png";
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string WorkshopScenePath = "Assets/Scenes/Workshop.unity";

        private static readonly string[] UiSpriteNames =
        {
            "UI_Logo", "BTN_Play", "BTN_Settings", "BTN_Shop", "BTN_Primary_Blue", "BTN_Primary_Green",
            "BTN_Primary_Yellow", "BTN_Primary_Red", "BTN_Secondary_Beige", "BTN_Secondary_Purple", "BTN_Secondary_Pink", "BTN_Close",
            "BTN_Back", "UI_ProgressBar", "Panel_Default", "Popup_Reward", "Panel_Tall", "Popup_Settings",
            "Panel_Wide", "Panel_Small", "Icon_Coin", "Icon_Home", "Icon_Sound", "Icon_Settings",
            "Icon_Mail", "Icon_Gift", "Icon_Daily", "Icon_Achievement",
        };

        [MenuItem("Tools/ToyRepairShop/Art/Slice UI Atlas + Build MainMenu + Reskin Workshop HUD")]
        public static void Run()
        {
            List<RectInt> blobs = AlphaBoundsSlicer.DetectBlobs(UiAtlasPath, alphaThreshold: 253, minSize: 40);
            if (blobs.Count != UiSpriteNames.Length)
            {
                Debug.LogError($"ArtIntegration_UIAtlas: detected {blobs.Count} blobs, expected {UiSpriteNames.Length}. Aborting - check Assets/Art/_Debug/Atlas_UI_01_contactsheet.png.");
                AlphaBoundsSlicer.WriteContactSheet(UiAtlasPath, blobs, "Assets/Art/_Debug/Atlas_UI_01_contactsheet.png", 6);
                return;
            }

            AlphaBoundsSlicer.ApplySprites(UiAtlasPath, blobs, UiSpriteNames);

            BuildMainMenu();
            ReskinWorkshopHud();

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("UI Atlas Integration", "Sliced Atlas_UI_01, built the MainMenu Canvas, and reskinned the Workshop reward popup/HUD/coins.", "OK");
            Debug.Log("ArtIntegration_UIAtlas: done.");
        }

        private static void BuildMainMenu()
        {
            Scene scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);

            Transform existingCanvas = FindRoot(scene, "Canvas");
            if (existingCanvas != null)
            {
                Object.DestroyImmediate(existingCanvas.gameObject);
            }

            // Remove every prior MainMenuUI root - re-running this tool
            // previously left duplicates behind (only the last one ends
            // up wired, but stale unwired copies stick around too).
            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (GameObject root in rootObjects)
            {
                if (root.name == "MainMenuUI")
                {
                    Object.DestroyImmediate(root);
                }
            }

            GameObject canvasGO = CreateCanvas();
            EnsureEventSystem();

            Sprite backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundSpritePath);
            if (backgroundSprite != null)
            {
                RectTransform bgRect = CreateUIObject("BackgroundImage", canvasGO.transform);
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;
                Image bgImage = bgRect.gameObject.AddComponent<Image>();
                bgImage.sprite = backgroundSprite;
                bgImage.color = new Color(1f, 1f, 1f, 0.55f); // dimmed so buttons stay readable over the busy workshop art
                bgImage.raycastTarget = false;

                RectTransform tintRect = CreateUIObject("BackgroundTint", canvasGO.transform);
                tintRect.anchorMin = Vector2.zero;
                tintRect.anchorMax = Vector2.one;
                tintRect.offsetMin = Vector2.zero;
                tintRect.offsetMax = Vector2.zero;
                Image tint = tintRect.gameObject.AddComponent<Image>();
                tint.color = new Color(0.55f, 0.75f, 0.75f, 0.5f);
                tint.raycastTarget = false;
            }

            RectTransform logoRect = CreateUIObject("Logo", canvasGO.transform);
            logoRect.anchorMin = new Vector2(0.5f, 1f);
            logoRect.anchorMax = new Vector2(0.5f, 1f);
            logoRect.sizeDelta = new Vector2(600f, 460f);
            logoRect.anchoredPosition = new Vector2(0f, -320f);
            Image logoImage = logoRect.gameObject.AddComponent<Image>();
            logoImage.sprite = AtlasGridSlicer.LoadSprite(UiAtlasPath, "UI_Logo");
            logoImage.preserveAspect = true;
            logoImage.raycastTarget = false;

            Button playButton = CreateSpriteButton(canvasGO.transform, "PlayButton", "BTN_Play", new Vector2(0f, 200f), new Vector2(420f, 220f));
            Button settingsButton = CreateSpriteButton(canvasGO.transform, "SettingsButton", "BTN_Settings", new Vector2(0f, -60f), new Vector2(220f, 220f));
            Button quitButton = CreateSpriteButton(canvasGO.transform, "QuitButton", "BTN_Close", new Vector2(0f, -320f), new Vector2(180f, 180f));

            RectTransform settingsPanelRect = CreateUIObject("SettingsPanel", canvasGO.transform);
            settingsPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            settingsPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            settingsPanelRect.sizeDelta = new Vector2(750f, 750f);
            settingsPanelRect.anchoredPosition = Vector2.zero;
            Image settingsPanelImage = settingsPanelRect.gameObject.AddComponent<Image>();
            settingsPanelImage.sprite = AtlasGridSlicer.LoadSprite(UiAtlasPath, "Popup_Settings");
            settingsPanelImage.preserveAspect = true;
            settingsPanelRect.gameObject.SetActive(false);

            GameObject uiRootGO = new GameObject("MainMenuUI");
            MainMenuUI mainMenuUI = uiRootGO.AddComponent<MainMenuUI>();
            SetPrivateField(mainMenuUI, "_playButton", playButton);
            SetPrivateField(mainMenuUI, "_settingsButton", settingsButton);
            SetPrivateField(mainMenuUI, "_quitButton", quitButton);
            SetPrivateField(mainMenuUI, "_settingsPanel", settingsPanelRect.gameObject);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void ReskinWorkshopHud()
        {
            Scene scene = EditorSceneManager.OpenScene(WorkshopScenePath, OpenSceneMode.Single);

            GameObject popupPanel = GameObject.Find("PopupPanel");
            if (popupPanel != null)
            {
                Image popupImage = popupPanel.GetComponent<Image>();
                if (popupImage != null)
                {
                    popupImage.sprite = AtlasGridSlicer.LoadSprite(UiAtlasPath, "Panel_Default");
                    popupImage.type = Image.Type.Sliced;
                    popupImage.color = Color.white;
                }
            }

            GameObject hudInfo = GameObject.Find("HUDInfo");
            if (hudInfo != null)
            {
                Transform existingBg = hudInfo.transform.Find("PanelBackground");
                if (existingBg != null)
                {
                    Object.DestroyImmediate(existingBg.gameObject);
                }

                GameObject bgGO = new GameObject("PanelBackground", typeof(RectTransform));
                bgGO.transform.SetParent(hudInfo.transform, false);
                bgGO.transform.SetAsFirstSibling();

                RectTransform bgRect = bgGO.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = new Vector2(-20f, -16f);
                bgRect.offsetMax = new Vector2(20f, 16f);

                // A plain rounded color panel reads more reliably here than
                // a decorative sprite stretched into a wide, short bar -
                // the UI atlas's panels are roughly square and distort
                // badly at this aspect ratio without proper 9-slice borders.
                Image bgImage = bgGO.AddComponent<Image>();
                bgImage.color = new Color(1f, 0.97f, 0.9f, 0.85f);
                bgImage.raycastTarget = false;
            }

            GameObject coinsArea = GameObject.Find("CoinsArea");
            if (coinsArea != null && coinsArea.transform.Find("CoinIcon") == null)
            {
                RectTransform coinsAreaRect = coinsArea.GetComponent<RectTransform>();

                GameObject iconGO = new GameObject("CoinIcon", typeof(RectTransform));
                iconGO.transform.SetParent(coinsArea.transform, false);
                iconGO.transform.SetAsFirstSibling();

                RectTransform iconRect = iconGO.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0f, 0.5f);
                iconRect.anchorMax = new Vector2(0f, 0.5f);
                iconRect.sizeDelta = new Vector2(90f, 90f);
                iconRect.anchoredPosition = new Vector2(60f, 0f);

                Image iconImage = iconGO.AddComponent<Image>();
                iconImage.sprite = AtlasGridSlicer.LoadSprite(UiAtlasPath, "Icon_Coin");
                iconImage.preserveAspect = true;
                iconImage.raycastTarget = false;

                Transform coinsTextTransform = coinsArea.transform.Find("CoinsText");
                if (coinsTextTransform != null && coinsTextTransform.GetComponent<RectTransform>() is RectTransform coinsTextRect)
                {
                    coinsTextRect.anchorMin = new Vector2(0f, 0f);
                    coinsTextRect.anchorMax = new Vector2(1f, 1f);
                    coinsTextRect.offsetMin = new Vector2(120f, 0f);
                    coinsTextRect.offsetMax = Vector2.zero;
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static Button CreateSpriteButton(Transform parent, string name, string spriteName, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            Image image = go.GetComponent<Image>();
            image.sprite = AtlasGridSlicer.LoadSprite(UiAtlasPath, spriteName);
            image.preserveAspect = true;

            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            return button;
        }

        private static Transform FindRoot(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == name)
                {
                    return root.transform;
                }
            }
            return null;
        }

        private static GameObject CreateCanvas()
        {
            GameObject canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            return canvasGO;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemGO = new GameObject("EventSystem", typeof(EventSystem));
            eventSystemGO.AddComponent<InputSystemUIInputModule>();
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
                Debug.LogError($"ArtIntegration_UIAtlas: field '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }

            field.SetValue(target, value);
        }
    }
}

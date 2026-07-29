using System.Collections.Generic;
using System.Reflection;
using ToyRepairShop.Data.Enums;
using ToyRepairShop.Data.ScriptableObjects;
using ToyRepairShop.Gameplay.Interaction;
using ToyRepairShop.Gameplay.Spawning;
using ToyRepairShop.Managers;
using ToyRepairShop.UI;
using TMPro;
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
    /// One-time Stage 4 bootstrap: builds the Workshop scene's Canvas/UI
    /// hierarchy and wires every serialized reference using real Unity API
    /// calls (AddComponent, reflection into our own SerializeField
    /// backing fields) instead of hand-authored YAML - so every component
    /// reference is resolved by the Editor itself, not guessed GUIDs.
    /// Editor-only: lives under an "Editor" folder and is excluded from
    /// player builds. Safe to delete after running once.
    /// </summary>
    public static class Stage4WorkshopSceneSetup
    {
        private const string ScenePath = "Assets/Scenes/Workshop.unity";
        private const string ToyDatabasePath = "Assets/ScriptableObjects/ToyDatabase.asset";

        [MenuItem("Tools/ToyRepairShop/Stage 4/Setup Workshop Scene")]
        public static void SetupWorkshopScene()
        {
            if (GameObject.Find("Canvas") != null)
            {
                bool proceed = EditorUtility.DisplayDialog(
                    "Workshop Scene Setup",
                    "A 'Canvas' GameObject already exists in the currently open scene. Running this again will create duplicates.\n\nContinue anyway?",
                    "Continue", "Cancel");

                if (!proceed)
                {
                    return;
                }
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            GameObject canvasGO = CreateCanvas();
            EnsureEventSystem();

            // --- Top: Coins ---
            RectTransform top = CreateUIObject("Top", canvasGO.transform);
            top.anchorMin = new Vector2(0f, 1f);
            top.anchorMax = new Vector2(1f, 1f);
            top.pivot = new Vector2(0.5f, 1f);
            top.sizeDelta = new Vector2(0f, 150f);
            top.anchoredPosition = Vector2.zero;

            TextMeshProUGUI coinsText = CreateText(top, "CoinsText", "0", 60, TextAlignmentOptions.Center);
            CoinsDisplayView coinsDisplay = top.gameObject.AddComponent<CoinsDisplayView>();
            SetPrivateField(coinsDisplay, "_coinsText", coinsText);

            // --- Center: Toy Area (view + wash interaction share the same object) ---
            RectTransform toyArea = CreateUIObject("ToyArea", canvasGO.transform);
            toyArea.anchorMin = new Vector2(0.5f, 0.5f);
            toyArea.anchorMax = new Vector2(0.5f, 0.5f);
            toyArea.sizeDelta = new Vector2(700f, 700f);
            toyArea.anchoredPosition = new Vector2(0f, 150f);

            Image toyImage = toyArea.gameObject.AddComponent<Image>();
            toyImage.preserveAspect = true;
            toyImage.color = Color.white;

            ToyView toyView = toyArea.gameObject.AddComponent<ToyView>();
            SetPrivateField(toyView, "_toyImage", toyImage);

            WashInteraction washInteraction = toyArea.gameObject.AddComponent<WashInteraction>();

            // --- Bottom: Toolbar + Progress Bar ---
            RectTransform bottom = CreateUIObject("Bottom", canvasGO.transform);
            bottom.anchorMin = new Vector2(0f, 0f);
            bottom.anchorMax = new Vector2(1f, 0f);
            bottom.pivot = new Vector2(0.5f, 0f);
            bottom.sizeDelta = new Vector2(0f, 450f);
            bottom.anchoredPosition = Vector2.zero;

            RectTransform toolbarRoot = CreateUIObject("Toolbar", bottom);
            toolbarRoot.anchorMin = new Vector2(0f, 1f);
            toolbarRoot.anchorMax = new Vector2(1f, 1f);
            toolbarRoot.pivot = new Vector2(0.5f, 1f);
            toolbarRoot.sizeDelta = new Vector2(0f, 180f);
            toolbarRoot.anchoredPosition = Vector2.zero;

            Button spongeButton = CreateButton(toolbarRoot, "SpongeButton", "Sponge", out _);
            RectTransform spongeRect = spongeButton.GetComponent<RectTransform>();
            spongeRect.anchorMin = new Vector2(0.5f, 0.5f);
            spongeRect.anchorMax = new Vector2(0.5f, 0.5f);
            spongeRect.sizeDelta = new Vector2(220f, 140f);
            spongeRect.anchoredPosition = Vector2.zero;

            ToolButtonView spongeToolButton = spongeButton.gameObject.AddComponent<ToolButtonView>();
            SetPrivateField(spongeToolButton, "_tool", ToolType.Sponge);
            SetPrivateField(spongeToolButton, "_button", spongeButton);
            SetPrivateField(spongeToolButton, "_visualRoot", spongeRect);

            ToolbarView toolbarView = toolbarRoot.gameObject.AddComponent<ToolbarView>();
            SetPrivateField(toolbarView, "_buttons", new List<ToolButtonView> { spongeToolButton });

            RectTransform progressRoot = CreateUIObject("ProgressBar", bottom);
            progressRoot.anchorMin = new Vector2(0.1f, 0f);
            progressRoot.anchorMax = new Vector2(0.9f, 0f);
            progressRoot.pivot = new Vector2(0.5f, 0f);
            progressRoot.sizeDelta = new Vector2(0f, 50f);
            progressRoot.anchoredPosition = new Vector2(0f, 40f);

            Image progressBg = progressRoot.gameObject.AddComponent<Image>();
            progressBg.color = new Color(0.8f, 0.8f, 0.8f);

            RectTransform fillRect = CreateUIObject("Fill", progressRoot);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image fillImage = fillRect.gameObject.AddComponent<Image>();
            fillImage.color = new Color(0.3f, 0.8f, 0.4f);
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillAmount = 0f;

            ProgressBarView progressBarView = progressRoot.gameObject.AddComponent<ProgressBarView>();
            SetPrivateField(progressBarView, "_root", progressRoot.gameObject);
            SetPrivateField(progressBarView, "_fillImage", fillImage);

            // --- Reward popup (centered over the whole canvas so it reads clearly) ---
            RectTransform popupRoot = CreateUIObject("PopupPanel", canvasGO.transform);
            popupRoot.anchorMin = new Vector2(0.5f, 0.5f);
            popupRoot.anchorMax = new Vector2(0.5f, 0.5f);
            popupRoot.sizeDelta = new Vector2(600f, 300f);
            popupRoot.anchoredPosition = Vector2.zero;

            Image popupBg = popupRoot.gameObject.AddComponent<Image>();
            popupBg.color = new Color(1f, 1f, 1f, 0.95f);

            TextMeshProUGUI rewardText = CreateText(popupRoot, "RewardText", "+0 Coins", 70, TextAlignmentOptions.Center);
            rewardText.color = Color.black;

            RewardPopupView rewardPopupView = popupRoot.gameObject.AddComponent<RewardPopupView>();
            SetPrivateField(rewardPopupView, "_root", popupRoot.gameObject);
            SetPrivateField(rewardPopupView, "_rewardText", rewardText);
            popupRoot.gameObject.SetActive(false);

            // --- Non-visual systems ---
            GameObject toySpawnerGO = new GameObject("ToySpawner");
            ToySpawner toySpawner = toySpawnerGO.AddComponent<ToySpawner>();
            ToyDatabase toyDatabase = AssetDatabase.LoadAssetAtPath<ToyDatabase>(ToyDatabasePath);
            if (toyDatabase == null)
            {
                Debug.LogError($"Stage4WorkshopSceneSetup: could not load ToyDatabase at '{ToyDatabasePath}'.");
            }
            SetPrivateField(toySpawner, "_toyDatabase", toyDatabase);

            GameObject workshopControllerGO = new GameObject("WorkshopController");
            WorkshopController workshopController = workshopControllerGO.AddComponent<WorkshopController>();
            SetPrivateField(workshopController, "_toySpawner", toySpawner);
            SetPrivateField(workshopController, "_toyView", toyView);
            SetPrivateField(workshopController, "_washInteraction", washInteraction);
            SetPrivateField(workshopController, "_toolbarView", toolbarView);
            SetPrivateField(workshopController, "_progressBarView", progressBarView);
            SetPrivateField(workshopController, "_rewardPopupView", rewardPopupView);
            SetPrivateField(workshopController, "_coinsDisplayView", coinsDisplay);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog(
                "Workshop Scene Setup",
                "Done. The Workshop scene now has a Canvas, toy area, sponge toolbar, progress bar, reward popup, ToySpawner and WorkshopController, all wired up and saved.\n\nPress Play to test: tap Sponge, then drag on the teddy.",
                "OK");

            Debug.Log("Stage4WorkshopSceneSetup: Workshop scene setup complete.");
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

        private static Image CreateImage(Transform parent, string name, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, int fontSize, TextAlignmentOptions alignment)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.black;

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return tmp;
        }

        private static Button CreateButton(Transform parent, string name, string label, out TextMeshProUGUI buttonText)
        {
            Image background = CreateImage(parent, name, new Color(0.85f, 0.85f, 0.9f));
            Button button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            buttonText = CreateText(background.transform, "Label", label, 36, TextAlignmentOptions.Center);
            return button;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                Debug.LogError($"Stage4WorkshopSceneSetup: field '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }

            field.SetValue(target, value);
        }
    }
}

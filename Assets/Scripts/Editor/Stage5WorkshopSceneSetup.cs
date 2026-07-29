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
    /// One-time Stage 5 bootstrap: rebuilds the Workshop scene's Canvas/UI
    /// hierarchy for the multi-step repair loop (expanded toolbar, full
    /// RepairHUD, reward popup with a Continue button, debug panel) using
    /// real Unity API calls - so every component reference is resolved by
    /// the Editor itself, not guessed GUIDs. Removes whatever the Stage 4
    /// tool built first, then rebuilds fresh. Editor-only: lives under an
    /// "Editor" folder and is excluded from player builds. Safe to delete
    /// after running once.
    /// </summary>
    public static class Stage5WorkshopSceneSetup
    {
        private const string ScenePath = "Assets/Scenes/Workshop.unity";
        private const string ToyDatabasePath = "Assets/ScriptableObjects/ToyDatabase.asset";

        private static readonly ToolType[] ToolbarTools =
        {
            ToolType.Sponge,
            ToolType.Cloth,
            ToolType.Needle,
            ToolType.PaintRoller,
            ToolType.Screwdriver,
        };

        [MenuItem("Tools/ToyRepairShop/Stage 5/Setup Workshop Scene")]
        public static void SetupWorkshopScene()
        {
            bool proceed = EditorUtility.DisplayDialog(
                "Workshop Scene Setup (Stage 5)",
                "This rebuilds the Workshop scene's Canvas, ToySpawner, WorkshopController and debug panel from scratch, removing any existing ones with those names first.\n\nContinue?",
                "Continue", "Cancel");

            if (!proceed)
            {
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            RemoveIfPresent("Canvas");
            RemoveIfPresent("ToySpawner");
            RemoveIfPresent("WorkshopController");
            RemoveIfPresent("WorkshopDebugPanel");
            RemoveIfPresent("EventSystem");

            GameObject canvasGO = CreateCanvas();
            EnsureEventSystem();

            // --- Top: Coins + Toy Name ---
            RectTransform top = CreateUIObject("Top", canvasGO.transform);
            top.anchorMin = new Vector2(0f, 1f);
            top.anchorMax = new Vector2(1f, 1f);
            top.pivot = new Vector2(0.5f, 1f);
            top.sizeDelta = new Vector2(0f, 150f);
            top.anchoredPosition = Vector2.zero;

            RectTransform coinsRect = CreateUIObject("CoinsArea", top);
            coinsRect.anchorMin = new Vector2(0f, 0f);
            coinsRect.anchorMax = new Vector2(0.5f, 1f);
            coinsRect.offsetMin = Vector2.zero;
            coinsRect.offsetMax = Vector2.zero;
            TextMeshProUGUI coinsText = CreateText(coinsRect, "CoinsText", "0", 60, TextAlignmentOptions.MidlineLeft);
            CoinsDisplayView coinsDisplay = top.gameObject.AddComponent<CoinsDisplayView>();
            SetPrivateField(coinsDisplay, "_coinsText", coinsText);

            RectTransform toyNameRect = CreateUIObject("ToyNameArea", top);
            toyNameRect.anchorMin = new Vector2(0.5f, 0f);
            toyNameRect.anchorMax = new Vector2(1f, 1f);
            toyNameRect.offsetMin = Vector2.zero;
            toyNameRect.offsetMax = Vector2.zero;
            TextMeshProUGUI toyNameText = CreateText(toyNameRect, "ToyNameText", "-", 50, TextAlignmentOptions.MidlineRight);

            // --- HUD info block: current/next step, current tool, remaining steps ---
            RectTransform hudInfo = CreateUIObject("HUDInfo", canvasGO.transform);
            hudInfo.anchorMin = new Vector2(0f, 1f);
            hudInfo.anchorMax = new Vector2(1f, 1f);
            hudInfo.pivot = new Vector2(0.5f, 1f);
            hudInfo.sizeDelta = new Vector2(0f, 200f);
            hudInfo.anchoredPosition = new Vector2(0f, -150f);

            TextMeshProUGUI currentStepText = CreateText(CreateRow(hudInfo, "CurrentStepRow", 0), "CurrentStepText", "Current: -", 36, TextAlignmentOptions.MidlineLeft);
            TextMeshProUGUI nextStepText = CreateText(CreateRow(hudInfo, "NextStepRow", 1), "NextStepText", "Next: -", 32, TextAlignmentOptions.MidlineLeft);
            TextMeshProUGUI currentToolText = CreateText(CreateRow(hudInfo, "CurrentToolRow", 2), "CurrentToolText", "Tool: -", 32, TextAlignmentOptions.MidlineLeft);
            TextMeshProUGUI remainingStepsText = CreateText(CreateRow(hudInfo, "RemainingStepsRow", 3), "RemainingStepsText", "Remaining: 0", 32, TextAlignmentOptions.MidlineLeft);

            // --- Center: Toy Area (view + repair drag interaction share the same object) ---
            RectTransform toyArea = CreateUIObject("ToyArea", canvasGO.transform);
            toyArea.anchorMin = new Vector2(0.5f, 0.5f);
            toyArea.anchorMax = new Vector2(0.5f, 0.5f);
            toyArea.sizeDelta = new Vector2(650f, 650f);
            toyArea.anchoredPosition = new Vector2(0f, 50f);

            Image toyImage = toyArea.gameObject.AddComponent<Image>();
            toyImage.preserveAspect = true;
            toyImage.color = Color.white;

            ToyView toyView = toyArea.gameObject.AddComponent<ToyView>();
            SetPrivateField(toyView, "_toyImage", toyImage);

            RepairDragInteraction repairDragInteraction = toyArea.gameObject.AddComponent<RepairDragInteraction>();

            // --- Bottom: Toolbar (5 tools) + Progress Bar ---
            RectTransform bottom = CreateUIObject("Bottom", canvasGO.transform);
            bottom.anchorMin = new Vector2(0f, 0f);
            bottom.anchorMax = new Vector2(1f, 0f);
            bottom.pivot = new Vector2(0.5f, 0f);
            bottom.sizeDelta = new Vector2(0f, 420f);
            bottom.anchoredPosition = Vector2.zero;

            RectTransform toolbarRoot = CreateUIObject("Toolbar", bottom);
            toolbarRoot.anchorMin = new Vector2(0f, 1f);
            toolbarRoot.anchorMax = new Vector2(1f, 1f);
            toolbarRoot.pivot = new Vector2(0.5f, 1f);
            toolbarRoot.sizeDelta = new Vector2(0f, 180f);
            toolbarRoot.anchoredPosition = Vector2.zero;

            var toolButtons = new System.Collections.Generic.List<ToolButtonView>();
            float buttonWidth = 190f;
            float spacing = 20f;
            float totalWidth = ToolbarTools.Length * buttonWidth + (ToolbarTools.Length - 1) * spacing;
            float startX = -totalWidth * 0.5f + buttonWidth * 0.5f;

            for (int i = 0; i < ToolbarTools.Length; i++)
            {
                ToolType tool = ToolbarTools[i];
                Button toolButton = CreateButton(toolbarRoot, $"{tool}Button", tool.ToString(), out _);
                RectTransform toolRect = toolButton.GetComponent<RectTransform>();
                toolRect.anchorMin = new Vector2(0.5f, 0.5f);
                toolRect.anchorMax = new Vector2(0.5f, 0.5f);
                toolRect.sizeDelta = new Vector2(buttonWidth, 140f);
                toolRect.anchoredPosition = new Vector2(startX + i * (buttonWidth + spacing), 0f);

                ToolButtonView toolButtonView = toolButton.gameObject.AddComponent<ToolButtonView>();
                SetPrivateField(toolButtonView, "_tool", tool);
                SetPrivateField(toolButtonView, "_button", toolButton);
                SetPrivateField(toolButtonView, "_visualRoot", toolRect);
                toolButtons.Add(toolButtonView);
            }

            ToolbarView toolbarView = toolbarRoot.gameObject.AddComponent<ToolbarView>();
            SetPrivateField(toolbarView, "_buttons", toolButtons);

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

            RepairHUDView repairHUD = hudInfo.gameObject.AddComponent<RepairHUDView>();
            SetPrivateField(repairHUD, "_toyNameText", toyNameText);
            SetPrivateField(repairHUD, "_currentStepText", currentStepText);
            SetPrivateField(repairHUD, "_nextStepText", nextStepText);
            SetPrivateField(repairHUD, "_currentToolText", currentToolText);
            SetPrivateField(repairHUD, "_remainingStepsText", remainingStepsText);
            SetPrivateField(repairHUD, "_progressBarView", progressBarView);

            // --- Reward popup (centered over the whole canvas so it reads clearly) ---
            RectTransform popupRoot = CreateUIObject("PopupPanel", canvasGO.transform);
            popupRoot.anchorMin = new Vector2(0.5f, 0.5f);
            popupRoot.anchorMax = new Vector2(0.5f, 0.5f);
            popupRoot.sizeDelta = new Vector2(700f, 450f);
            popupRoot.anchoredPosition = Vector2.zero;

            Image popupBg = popupRoot.gameObject.AddComponent<Image>();
            popupBg.color = new Color(1f, 1f, 1f, 0.95f);

            RectTransform completedRect = CreateUIObject("CompletedTextArea", popupRoot);
            completedRect.anchorMin = new Vector2(0f, 0.62f);
            completedRect.anchorMax = new Vector2(1f, 1f);
            completedRect.offsetMin = Vector2.zero;
            completedRect.offsetMax = Vector2.zero;
            TextMeshProUGUI completedText = CreateText(completedRect, "CompletedText", "Repair Completed!", 48, TextAlignmentOptions.Center);
            completedText.color = Color.black;

            RectTransform rewardRect = CreateUIObject("RewardTextArea", popupRoot);
            rewardRect.anchorMin = new Vector2(0f, 0.32f);
            rewardRect.anchorMax = new Vector2(1f, 0.62f);
            rewardRect.offsetMin = Vector2.zero;
            rewardRect.offsetMax = Vector2.zero;
            TextMeshProUGUI rewardText = CreateText(rewardRect, "RewardText", "+0 Coins", 60, TextAlignmentOptions.Center);
            rewardText.color = Color.black;

            Button continueButton = CreateButton(popupRoot, "ContinueButton", "Continue", out _);
            RectTransform continueRect = continueButton.GetComponent<RectTransform>();
            continueRect.anchorMin = new Vector2(0.5f, 0f);
            continueRect.anchorMax = new Vector2(0.5f, 0f);
            continueRect.pivot = new Vector2(0.5f, 0f);
            continueRect.sizeDelta = new Vector2(260f, 100f);
            continueRect.anchoredPosition = new Vector2(0f, 40f);

            RewardPopupView rewardPopupView = popupRoot.gameObject.AddComponent<RewardPopupView>();
            SetPrivateField(rewardPopupView, "_root", popupRoot.gameObject);
            SetPrivateField(rewardPopupView, "_rewardText", rewardText);
            SetPrivateField(rewardPopupView, "_completedText", completedText);
            SetPrivateField(rewardPopupView, "_continueButton", continueButton);
            popupRoot.gameObject.SetActive(false);

            // --- Non-visual systems ---
            GameObject toySpawnerGO = new GameObject("ToySpawner");
            ToySpawner toySpawner = toySpawnerGO.AddComponent<ToySpawner>();
            ToyDatabase toyDatabase = AssetDatabase.LoadAssetAtPath<ToyDatabase>(ToyDatabasePath);
            if (toyDatabase == null)
            {
                Debug.LogError($"Stage5WorkshopSceneSetup: could not load ToyDatabase at '{ToyDatabasePath}'.");
            }
            SetPrivateField(toySpawner, "_toyDatabase", toyDatabase);

            GameObject workshopControllerGO = new GameObject("WorkshopController");
            WorkshopController workshopController = workshopControllerGO.AddComponent<WorkshopController>();
            SetPrivateField(workshopController, "_toySpawner", toySpawner);
            SetPrivateField(workshopController, "_toyView", toyView);
            SetPrivateField(workshopController, "_repairDragInteraction", repairDragInteraction);
            SetPrivateField(workshopController, "_toolbarView", toolbarView);
            SetPrivateField(workshopController, "_repairHUD", repairHUD);
            SetPrivateField(workshopController, "_rewardPopupView", rewardPopupView);
            SetPrivateField(workshopController, "_coinsDisplayView", coinsDisplay);

            GameObject debugPanelGO = new GameObject("WorkshopDebugPanel");
            WorkshopDebugPanel debugPanel = debugPanelGO.AddComponent<WorkshopDebugPanel>();
            SetPrivateField(debugPanel, "_workshopController", workshopController);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog(
                "Workshop Scene Setup",
                "Done. The Workshop scene now has the full Stage 5 repair loop: 5-tool toolbar, repair HUD, reward popup with a Continue button, and an editor-only debug panel, all wired up and saved.\n\nPress Play to test.",
                "OK");

            Debug.Log("Stage5WorkshopSceneSetup: Workshop scene setup complete.");
        }

        private static void RemoveIfPresent(string name)
        {
            GameObject existing = GameObject.Find(name);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }
        }

        private static RectTransform CreateRow(Transform parent, string name, int rowIndex)
        {
            RectTransform row = CreateUIObject(name, parent);
            row.anchorMin = new Vector2(0f, 1f);
            row.anchorMax = new Vector2(1f, 1f);
            row.pivot = new Vector2(0.5f, 1f);
            row.sizeDelta = new Vector2(-40f, 45f);
            row.anchoredPosition = new Vector2(20f, -rowIndex * 48f);
            return row;
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
            buttonText = CreateText(background.transform, "Label", label, 30, TextAlignmentOptions.Center);
            return button;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                Debug.LogError($"Stage5WorkshopSceneSetup: field '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }

            field.SetValue(target, value);
        }
    }
}

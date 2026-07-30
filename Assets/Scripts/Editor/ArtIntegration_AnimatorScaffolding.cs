using System.IO;
using System.Reflection;
using ToyRepairShop.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ToyRepairShop.EditorTools
{
    /// <summary>
    /// One-time scaffolding pass: creates 4 minimal AnimatorController
    /// assets (Trigger parameters + empty placeholder states only - no
    /// actual keyframes/motion, which the user will hand-author
    /// themselves), wires an Animator component onto every GameObject
    /// that needs one and assigns it into that component's own
    /// SerializeField _animator reference, and builds the MainMenu's
    /// Help button/panel (plus a missing close button on the Settings
    /// panel). Same batch-mode-runnable, idempotent pattern as the other
    /// ArtIntegration_* tools.
    /// </summary>
    public static class ArtIntegration_AnimatorScaffolding
    {
        private const string ControllersFolder = "Assets/Animations/Controllers";
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string WorkshopScenePath = "Assets/Scenes/Workshop.unity";
        private const string UiAtlasPath = "Assets/Art/Atlas_UI_01.png";

        private static readonly string[] ToolButtonNames =
        {
            "SpongeButton", "ClothButton", "NeedleButton", "PaintRollerButton", "ScrewdriverButton",
        };

        [MenuItem("Tools/ToyRepairShop/Art/Build Animator Scaffolding + Help Menu")]
        public static void Run()
        {
            Directory.CreateDirectory(ControllersFolder);

            AnimatorController playIdle = CreateController("PlayButtonIdle", new string[0], new[] { "Idle" });
            AnimatorController toolFeedback = CreateController("ToolButtonFeedback", new[] { "Selected" }, new[] { "Idle", "Selected" });
            AnimatorController toyAdvance = CreateController("ToyAdvance", new[] { "Advance", "Complete" }, new[] { "Idle", "Advance", "Complete" });
            AnimatorController popupShow = CreateController("PopupShow", new[] { "Show" }, new[] { "Idle", "Show" });

            BuildMainMenu(playIdle);
            WireWorkshopAnimators(toolFeedback, toyAdvance, popupShow);

            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog(
                "Animator Scaffolding",
                "Created 4 AnimatorControllers (empty placeholder states only), wired Animator components into both scenes, and built the MainMenu Help button/panel.\n\nOpen each controller in the Animator window to add your own clips.",
                "OK");

            Debug.Log("ArtIntegration_AnimatorScaffolding: done.");
        }

        // ------------------------------------------------------------------
        // AnimatorController creation
        // ------------------------------------------------------------------

        private static AnimatorController CreateController(string name, string[] triggerParams, string[] stateNames)
        {
            string path = $"{ControllersFolder}/{name}.controller";

            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            foreach (string param in triggerParams)
            {
                controller.AddParameter(param, AnimatorControllerParameterType.Trigger);
            }

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

            AnimatorState idleState = null;
            foreach (string stateName in stateNames)
            {
                AnimatorState state = stateMachine.AddState(stateName);
                if (stateName == "Idle")
                {
                    idleState = state;
                    stateMachine.defaultState = state;
                }
            }

            foreach (string stateName in stateNames)
            {
                if (stateName == "Idle")
                {
                    continue;
                }

                AnimatorState target = FindState(stateMachine, stateName);
                if (target == null)
                {
                    continue;
                }

                AnimatorStateTransition enter = stateMachine.AddAnyStateTransition(target);
                enter.AddCondition(AnimatorConditionMode.If, 0f, stateName);
                enter.hasExitTime = false;
                enter.duration = 0f;

                if (idleState != null)
                {
                    AnimatorStateTransition exit = target.AddTransition(idleState);
                    exit.hasExitTime = true;
                    exit.exitTime = 1f;
                    exit.duration = 0f;
                }
            }

            return controller;
        }

        private static AnimatorState FindState(AnimatorStateMachine stateMachine, string name)
        {
            foreach (ChildAnimatorState child in stateMachine.states)
            {
                if (child.state.name == name)
                {
                    return child.state;
                }
            }

            return null;
        }

        // ------------------------------------------------------------------
        // MainMenu: Play button Animator + Help button/panel + Settings close button
        // ------------------------------------------------------------------

        private static void BuildMainMenu(AnimatorController playIdleController)
        {
            Scene scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);

            GameObject canvasGO = GameObject.Find("Canvas");
            if (canvasGO == null)
            {
                Debug.LogError("ArtIntegration_AnimatorScaffolding: no 'Canvas' found in MainMenu scene.");
                return;
            }

            GameObject playButtonGO = GameObject.Find("PlayButton");
            if (playButtonGO != null)
            {
                // Play button has no view script - the Animator alone is
                // enough; the user's default "Idle" state auto-plays.
                GetOrAddAnimator(playButtonGO, playIdleController);
            }

            RemoveChildIfExists(canvasGO.transform, "HelpButton");
            Button helpButton = CreateTextButton(canvasGO.transform, "HelpButton", "BTN_Primary_Blue", "?",
                anchor: new Vector2(1f, 1f), anchoredPosition: new Vector2(-100f, -100f), size: new Vector2(140f, 140f));

            RemoveChildIfExists(canvasGO.transform, "HelpPanel");
            GameObject helpPanelGO = BuildHelpPanel(canvasGO.transform, out Button helpCloseButton);

            // SettingsPanel is inactive by default (it's a hidden popup) -
            // GameObject.Find skips inactive objects entirely, so it must
            // be located via Transform.Find on its known parent instead.
            GameObject settingsPanelGO = canvasGO.transform.Find("SettingsPanel")?.gameObject;
            Button settingsCloseButton = null;
            if (settingsPanelGO != null)
            {
                RemoveChildIfExists(settingsPanelGO.transform, "CloseButton");
                settingsCloseButton = CreateSpriteIconButton(settingsPanelGO.transform, "CloseButton", "BTN_Close",
                    anchor: new Vector2(1f, 1f), anchoredPosition: new Vector2(-80f, -80f), size: new Vector2(110f, 110f));
            }
            else
            {
                Debug.LogWarning("ArtIntegration_AnimatorScaffolding: no 'SettingsPanel' found in MainMenu scene.");
            }

            GameObject mainMenuUIGO = GameObject.Find("MainMenuUI");
            if (mainMenuUIGO != null)
            {
                MainMenuUI mainMenuUI = mainMenuUIGO.GetComponent<MainMenuUI>();
                if (mainMenuUI != null)
                {
                    SetPrivateField(mainMenuUI, "_helpButton", helpButton);
                    SetPrivateField(mainMenuUI, "_helpPanel", helpPanelGO);
                    SetPrivateField(mainMenuUI, "_helpCloseButton", helpCloseButton);
                    SetPrivateField(mainMenuUI, "_settingsCloseButton", settingsCloseButton);
                }
            }
            else
            {
                Debug.LogError("ArtIntegration_AnimatorScaffolding: no 'MainMenuUI' found in MainMenu scene.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static GameObject BuildHelpPanel(Transform canvasTransform, out Button closeButton)
        {
            RectTransform panelRect = CreateUIObject("HelpPanel", canvasTransform);
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(800f, 1000f);
            panelRect.anchoredPosition = Vector2.zero;

            Image panelImage = panelRect.gameObject.AddComponent<Image>();
            panelImage.sprite = AtlasGridSlicer.LoadSprite(UiAtlasPath, "Panel_Tall");
            panelImage.type = Image.Type.Sliced;

            RectTransform titleRect = CreateUIObject("Title", panelRect);
            titleRect.anchorMin = new Vector2(0f, 0.86f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            TextMeshProUGUI titleText = CreateText(titleRect, "TitleText", "How To Play", 54, TextAlignmentOptions.Center);
            titleText.color = Color.black;

            RectTransform bodyRect = CreateUIObject("Body", panelRect);
            bodyRect.anchorMin = new Vector2(0.08f, 0.15f);
            bodyRect.anchorMax = new Vector2(0.92f, 0.84f);
            bodyRect.offsetMin = Vector2.zero;
            bodyRect.offsetMax = Vector2.zero;
            TextMeshProUGUI bodyText = CreateText(
                bodyRect,
                "BodyText",
                "1. Tap PLAY to enter the workshop\n\n" +
                "2. See what the toy needs in the HUD\n\n" +
                "3. Pick the matching tool at the bottom\n\n" +
                "4. Drag on the toy to repair it\n\n" +
                "5. Repeat until the toy is fully fixed!",
                34,
                TextAlignmentOptions.TopLeft);
            bodyText.color = Color.black;

            closeButton = CreateSpriteIconButton(panelRect, "CloseButton", "BTN_Close",
                anchor: new Vector2(1f, 1f), anchoredPosition: new Vector2(-70f, -70f), size: new Vector2(110f, 110f));

            panelRect.gameObject.SetActive(false);
            return panelRect.gameObject;
        }

        // ------------------------------------------------------------------
        // Workshop: Animator on toolbar buttons / toy / popup
        // ------------------------------------------------------------------

        private static void WireWorkshopAnimators(AnimatorController toolFeedback, AnimatorController toyAdvance, AnimatorController popupShow)
        {
            Scene scene = EditorSceneManager.OpenScene(WorkshopScenePath, OpenSceneMode.Single);

            foreach (string buttonName in ToolButtonNames)
            {
                GameObject buttonGO = GameObject.Find(buttonName);
                if (buttonGO == null)
                {
                    Debug.LogWarning($"ArtIntegration_AnimatorScaffolding: could not find '{buttonName}' in the Workshop scene.");
                    continue;
                }

                Animator animator = GetOrAddAnimator(buttonGO, toolFeedback);
                ToolButtonView view = buttonGO.GetComponent<ToolButtonView>();
                if (view != null)
                {
                    SetPrivateField(view, "_animator", animator);
                }
            }

            GameObject toyAreaGO = GameObject.Find("ToyArea");
            if (toyAreaGO != null)
            {
                Animator animator = GetOrAddAnimator(toyAreaGO, toyAdvance);
                ToyView view = toyAreaGO.GetComponent<ToyView>();
                if (view != null)
                {
                    SetPrivateField(view, "_animator", animator);
                }
            }
            else
            {
                Debug.LogWarning("ArtIntegration_AnimatorScaffolding: no 'ToyArea' found in Workshop scene.");
            }

            // PopupPanel is inactive by default (it's a hidden popup) -
            // GameObject.Find skips inactive objects entirely, so it must
            // be located via Transform.Find on its known parent instead.
            GameObject canvasGO = GameObject.Find("Canvas");
            GameObject popupPanelGO = canvasGO != null ? canvasGO.transform.Find("PopupPanel")?.gameObject : null;
            if (popupPanelGO != null)
            {
                Animator animator = GetOrAddAnimator(popupPanelGO, popupShow);
                RewardPopupView view = popupPanelGO.GetComponent<RewardPopupView>();
                if (view != null)
                {
                    SetPrivateField(view, "_animator", animator);
                }
            }
            else
            {
                Debug.LogWarning("ArtIntegration_AnimatorScaffolding: no 'PopupPanel' found in Workshop scene.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // ------------------------------------------------------------------
        // Small UI-building helpers (duplicated per established per-tool convention)
        // ------------------------------------------------------------------

        private static Animator GetOrAddAnimator(GameObject go, AnimatorController controller)
        {
            Animator animator = go.GetComponent<Animator>();
            if (animator == null)
            {
                animator = go.AddComponent<Animator>();
            }

            animator.runtimeAnimatorController = controller;
            return animator;
        }

        private static void RemoveChildIfExists(Transform parent, string childName)
        {
            Transform existing = parent.Find(childName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }
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

        /// <summary>Creates a button whose background is a sprite from Atlas_UI_01, anchored at a corner/edge.</summary>
        private static Button CreateSpriteIconButton(Transform parent, string name, string spriteName, Vector2 anchor, Vector2 anchoredPosition, Vector2 size)
        {
            Image background = CreateImage(parent, name, Color.white);
            RectTransform rect = background.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            background.sprite = AtlasGridSlicer.LoadSprite(UiAtlasPath, spriteName);
            background.preserveAspect = true;

            Button button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            return button;
        }

        /// <summary>Creates a button whose background is a sprite from Atlas_UI_01, with a text label on top (for actions with no dedicated icon, like Help).</summary>
        private static Button CreateTextButton(Transform parent, string name, string spriteName, string label, Vector2 anchor, Vector2 anchoredPosition, Vector2 size)
        {
            Image background = CreateImage(parent, name, Color.white);
            RectTransform rect = background.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            background.sprite = AtlasGridSlicer.LoadSprite(UiAtlasPath, spriteName);
            background.preserveAspect = true;

            TextMeshProUGUI text = CreateText(background.transform, "Label", label, 60, TextAlignmentOptions.Center);
            text.color = Color.white;
            text.fontStyle = FontStyles.Bold;

            Button button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            return button;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                Debug.LogError($"ArtIntegration_AnimatorScaffolding: field '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }

            field.SetValue(target, value);
        }
    }
}

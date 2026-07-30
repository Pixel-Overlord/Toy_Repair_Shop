using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ToyRepairShop.EditorTools
{
    /// <summary>
    /// Diagnostic-only: renders the currently open scene's Canvas (Screen
    /// Space - Overlay) to a PNG via an offscreen camera, so UI layout
    /// can be visually checked from the command line without opening the
    /// Editor's Game view. Not part of the shipped art pipeline.
    /// </summary>
    public static class ScenePreviewRenderer
    {
        [MenuItem("Tools/ToyRepairShop/Art/Render Workshop Preview")]
        public static void RenderWorkshopPreview()
        {
            RenderScenePreview("Assets/Scenes/Workshop.unity", "Assets/Art/_Debug/Workshop_preview.png");
        }

        [MenuItem("Tools/ToyRepairShop/Art/Render MainMenu Preview")]
        public static void RenderMainMenuPreview()
        {
            RenderScenePreview("Assets/Scenes/MainMenu.unity", "Assets/Art/_Debug/MainMenu_preview.png");
        }

        private static void RenderScenePreview(string scenePath, string outputPath)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError($"ScenePreviewRenderer: no Canvas found in '{scenePath}'.");
                return;
            }

            // Force a screen-space canvas to lay itself out and render via
            // a temporary camera, regardless of its normal render mode.
            RenderMode originalMode = canvas.renderMode;
            Camera originalWorldCamera = canvas.worldCamera;

            const int width = 1080;
            const int height = 1920;

            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);

            GameObject cameraGO = new GameObject("PreviewCamera");
            Camera previewCamera = cameraGO.AddComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            previewCamera.orthographic = true;
            previewCamera.targetTexture = renderTexture;
            previewCamera.cullingMask = ~0;

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = previewCamera;
            canvas.planeDistance = 1f;
            previewCamera.nearClipPlane = 0.01f;
            previewCamera.farClipPlane = 10f;

            Canvas.ForceUpdateCanvases();

            RenderTexture previousActive = RenderTexture.active;
            previewCamera.Render();

            RenderTexture.active = renderTexture;
            var screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenshot.Apply();
            RenderTexture.active = previousActive;

            byte[] png = screenshot.EncodeToPNG();
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            File.WriteAllBytes(outputPath, png);

            Object.DestroyImmediate(screenshot);
            Object.DestroyImmediate(cameraGO);
            renderTexture.Release();
            Object.DestroyImmediate(renderTexture);

            // Restore the canvas to how the scene had it, then discard
            // these runtime-only changes rather than saving them.
            canvas.renderMode = originalMode;
            canvas.worldCamera = originalWorldCamera;

            AssetDatabase.ImportAsset(outputPath);
            Debug.Log($"ScenePreviewRenderer: wrote '{outputPath}'.");
        }
    }
}

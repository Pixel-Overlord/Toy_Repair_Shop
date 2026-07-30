using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ToyRepairShop.EditorTools
{
    /// <summary>
    /// Diagnostic-only tools used while tuning the atlas slicing
    /// pipeline. Not part of the shipped art pipeline - safe to delete
    /// once slicing parameters are finalized.
    /// </summary>
    public static class ArtIntegration_ThresholdProbe
    {
        [MenuItem("Tools/ToyRepairShop/Art/Probe UI Atlas Alpha Thresholds")]
        public static void ProbeUIAlpha()
        {
            int[] thresholds = { 40, 100, 150, 180, 200, 220, 240, 250, 253 };
            foreach (int t in thresholds)
            {
                var blobs = AlphaBoundsSlicer.DetectBlobs("Assets/Art/Atlas_UI_01.png", (byte)t, minSize: 40);
                Debug.Log($"PROBE UI alpha={t} -> {blobs.Count} blobs");
            }
        }

        [MenuItem("Tools/ToyRepairShop/Art/Contact Sheet UI Atlas (minSize 40)")]
        public static void ContactSheetUI()
        {
            var blobs = AlphaBoundsSlicer.DetectBlobs("Assets/Art/Atlas_UI_01.png", alphaThreshold: 253, minSize: 40);
            AlphaBoundsSlicer.WriteContactSheet("Assets/Art/Atlas_UI_01.png", blobs, "Assets/Art/_Debug/Atlas_UI_01_contactsheet.png", columns: 6);
            Debug.Log($"CONTACTSHEET UI wrote {blobs.Count} blobs");
        }

        [MenuItem("Tools/ToyRepairShop/Art/Probe Toys Atlas Thresholds")]
        public static void ProbeToys()
        {
            int[] thresholds = { 40, 80, 120, 160, 200, 230, 250 };
            foreach (int t in thresholds)
            {
                var blobs = AlphaBoundsSlicer.DetectBlobs("Assets/Art/Atlas_Toys_01.png", (byte)t, minSize: 40);
                Debug.Log($"PROBE Toys threshold={t} -> {blobs.Count} blobs");
            }
        }

        [MenuItem("Tools/ToyRepairShop/Art/Probe UI Atlas MinSize")]
        public static void ProbeUI()
        {
            int[] minSizes = { 10, 20, 30, 40, 50, 60, 70, 80 };
            foreach (int m in minSizes)
            {
                var blobs = AlphaBoundsSlicer.DetectBlobs("Assets/Art/Atlas_UI_01.png", alphaThreshold: 40, minSize: m);
                Debug.Log($"PROBE UI minSize={m} -> {blobs.Count} blobs");
            }
        }

        [MenuItem("Tools/ToyRepairShop/Art/Dump Alpha Stats")]
        public static void DumpAlphaStats()
        {
            DumpForTexture("Assets/Art/Atlas_Toys_01.png");
            DumpForTexture("Assets/Art/Atlas_Tools_01.png");
            DumpForTexture("Assets/Art/Atlas_UI_01.png");
            DumpForTexture("Assets/Art/Atlas_Effects_01.png");
            DumpForTexture("Assets/Art/Environment Atlas.png");
        }

        [MenuItem("Tools/ToyRepairShop/Art/Preview Toys Grid Slice")]
        public static void PreviewToysGridSlice()
        {
            const string path = "Assets/Art/Atlas_Toys_01.png";
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            int columns = 6, rows = 3;
            float cellWidth = texture.width / (float)columns;
            float cellHeight = texture.height / (float)rows;

            var blobs = new List<RectInt>();
            for (int row = 0; row < rows; row++)
            {
                float yTopInImage = row * cellHeight;
                float yUnity = texture.height - yTopInImage - cellHeight;
                for (int col = 0; col < columns; col++)
                {
                    blobs.Add(new RectInt(Mathf.RoundToInt(col * cellWidth), Mathf.RoundToInt(yUnity), Mathf.RoundToInt(cellWidth), Mathf.RoundToInt(cellHeight)));
                }
            }

            AlphaBoundsSlicer.WriteContactSheet(path, blobs, "Assets/Art/_Debug/Atlas_Toys_01_gridpreview.png", columns);
            Debug.Log("PREVIEW wrote grid preview for Toys atlas.");
        }

        private static void DumpForTexture(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            bool wasReadable = importer.isReadable;
            if (!wasReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Color32[] pixels = tex.GetPixels32();

            int min = 255, max = 0;
            long sum = 0;
            int[] histogram = new int[256];
            foreach (Color32 p in pixels)
            {
                if (p.a < min) min = p.a;
                if (p.a > max) max = p.a;
                sum += p.a;
                histogram[p.a]++;
            }

            double avg = sum / (double)pixels.Length;
            Debug.Log($"ALPHASTATS {path}: min={min} max={max} avg={avg:F2} totalPixels={pixels.Length}");
            Debug.Log($"ALPHASTATS {path}: count(a==0)={histogram[0]} count(a==255)={histogram[255]} count(a<10)={SumRange(histogram, 0, 9)} count(a>245)={SumRange(histogram, 246, 255)}");

            int w = tex.width, h = tex.height;
            LogPixel(pixels, w, h, 0, 0, "top-left");
            LogPixel(pixels, w, h, w - 1, 0, "top-right");
            LogPixel(pixels, w, h, w / 2, h / 2, "center");
            LogPixel(pixels, w, h, 5, 5, "near-top-left");

            if (!wasReadable)
            {
                importer.isReadable = false;
                importer.SaveAndReimport();
            }
        }

        private static int SumRange(int[] histogram, int from, int to)
        {
            int sum = 0;
            for (int i = from; i <= to; i++)
            {
                sum += histogram[i];
            }
            return sum;
        }

        private static void LogPixel(Color32[] pixels, int width, int height, int x, int y, string label)
        {
            int index = y * width + x;
            Color32 p = pixels[index];
            Debug.Log($"ALPHASTATS pixel[{label}] ({x},{y}) = r{p.r} g{p.g} b{p.b} a{p.a}");
        }
    }
}

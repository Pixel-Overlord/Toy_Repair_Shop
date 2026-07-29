using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ToyRepairShop.EditorTools
{
    /// <summary>
    /// Content-aware sprite slicing: finds each icon's actual pixel
    /// bounds via connected-component analysis of the alpha channel,
    /// instead of assuming a perfectly uniform grid. Needed because these
    /// atlases have inconsistent padding between icons, which broke naive
    /// grid-math slicing (AtlasGridSlicer).
    /// </summary>
    public static class AlphaBoundsSlicer
    {
        /// <summary>
        /// Finds all connected non-transparent regions in the texture at
        /// texturePath, discards any smaller than minSize in either
        /// dimension, and returns their bounding rects sorted in reading
        /// order (top-to-bottom rows, left-to-right within a row).
        /// </summary>
        public static List<RectInt> DetectBlobs(string texturePath, byte alphaThreshold, int minSize)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
            bool wasReadable = importer.isReadable;

            if (!wasReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            Color32[] pixels = texture.GetPixels32();
            int width = texture.width;
            int height = texture.height;

            var visited = new bool[width * height];
            var blobs = new List<RectInt>();
            var stack = new Stack<int>();

            for (int startIndex = 0; startIndex < pixels.Length; startIndex++)
            {
                if (visited[startIndex] || pixels[startIndex].a < alphaThreshold)
                {
                    continue;
                }

                int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
                stack.Push(startIndex);
                visited[startIndex] = true;

                while (stack.Count > 0)
                {
                    int index = stack.Pop();
                    int x = index % width;
                    int y = index / width;

                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;

                    TryPush(x + 1, y, width, height, pixels, alphaThreshold, visited, stack);
                    TryPush(x - 1, y, width, height, pixels, alphaThreshold, visited, stack);
                    TryPush(x, y + 1, width, height, pixels, alphaThreshold, visited, stack);
                    TryPush(x, y - 1, width, height, pixels, alphaThreshold, visited, stack);
                }

                int blobWidth = maxX - minX + 1;
                int blobHeight = maxY - minY + 1;

                if (blobWidth >= minSize && blobHeight >= minSize)
                {
                    blobs.Add(new RectInt(minX, minY, blobWidth, blobHeight));
                }
            }

            if (!wasReadable)
            {
                importer.isReadable = false;
                importer.SaveAndReimport();
            }

            blobs.Sort((a, b) =>
            {
                // Treat blobs whose vertical centers are within half a
                // typical icon height as being "the same row".
                int rowTolerance = Mathf.Max(a.height, b.height) / 2;
                int aCenterY = a.y + a.height / 2;
                int bCenterY = b.y + b.height / 2;

                if (Mathf.Abs(aCenterY - bCenterY) > rowTolerance)
                {
                    // Image Y is bottom-up in Unity's texture space, so a
                    // larger Y is higher up / an earlier row when reading top-to-bottom.
                    return bCenterY.CompareTo(aCenterY);
                }

                return a.x.CompareTo(b.x);
            });

            Debug.Log($"AlphaBoundsSlicer: detected {blobs.Count} blobs in '{texturePath}' (alphaThreshold={alphaThreshold}, minSize={minSize}).");
            return blobs;
        }

        private static void TryPush(int x, int y, int width, int height, Color32[] pixels, byte alphaThreshold, bool[] visited, Stack<int> stack)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return;
            }

            int index = y * width + x;
            if (visited[index])
            {
                return;
            }

            if (pixels[index].a < alphaThreshold)
            {
                return;
            }

            visited[index] = true;
            stack.Push(index);
        }

        /// <summary>
        /// Applies the given blobs as named sprites on the texture via
        /// the TextureImporter, in the same order as names. blobs.Count
        /// must equal names.Length.
        /// </summary>
        public static void ApplySprites(string texturePath, List<RectInt> blobs, string[] names)
        {
            if (blobs.Count != names.Length)
            {
                Debug.LogError($"AlphaBoundsSlicer: '{texturePath}' detected {blobs.Count} blobs but {names.Length} names were provided. Aborting - adjust the threshold/minSize or the name list.");
                return;
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;

            var metas = new List<SpriteMetaData>(blobs.Count);
            for (int i = 0; i < blobs.Count; i++)
            {
                RectInt blob = blobs[i];
                metas.Add(new SpriteMetaData
                {
                    name = names[i],
                    rect = new Rect(blob.x, blob.y, blob.width, blob.height),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                });
            }

#pragma warning disable 618
            importer.spritesheet = metas.ToArray();
#pragma warning restore 618

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();

            Debug.Log($"AlphaBoundsSlicer: applied {blobs.Count} named sprites to '{texturePath}'.");
        }

        /// <summary>
        /// Writes a debug PNG tiling every detected blob (in order) into
        /// a grid with a thin red border per cell, so slicing can be
        /// visually verified without opening the Sprite Editor.
        /// </summary>
        public static void WriteContactSheet(string texturePath, List<RectInt> blobs, string outputPath, int columns)
        {
            Texture2D source = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

            var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
            bool wasReadable = importer.isReadable;
            if (!wasReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
                source = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            }

            int cellSize = 0;
            foreach (RectInt blob in blobs)
            {
                cellSize = Mathf.Max(cellSize, blob.width, blob.height);
            }
            cellSize += 8; // padding

            int rows = Mathf.CeilToInt(blobs.Count / (float)columns);
            var sheet = new Texture2D(columns * cellSize, rows * cellSize, TextureFormat.RGBA32, false);

            var clear = new Color32(30, 30, 30, 255);
            var fillPixels = new Color32[sheet.width * sheet.height];
            for (int i = 0; i < fillPixels.Length; i++)
            {
                fillPixels[i] = clear;
            }
            sheet.SetPixels32(fillPixels);

            for (int i = 0; i < blobs.Count; i++)
            {
                RectInt blob = blobs[i];
                Color[] cropped = source.GetPixels(blob.x, blob.y, blob.width, blob.height);

                int col = i % columns;
                int row = i / columns;
                int destX = col * cellSize + 4;
                int destY = sheet.height - (row + 1) * cellSize + 4;

                sheet.SetPixels(destX, destY, blob.width, blob.height, cropped);

                DrawBorder(sheet, col * cellSize, sheet.height - (row + 1) * cellSize, cellSize, cellSize, new Color32(255, 60, 60, 255));
            }

            sheet.Apply();

            byte[] png = sheet.EncodeToPNG();
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            File.WriteAllBytes(outputPath, png);
            Object.DestroyImmediate(sheet);

            if (!wasReadable)
            {
                importer.isReadable = false;
                importer.SaveAndReimport();
            }

            AssetDatabase.ImportAsset(outputPath);
            Debug.Log($"AlphaBoundsSlicer: wrote contact sheet to '{outputPath}' ({blobs.Count} blobs, {columns} columns).");
        }

        private static void DrawBorder(Texture2D tex, int x, int y, int width, int height, Color32 color)
        {
            for (int i = 0; i < width; i++)
            {
                tex.SetPixel(x + i, y, color);
                tex.SetPixel(x + i, y + height - 1, color);
            }

            for (int j = 0; j < height; j++)
            {
                tex.SetPixel(x, y + j, color);
                tex.SetPixel(x + width - 1, y + j, color);
            }
        }
    }
}

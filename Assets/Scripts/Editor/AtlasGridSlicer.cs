using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ToyRepairShop.EditorTools
{
    /// <summary>
    /// Slices a texture on a uniform grid into named sprites, using the
    /// TextureImporter's legacy spritesheet API (still functional in
    /// 2022.3 LTS, just marked obsolete in favor of the 2D Sprite
    /// package's data-provider API). Reusable across atlases that are
    /// laid out as clean, evenly-sized grids.
    /// </summary>
    public static class AtlasGridSlicer
    {
        /// <summary>
        /// Slices texturePath into columns x rows equal cells and names
        /// them from names, in row-major order starting at the top-left
        /// of the image (English reading order). names.Length must equal
        /// columns * rows.
        /// </summary>
        public static void SliceGrid(string texturePath, int columns, int rows, string[] names)
        {
            if (names.Length != columns * rows)
            {
                Debug.LogError($"AtlasGridSlicer: '{texturePath}' expected {columns * rows} names for a {columns}x{rows} grid, got {names.Length}.");
                return;
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
            if (importer == null)
            {
                Debug.LogError($"AtlasGridSlicer: no TextureImporter found at '{texturePath}'.");
                return;
            }

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture == null)
            {
                Debug.LogError($"AtlasGridSlicer: could not load Texture2D at '{texturePath}'.");
                return;
            }

            float cellWidth = texture.width / (float)columns;
            float cellHeight = texture.height / (float)rows;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;

            var metas = new List<SpriteMetaData>(columns * rows);
            int index = 0;

            for (int row = 0; row < rows; row++)
            {
                // Image row 0 is the top of the image; Unity sprite rects
                // are measured from the bottom of the texture, so flip Y.
                float yTopInImage = row * cellHeight;
                float yUnity = texture.height - yTopInImage - cellHeight;

                for (int col = 0; col < columns; col++)
                {
                    float xLeft = col * cellWidth;

                    metas.Add(new SpriteMetaData
                    {
                        name = names[index],
                        rect = new Rect(xLeft, yUnity, cellWidth, cellHeight),
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f),
                    });

                    index++;
                }
            }

#pragma warning disable 618
            importer.spritesheet = metas.ToArray();
#pragma warning restore 618

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();

            Debug.Log($"AtlasGridSlicer: sliced '{texturePath}' into {columns * rows} sprites.");
        }

        /// <summary>Loads a specific named sprite that was produced by SliceGrid (or any Multiple-mode texture) at texturePath.</summary>
        public static Sprite LoadSprite(string texturePath, string spriteName)
        {
            foreach (Object asset in AssetDatabase.LoadAllAssetRepresentationsAtPath(texturePath))
            {
                if (asset is Sprite sprite && sprite.name == spriteName)
                {
                    return sprite;
                }
            }

            Debug.LogError($"AtlasGridSlicer: sprite '{spriteName}' not found in '{texturePath}'. Did you slice it first?");
            return null;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace ToyRepairShop.Data.ScriptableObjects
{
    /// <summary>
    /// Static catalogue of every ToolData asset in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "ToolDatabase", menuName = "ToyRepairShop/Tool Database")]
    public sealed class ToolDatabase : ScriptableObject
    {
        [SerializeField, Tooltip("Every tool available in the game.")]
        private List<ToolData> _tools = new List<ToolData>();

        public IReadOnlyList<ToolData> Tools => _tools;

        /// <summary>Finds a tool by its Tool ID, or null if none matches.</summary>
        public ToolData GetToolById(string toolId)
        {
            for (int i = 0; i < _tools.Count; i++)
            {
                if (_tools[i] != null && _tools[i].ToolId == toolId)
                {
                    return _tools[i];
                }
            }

            return null;
        }

        private void OnValidate()
        {
            var seenIds = new HashSet<string>();
            foreach (ToolData tool in _tools)
            {
                if (tool == null)
                {
                    continue;
                }

                if (!seenIds.Add(tool.ToolId))
                {
                    Debug.LogWarning($"ToolDatabase '{name}' contains a duplicate Tool ID: '{tool.ToolId}'.", this);
                }
            }
        }
    }
}

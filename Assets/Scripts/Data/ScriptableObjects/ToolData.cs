using UnityEngine;

namespace ToyRepairShop.Data.ScriptableObjects
{
    /// <summary>
    /// Static definition of a tool the player can own and use during repairs.
    /// </summary>
    [CreateAssetMenu(fileName = "ToolData", menuName = "ToyRepairShop/Tool Data")]
    public sealed class ToolData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField, Tooltip("Unique identifier used to look up this tool at runtime.")]
        private string _toolId;

        [SerializeField, Tooltip("Display name shown to the player.")]
        private string _toolName;

        [SerializeField, TextArea(2, 4)]
        private string _description;

        [Header("Presentation")]
        [SerializeField] private Sprite _icon;

        [Header("Unlock")]
        [SerializeField, Tooltip("Whether this tool starts locked and must be unlocked by the player.")]
        private bool _locked;

        [SerializeField, Tooltip("Coin cost to unlock this tool, if locked.")]
        [Min(0)]
        private int _unlockCost;

        public string ToolId => _toolId;
        public string ToolName => _toolName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public bool Locked => _locked;
        public int UnlockCost => _unlockCost;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(_toolId))
            {
                Debug.LogWarning($"ToolData '{name}' has no Tool ID assigned.", this);
            }
        }
    }
}

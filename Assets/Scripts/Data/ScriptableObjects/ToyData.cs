using System.Collections.Generic;
using ToyRepairShop.Data.Enums;
using UnityEngine;

namespace ToyRepairShop.Data.ScriptableObjects
{
    /// <summary>
    /// Static definition of a toy. A new toy is added to the game by
    /// creating one of these assets - no code changes required.
    /// </summary>
    [CreateAssetMenu(fileName = "ToyData", menuName = "ToyRepairShop/Toy Data")]
    public sealed class ToyData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField, Tooltip("Unique identifier used to look up this toy at runtime. Must be unique across all ToyData assets.")]
        private string _toyId;

        [SerializeField, Tooltip("Display name shown to the player.")]
        private string _toyName;

        [SerializeField, TextArea(2, 5)]
        private string _description;

        [Header("Classification")]
        [SerializeField] private ToyCategory _category;
        [SerializeField] private Difficulty _difficulty;

        [Header("Visuals")]
        [SerializeField] private Sprite _thumbnailSprite;
        [SerializeField] private Sprite _brokenSprite;
        [SerializeField] private Sprite _repairedSprite;

        [Header("Progression")]
        [SerializeField, Tooltip("Coins granted to the player when this toy is fully repaired.")]
        [Min(0)]
        private int _rewardCoins;

        [SerializeField, Tooltip("Player level required before this toy becomes available.")]
        [Min(0)]
        private int _unlockLevel;

        [SerializeField, Tooltip("Expected time, in seconds, to fully repair this toy.")]
        [Min(0f)]
        private float _estimatedRepairTime;

        [Header("Repair Sequence")]
        [SerializeField, Tooltip("Repair steps required to fully repair this toy, in order.")]
        private List<RepairStepData> _requiredRepairSteps = new List<RepairStepData>();

        public string ToyId => _toyId;
        public string ToyName => _toyName;
        public string Description => _description;
        public ToyCategory Category => _category;
        public Difficulty Difficulty => _difficulty;
        public Sprite ThumbnailSprite => _thumbnailSprite;
        public Sprite BrokenSprite => _brokenSprite;
        public Sprite RepairedSprite => _repairedSprite;
        public int RewardCoins => _rewardCoins;
        public int UnlockLevel => _unlockLevel;
        public float EstimatedRepairTime => _estimatedRepairTime;
        public IReadOnlyList<RepairStepData> RequiredRepairSteps => _requiredRepairSteps;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(_toyId))
            {
                Debug.LogWarning($"ToyData '{name}' has no Toy ID assigned.", this);
            }
        }
    }
}

using ToyRepairShop.Data.Enums;
using UnityEngine;

namespace ToyRepairShop.Data.ScriptableObjects
{
    /// <summary>
    /// Static definition of a single repair step that can be attached to
    /// one or more ToyData assets.
    /// </summary>
    [CreateAssetMenu(fileName = "RepairStepData", menuName = "ToyRepairShop/Repair Step Data")]
    public sealed class RepairStepData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField, Tooltip("Unique identifier used to look up this step at runtime.")]
        private string _stepId;

        [SerializeField, Tooltip("Display name shown to the player.")]
        private string _stepName;

        [Header("Repair")]
        [SerializeField] private RepairType _repairType;

        [SerializeField, Tooltip("Type of tool required to perform this step.")]
        private ToolType _toolRequired;

        [SerializeField, Tooltip("Time, in seconds, this step takes to complete.")]
        [Min(0f)]
        private float _duration;

        [Header("Presentation")]
        [SerializeField] private Sprite _stepIcon;

        [SerializeField, TextArea(1, 3)]
        private string _successMessage;

        public string StepId => _stepId;
        public string StepName => _stepName;
        public RepairType RepairType => _repairType;
        public ToolType ToolRequired => _toolRequired;
        public float Duration => _duration;
        public Sprite StepIcon => _stepIcon;
        public string SuccessMessage => _successMessage;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(_stepId))
            {
                Debug.LogWarning($"RepairStepData '{name}' has no Step ID assigned.", this);
            }
        }
    }
}

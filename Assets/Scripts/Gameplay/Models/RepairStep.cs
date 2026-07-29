using ToyRepairShop.Data.ScriptableObjects;

namespace ToyRepairShop.Gameplay.Models
{
    /// <summary>
    /// Runtime state for a single repair step within an in-progress Toy.
    /// </summary>
    public sealed class RepairStep
    {
        public RepairStepData Data { get; }
        public bool IsCompleted { get; private set; }
        public float Progress { get; private set; }

        public RepairStep(RepairStepData data)
        {
            Data = data;
        }

        public void SetProgress(float progress)
        {
            Progress = progress;
        }

        public void Complete()
        {
            IsCompleted = true;
            Progress = 1f;
        }
    }
}

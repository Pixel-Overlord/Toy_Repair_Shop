using System.Collections.Generic;
using ToyRepairShop.Data.Enums;
using ToyRepairShop.Data.ScriptableObjects;

namespace ToyRepairShop.Gameplay.Models
{
    /// <summary>
    /// Runtime state for a toy being repaired. Wraps a static ToyData asset
    /// with mutable progress that only exists for the lifetime of a play
    /// session - it is never serialized as a ScriptableObject itself.
    /// </summary>
    public sealed class Toy
    {
        public ToyData Data { get; }
        public float RepairProgress { get; private set; }
        public ToyState State { get; private set; }
        public IReadOnlyList<RepairStep> CompletedSteps => _completedSteps;

        private readonly List<RepairStep> _completedSteps = new List<RepairStep>();

        public Toy(ToyData data)
        {
            Data = data;
            State = ToyState.Available;
        }

        public void SetState(ToyState state)
        {
            State = state;
        }

        public void SetProgress(float progress)
        {
            RepairProgress = progress;
        }

        public void AddCompletedStep(RepairStep step)
        {
            if (step != null && !_completedSteps.Contains(step))
            {
                _completedSteps.Add(step);
            }
        }
    }
}

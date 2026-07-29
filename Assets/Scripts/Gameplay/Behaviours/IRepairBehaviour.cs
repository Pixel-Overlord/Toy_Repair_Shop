using System;
using ToyRepairShop.Data.Enums;
using ToyRepairShop.Gameplay.Models;

namespace ToyRepairShop.Gameplay.Behaviours
{
    /// <summary>
    /// Strategy interface for how a single repair step's progress is
    /// driven. RepairController talks to steps only through this
    /// interface - it never branches on RepairType itself.
    /// </summary>
    public interface IRepairBehaviour
    {
        RepairType RepairType { get; }
        bool IsActive { get; }

        /// <summary>Raised with the new progress value (0-1) whenever it changes.</summary>
        event Action<float> ProgressChanged;

        /// <summary>Raised once progress reaches 1 (or Complete() is called directly).</summary>
        event Action Completed;

        /// <summary>Starts driving the given step from zero progress.</summary>
        void Begin(RepairStep step);

        /// <summary>Adds delta (can be negative) to progress, clamped to 0-1.</summary>
        void UpdateProgress(float delta);

        /// <summary>Abandons the in-progress attempt, resetting progress to zero.</summary>
        void Cancel();

        /// <summary>Forces the step to 100% and raises Completed.</summary>
        void Complete();
    }
}

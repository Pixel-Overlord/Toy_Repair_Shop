using System;
using ToyRepairShop.Data.Enums;
using ToyRepairShop.Gameplay.Models;
using UnityEngine;

namespace ToyRepairShop.Gameplay.Behaviours
{
    /// <summary>
    /// Shared generic progress-accumulation mechanic used by every repair
    /// behaviour, so each concrete class only has to declare which
    /// RepairType it handles. Wash is the one behaviour this mechanic was
    /// actually built and proven for (Stage 4's drag interaction); every
    /// other subclass reuses it as a placeholder until it gets a unique
    /// mechanic of its own.
    /// </summary>
    public abstract class RepairBehaviourBase : IRepairBehaviour
    {
        public abstract RepairType RepairType { get; }
        public bool IsActive { get; private set; }

        public event Action<float> ProgressChanged;
        public event Action Completed;

        protected RepairStep Step { get; private set; }
        protected float Progress { get; private set; }

        public virtual void Begin(RepairStep step)
        {
            Step = step;
            Progress = 0f;
            IsActive = true;
        }

        public virtual void UpdateProgress(float delta)
        {
            if (!IsActive)
            {
                return;
            }

            Progress = Mathf.Clamp01(Progress + delta);
            ProgressChanged?.Invoke(Progress);

            if (Progress >= 1f)
            {
                Complete();
            }
        }

        public virtual void Cancel()
        {
            IsActive = false;
            Progress = 0f;
        }

        public virtual void Complete()
        {
            if (!IsActive)
            {
                return;
            }

            IsActive = false;
            Progress = 1f;
            Completed?.Invoke();
        }
    }
}

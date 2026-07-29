using System;
using System.Collections.Generic;
using ToyRepairShop.Data.Enums;

namespace ToyRepairShop.Gameplay.Controllers
{
    /// <summary>
    /// Finite state machine for a single toy's repair session. Only allows
    /// the transitions listed below - anything else is rejected rather
    /// than silently applied, so RepairController can never put the
    /// session into an inconsistent state.
    /// </summary>
    public sealed class RepairStateMachine
    {
        private static readonly Dictionary<RepairState, RepairState[]> ValidTransitions = new Dictionary<RepairState, RepairState[]>
        {
            { RepairState.Idle, new[] { RepairState.ToolSelection, RepairState.ToyCompleted } },
            { RepairState.ToolSelection, new[] { RepairState.Repairing } },
            { RepairState.Repairing, new[] { RepairState.StepCompleted, RepairState.ToolSelection } },
            { RepairState.StepCompleted, new[] { RepairState.ToolSelection, RepairState.ToyCompleted } },
            { RepairState.ToyCompleted, new[] { RepairState.Idle } },
        };

        /// <summary>Raised after a successful transition, with (previous, current).</summary>
        public event Action<RepairState, RepairState> StateChanged;

        public RepairState Current { get; private set; } = RepairState.Idle;

        public bool CanTransitionTo(RepairState next)
        {
            return ValidTransitions.TryGetValue(Current, out RepairState[] allowed) && Array.IndexOf(allowed, next) >= 0;
        }

        /// <summary>Attempts the transition. Returns false and makes no change if it isn't valid from the current state.</summary>
        public bool TransitionTo(RepairState next)
        {
            if (!CanTransitionTo(next))
            {
                return false;
            }

            RepairState previous = Current;
            Current = next;
            StateChanged?.Invoke(previous, next);
            return true;
        }
    }
}

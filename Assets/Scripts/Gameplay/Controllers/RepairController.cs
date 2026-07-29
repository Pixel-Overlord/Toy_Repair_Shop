using System;
using ToyRepairShop.Data.Enums;
using ToyRepairShop.Gameplay.Behaviours;
using ToyRepairShop.Gameplay.Models;

namespace ToyRepairShop.Gameplay.Controllers
{
    /// <summary>
    /// Orchestrates the repair of a single Toy through its full sequence
    /// of required repair steps. Pure C# - no MonoBehaviour, no UI - so it
    /// never manipulates views directly; it only reports state through
    /// events. Delegates state validity to RepairStateMachine, step order
    /// to RepairStepQueue, and "what does progress on this step actually
    /// mean" to whichever IRepairBehaviour the registry returns for the
    /// step's RepairType - this class itself never branches on RepairType.
    /// </summary>
    public sealed class RepairController
    {
        /// <summary>Raised when a repair session starts for a toy.</summary>
        public event Action<Toy> RepairStarted;

        /// <summary>Raised when a new step becomes current and is awaiting tool selection.</summary>
        public event Action<RepairStep> RepairStepStarted;

        /// <summary>Raised with the current step's progress (0-1) whenever it changes.</summary>
        public event Action<float> RepairProgressChanged;

        /// <summary>Raised when the current step reaches 100% and is marked complete.</summary>
        public event Action<RepairStep> RepairStepCompleted;

        /// <summary>Raised when the wrong tool is selected for the current step.</summary>
        public event Action<ToolType> IncorrectToolUsed;

        /// <summary>Raised when an in-progress repair attempt is abandoned (e.g. switching tools mid-repair).</summary>
        public event Action RepairCancelled;

        /// <summary>Raised when every required step for the toy is complete.</summary>
        public event Action<Toy> ToyCompleted;

        public RepairStateMachine StateMachine { get; } = new RepairStateMachine();
        public Toy CurrentToy { get; private set; }
        public RepairStep CurrentStep => _queue.Current;
        public RepairStep NextStep => _queue.Next;
        public int RemainingStepCount => _queue.RemainingCount;
        public bool IsRepairing => StateMachine.Current == RepairState.Repairing;

        private readonly IRepairBehaviourRegistry _behaviourRegistry;
        private readonly RepairStepQueue _queue = new RepairStepQueue();
        private IRepairBehaviour _activeBehaviour;

        public RepairController(IRepairBehaviourRegistry behaviourRegistry)
        {
            _behaviourRegistry = behaviourRegistry;
        }

        /// <summary>Begins a repair session for the given toy, building the step queue from its data.</summary>
        public void StartRepair(Toy toy)
        {
            if (toy == null)
            {
                return;
            }

            if (StateMachine.Current == RepairState.ToyCompleted)
            {
                StateMachine.TransitionTo(RepairState.Idle);
            }

            CurrentToy = toy;
            _queue.Load(toy.Data.RequiredRepairSteps);

            toy.SetState(ToyState.InProgress);
            RepairStarted?.Invoke(toy);

            if (_queue.IsEmpty)
            {
                FinishToy();
                return;
            }

            StateMachine.TransitionTo(RepairState.ToolSelection);
            RepairStepStarted?.Invoke(_queue.Current);
        }

        /// <summary>
        /// Attempts to select a tool for the current step. If a repair is
        /// already in progress with a different tool, that attempt is
        /// cancelled first. Returns false (and raises IncorrectToolUsed)
        /// if the tool doesn't match what the current step requires.
        /// </summary>
        public bool TrySelectTool(ToolType tool)
        {
            if (_queue.Current == null)
            {
                return false;
            }

            if (StateMachine.Current == RepairState.Repairing)
            {
                CancelRepair();
            }

            if (StateMachine.Current != RepairState.ToolSelection)
            {
                return false;
            }

            if (_queue.Current.Data.ToolRequired != tool)
            {
                IncorrectToolUsed?.Invoke(tool);
                return false;
            }

            _activeBehaviour = _behaviourRegistry.GetBehaviour(_queue.Current.Data.RepairType);
            _activeBehaviour.ProgressChanged += HandleBehaviourProgress;
            _activeBehaviour.Completed += HandleBehaviourCompleted;
            _activeBehaviour.Begin(_queue.Current);

            StateMachine.TransitionTo(RepairState.Repairing);
            return true;
        }

        /// <summary>Feeds interaction input (e.g. drag distance) into the active behaviour. No-op unless currently Repairing.</summary>
        public void AddProgress(float delta)
        {
            if (!IsRepairing || _activeBehaviour == null)
            {
                return;
            }

            _activeBehaviour.UpdateProgress(delta);
        }

        /// <summary>Abandons the current repair attempt, resetting its progress and returning to tool selection.</summary>
        public void CancelRepair()
        {
            if (!IsRepairing || _activeBehaviour == null)
            {
                return;
            }

            _activeBehaviour.Cancel();
            DetachActiveBehaviour();

            _queue.Current.SetProgress(0f);
            RepairProgressChanged?.Invoke(0f);

            StateMachine.TransitionTo(RepairState.ToolSelection);
            RepairCancelled?.Invoke();
        }

        private void HandleBehaviourProgress(float progress)
        {
            _queue.Current.SetProgress(progress);
            RepairProgressChanged?.Invoke(progress);
        }

        private void HandleBehaviourCompleted()
        {
            DetachActiveBehaviour();

            RepairStep completedStep = _queue.Current;
            completedStep.Complete();
            CurrentToy.AddCompletedStep(completedStep);

            StateMachine.TransitionTo(RepairState.StepCompleted);
            RepairStepCompleted?.Invoke(completedStep);

            if (_queue.Advance())
            {
                StateMachine.TransitionTo(RepairState.ToolSelection);
                RepairStepStarted?.Invoke(_queue.Current);
            }
            else
            {
                FinishToy();
            }
        }

        private void FinishToy()
        {
            CurrentToy.SetState(ToyState.Repaired);
            CurrentToy.SetProgress(1f);
            StateMachine.TransitionTo(RepairState.ToyCompleted);
            ToyCompleted?.Invoke(CurrentToy);
        }

        private void DetachActiveBehaviour()
        {
            if (_activeBehaviour == null)
            {
                return;
            }

            _activeBehaviour.ProgressChanged -= HandleBehaviourProgress;
            _activeBehaviour.Completed -= HandleBehaviourCompleted;
            _activeBehaviour = null;
        }

#if UNITY_EDITOR
        /// <summary>Editor-only: forces the current step to 100% instantly, selecting its tool if needed. For the Stage 5 debug panel.</summary>
        public void DebugForceCompleteCurrentStep()
        {
            if (_queue.Current == null)
            {
                return;
            }

            if (!IsRepairing)
            {
                TrySelectTool(_queue.Current.Data.ToolRequired);
            }

            _activeBehaviour?.UpdateProgress(1f);
        }

        /// <summary>Editor-only: force-completes every remaining step for the current toy. For the Stage 5 debug panel.</summary>
        public void DebugForceCompleteToy()
        {
            while (_queue.Current != null)
            {
                DebugForceCompleteCurrentStep();
            }
        }
#endif
    }
}

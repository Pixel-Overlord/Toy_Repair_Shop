using System;
using System.Collections.Generic;
using ToyRepairShop.Data.Enums;
using ToyRepairShop.Gameplay.Models;
using UnityEngine;

namespace ToyRepairShop.Gameplay.Controllers
{
    /// <summary>
    /// Drives the repair of a single Toy through its required repair
    /// steps. Pure C# - no MonoBehaviour, no UI - so it never manipulates
    /// views directly; it only reports state through events.
    /// </summary>
    public sealed class RepairController
    {
        /// <summary>Raised when a repair session starts for a toy.</summary>
        public event Action<Toy> RepairStarted;

        /// <summary>Raised with the current step's progress (0-1) whenever it changes.</summary>
        public event Action<float> RepairProgressChanged;

        /// <summary>Raised when the current step reaches 100% and is marked complete.</summary>
        public event Action<RepairStep> RepairStepCompleted;

        /// <summary>Raised when every required step for the toy is complete.</summary>
        public event Action<Toy> ToyFinished;

        public Toy CurrentToy { get; private set; }
        public RepairStep CurrentStep { get; private set; }

        private readonly List<RepairStep> _steps = new List<RepairStep>();
        private int _currentStepIndex;

        /// <summary>Begins a repair session for the given toy, building runtime steps from its data.</summary>
        public void StartRepair(Toy toy)
        {
            if (toy == null)
            {
                return;
            }

            CurrentToy = toy;
            _steps.Clear();

            foreach (var stepData in toy.Data.RequiredRepairSteps)
            {
                _steps.Add(new RepairStep(stepData));
            }

            _currentStepIndex = 0;
            CurrentStep = _steps.Count > 0 ? _steps[0] : null;

            toy.SetState(ToyState.InProgress);
            RepairStarted?.Invoke(toy);
        }

        /// <summary>
        /// Attempts to add progress to the current step using the given
        /// tool. Returns false (and makes no change) if there is no active
        /// step, it is already complete, or the tool does not match what
        /// the step requires - this is the single validation point for
        /// "is this repair action allowed right now".
        /// </summary>
        public bool TryAddProgress(ToolType usedTool, float delta)
        {
            if (CurrentStep == null || CurrentStep.IsCompleted)
            {
                return false;
            }

            if (CurrentStep.Data.ToolRequired != usedTool)
            {
                return false;
            }

            float newProgress = Mathf.Clamp01(CurrentStep.Progress + delta);
            CurrentStep.SetProgress(newProgress);
            RepairProgressChanged?.Invoke(newProgress);

            if (newProgress >= 1f)
            {
                CompleteCurrentStep();
            }

            return true;
        }

        private void CompleteCurrentStep()
        {
            RepairStep completedStep = CurrentStep;
            completedStep.Complete();
            CurrentToy.AddCompletedStep(completedStep);
            RepairStepCompleted?.Invoke(completedStep);

            _currentStepIndex++;

            if (_currentStepIndex >= _steps.Count)
            {
                CurrentToy.SetState(ToyState.Repaired);
                CurrentToy.SetProgress(1f);
                CurrentStep = null;
                ToyFinished?.Invoke(CurrentToy);
            }
            else
            {
                CurrentStep = _steps[_currentStepIndex];
            }
        }
    }
}

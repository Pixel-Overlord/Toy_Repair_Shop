using ToyRepairShop.Data.Enums;
using ToyRepairShop.Gameplay.Behaviours;
using ToyRepairShop.Gameplay.Controllers;
using ToyRepairShop.Gameplay.Interaction;
using ToyRepairShop.Gameplay.Models;
using ToyRepairShop.Gameplay.Spawning;
using ToyRepairShop.UI;
using UnityEngine;

namespace ToyRepairShop.Managers
{
    /// <summary>
    /// Composition root for the Workshop scene. Constructs the pure C#
    /// RepairController (with its behaviour registry) and ToolManager,
    /// wires the spawner, interaction, and HUD/popup views to them via
    /// events, and drives the "spawn next toy" loop. No repair rules live
    /// here - only wiring, mirroring how Bootstrapper only wires managers
    /// rather than implementing behaviour itself.
    /// </summary>
    public sealed class WorkshopController : MonoBehaviour
    {
        [Header("Spawning")]
        [SerializeField] private ToySpawner _toySpawner;
        [SerializeField] private ToyView _toyView;

        [Header("Interaction")]
        [SerializeField] private RepairDragInteraction _repairDragInteraction;
        [SerializeField] private ToolbarView _toolbarView;

        [Header("HUD")]
        [SerializeField] private RepairHUDView _repairHUD;
        [SerializeField] private RewardPopupView _rewardPopupView;
        [SerializeField] private CoinsDisplayView _coinsDisplayView;

        [Header("Audio (optional placeholders)")]
        [SerializeField] private AudioClip _repairCompleteSfx;
        [SerializeField] private AudioClip _incorrectToolSfx;

        private RepairController _repairController;
        private ToolManager _toolManager;

        public RepairController RepairController => _repairController;

        private void Start()
        {
            _repairController = new RepairController(new RepairBehaviourRegistry());
            _toolManager = new ToolManager();

            _toolbarView.Initialize(_toolManager);
            _toolbarView.ToolTapped += HandleToolTapped;
            _repairDragInteraction.Initialize(_repairController);

            _toySpawner.ToySpawned += HandleToySpawned;
            _repairController.RepairStarted += HandleRepairStarted;
            _repairController.RepairStepStarted += HandleRepairStepStarted;
            _repairController.RepairProgressChanged += HandleRepairProgress;
            _repairController.RepairStepCompleted += HandleRepairStepCompleted;
            _repairController.IncorrectToolUsed += HandleIncorrectToolUsed;
            _repairController.ToyCompleted += HandleToyCompleted;
            _toolManager.ToolSelected += HandleToolSelected;

            _repairHUD.SetProgressVisible(false);

            SpawnAndStartNextToy();
        }

        private void OnDestroy()
        {
            if (_toySpawner != null)
            {
                _toySpawner.ToySpawned -= HandleToySpawned;
            }

            if (_toolbarView != null)
            {
                _toolbarView.ToolTapped -= HandleToolTapped;
            }

            if (_toolManager != null)
            {
                _toolManager.ToolSelected -= HandleToolSelected;
            }

            if (_repairController != null)
            {
                _repairController.RepairStarted -= HandleRepairStarted;
                _repairController.RepairStepStarted -= HandleRepairStepStarted;
                _repairController.RepairProgressChanged -= HandleRepairProgress;
                _repairController.RepairStepCompleted -= HandleRepairStepCompleted;
                _repairController.IncorrectToolUsed -= HandleIncorrectToolUsed;
                _repairController.ToyCompleted -= HandleToyCompleted;
            }
        }

        private void SpawnAndStartNextToy()
        {
            Toy toy = _toySpawner.SpawnRandom();
            if (toy == null)
            {
                return;
            }

            _repairHUD.SetToyName(toy.Data.ToyName);
            _repairController.StartRepair(toy);
        }

        private void HandleToySpawned(Toy toy)
        {
            _toyView.Bind(toy);
        }

        private void HandleToolTapped(ToolType tool)
        {
            bool wasSelected = _repairController.TrySelectTool(tool);
            if (wasSelected)
            {
                _toolbarView.PlaySelectedFeedback(tool);
                _repairHUD.SetInstruction("Drag on the toy to repair it!");
            }
        }

        private void HandleToolSelected(ToolType? tool)
        {
            _repairHUD.SetCurrentTool(tool?.ToString());
        }

        private void HandleIncorrectToolUsed(ToolType tool)
        {
            _toolbarView.PlayIncorrectFeedback(tool);
            AudioManager.Instance?.PlaySFX(_incorrectToolSfx);
            _repairHUD.SetInstruction("Not quite - try a different tool!");
        }

        private void HandleRepairStarted(Toy toy)
        {
            _repairHUD.SetProgressVisible(true);
            _repairHUD.SetProgress(0f);
        }

        private void HandleRepairStepStarted(Gameplay.Models.RepairStep step)
        {
            _repairHUD.SetCurrentStep(step.Data.StepName);
            _repairHUD.SetNextStep(_repairController.NextStep?.Data.StepName);
            _repairHUD.SetRemainingSteps(_repairController.RemainingStepCount);
            _repairHUD.SetCurrentTool(null);
            _repairHUD.SetProgress(0f);
            _repairHUD.SetInstruction("Choose the right tool below!");
        }

        private void HandleRepairProgress(float progress)
        {
            _repairHUD.SetProgress(progress);
        }

        private void HandleRepairStepCompleted(Gameplay.Models.RepairStep step)
        {
            _repairHUD.PlayStepSuccessPulse();

            if (_repairController.RemainingStepCount > 0)
            {
                // Not the final step - HandleToyCompleted handles the
                // fully-repaired look instead when this was the last one.
                _toyView.AdvanceToState(_repairController.CurrentToy.CompletedSteps.Count - 1);
            }
        }

        private void HandleToyCompleted(Toy toy)
        {
            _repairHUD.SetProgressVisible(false);
            _repairHUD.SetInstruction(string.Empty);
            _toyView.PlayRepairedTransition();
            _coinsDisplayView.AddCoins(toy.Data.RewardCoins);
            AudioManager.Instance?.PlaySFX(_repairCompleteSfx);
            _rewardPopupView.Show(toy.Data.ToyName, toy.Data.RewardCoins, SpawnAndStartNextToy);
        }

#if UNITY_EDITOR
        /// <summary>Editor-only: spawns a specific toy by ID and starts its repair. For the Stage 5 debug panel.</summary>
        public void DebugSpawnToy(string toyId)
        {
            Toy toy = _toySpawner.Spawn(toyId);
            if (toy == null)
            {
                return;
            }

            _repairHUD.SetToyName(toy.Data.ToyName);
            _repairController.StartRepair(toy);
        }

        /// <summary>Editor-only: force-completes the current repair step. For the Stage 5 debug panel.</summary>
        public void DebugSkipCurrentStep()
        {
            _repairController?.DebugForceCompleteCurrentStep();
        }

        /// <summary>Editor-only: force-completes every remaining step for the current toy. For the Stage 5 debug panel.</summary>
        public void DebugCompleteToy()
        {
            _repairController?.DebugForceCompleteToy();
        }

        /// <summary>Editor-only: spawns a fresh random toy, discarding any in-progress repair. For the Stage 5 debug panel.</summary>
        public void DebugResetWorkshop()
        {
            SpawnAndStartNextToy();
        }
#endif
    }
}

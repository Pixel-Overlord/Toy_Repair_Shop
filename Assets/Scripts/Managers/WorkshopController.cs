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
    /// RepairController/ToolSelectionController, wires the spawner,
    /// interaction, and view components to them via events, and plays the
    /// placeholder repair-complete SFX. No repair rules live here - only
    /// wiring, mirroring how Bootstrapper only wires managers rather than
    /// implementing behaviour itself.
    /// </summary>
    public sealed class WorkshopController : MonoBehaviour
    {
        [Header("Spawning")]
        [SerializeField] private ToySpawner _toySpawner;
        [SerializeField] private ToyView _toyView;

        [Header("Interaction")]
        [SerializeField] private WashInteraction _washInteraction;
        [SerializeField] private ToolbarView _toolbarView;

        [Header("HUD")]
        [SerializeField] private ProgressBarView _progressBarView;
        [SerializeField] private RewardPopupView _rewardPopupView;
        [SerializeField] private CoinsDisplayView _coinsDisplayView;

        [Header("Audio (optional placeholder)")]
        [SerializeField] private AudioClip _repairCompleteSfx;

        private RepairController _repairController;
        private ToolSelectionController _toolSelection;

        private void Start()
        {
            _repairController = new RepairController();
            _toolSelection = new ToolSelectionController();

            _toolbarView.Initialize(_toolSelection);
            _washInteraction.Initialize(_repairController, _toolSelection);

            _toySpawner.ToySpawned += HandleToySpawned;
            _repairController.RepairStarted += HandleRepairStarted;
            _repairController.RepairProgressChanged += HandleRepairProgress;
            _repairController.ToyFinished += HandleToyFinished;

            _progressBarView.SetVisible(false);

            Toy toy = _toySpawner.Spawn();
            if (toy != null)
            {
                _repairController.StartRepair(toy);
            }
        }

        private void OnDestroy()
        {
            if (_toySpawner != null)
            {
                _toySpawner.ToySpawned -= HandleToySpawned;
            }

            if (_repairController != null)
            {
                _repairController.RepairStarted -= HandleRepairStarted;
                _repairController.RepairProgressChanged -= HandleRepairProgress;
                _repairController.ToyFinished -= HandleToyFinished;
            }
        }

        private void HandleToySpawned(Toy toy)
        {
            _toyView.Bind(toy);
        }

        private void HandleRepairStarted(Toy toy)
        {
            _progressBarView.SetVisible(true);
            _progressBarView.SetProgress(0f);
        }

        private void HandleRepairProgress(float progress)
        {
            _progressBarView.SetProgress(progress);
        }

        private void HandleToyFinished(Toy toy)
        {
            _progressBarView.SetVisible(false);
            _toyView.PlayRepairedTransition();
            _coinsDisplayView.AddCoins(toy.Data.RewardCoins);
            _rewardPopupView.Show(toy.Data.RewardCoins);
            AudioManager.Instance?.PlaySFX(_repairCompleteSfx);
        }
    }
}

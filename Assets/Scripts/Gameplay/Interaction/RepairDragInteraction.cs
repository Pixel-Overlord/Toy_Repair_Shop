using ToyRepairShop.Gameplay.Controllers;
using ToyRepairShop.Managers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ToyRepairShop.Gameplay.Interaction
{
    /// <summary>
    /// Generic drag-to-repair interaction: dragging over the toy while
    /// RepairController is in the Repairing state feeds drag distance
    /// into whichever IRepairBehaviour is currently active. Renamed from
    /// Stage 4's WashInteraction now that tool validity is checked once
    /// at tool-selection time (RepairController.TrySelectTool) rather
    /// than per drag event, so this component no longer needs to know
    /// about tools or Wash specifically - the same drag gesture now
    /// drives every repair type's placeholder mechanic. Driven entirely
    /// by uGUI's drag callbacks (event-driven, no Update() polling), and
    /// ignores any pointer beyond the first one already being tracked.
    /// </summary>
    public sealed class RepairDragInteraction : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField, Tooltip("Progress added per pixel of drag distance.")]
        private float _progressPerPixel = 0.004f;

        [SerializeField, Tooltip("Optional placeholder SFX played once when a repair drag begins.")]
        private AudioClip _repairingSfx;

        private RepairController _repairController;
        private int _activePointerId = -1;

        /// <summary>Injects the pure C# controller this interaction drives. Called by the scene's composition root.</summary>
        public void Initialize(RepairController repairController)
        {
            _repairController = repairController;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_activePointerId != -1)
            {
                return;
            }

            if (_repairController == null || !_repairController.IsRepairing)
            {
                return;
            }

            _activePointerId = eventData.pointerId;
            AudioManager.Instance?.PlaySFX(_repairingSfx);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != _activePointerId)
            {
                return;
            }

            float delta = eventData.delta.magnitude * _progressPerPixel;
            _repairController?.AddProgress(delta);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.pointerId == _activePointerId)
            {
                _activePointerId = -1;
            }
        }
    }
}

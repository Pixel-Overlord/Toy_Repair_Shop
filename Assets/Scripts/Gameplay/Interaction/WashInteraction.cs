using ToyRepairShop.Data.Enums;
using ToyRepairShop.Gameplay.Controllers;
using ToyRepairShop.Managers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ToyRepairShop.Gameplay.Interaction
{
    /// <summary>
    /// Stage 4's single interaction: dragging over the toy while the
    /// Sponge is selected increases wash progress. Driven entirely by
    /// uGUI's drag callbacks (event-driven, no Update() polling), and
    /// ignores any pointer beyond the first one already being tracked.
    /// </summary>
    public sealed class WashInteraction : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField, Tooltip("Progress added per pixel of drag distance.")]
        private float _progressPerPixel = 0.004f;

        [SerializeField, Tooltip("Optional placeholder SFX played once when a wash drag begins.")]
        private AudioClip _cleaningSfx;

        private RepairController _repairController;
        private ToolSelectionController _toolSelection;
        private int _activePointerId = -1;

        /// <summary>Injects the pure C# controllers this interaction drives. Called by the scene's composition root.</summary>
        public void Initialize(RepairController repairController, ToolSelectionController toolSelection)
        {
            _repairController = repairController;
            _toolSelection = toolSelection;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_activePointerId != -1)
            {
                return;
            }

            if (_toolSelection == null || _toolSelection.SelectedTool != ToolType.Sponge)
            {
                return;
            }

            _activePointerId = eventData.pointerId;
            AudioManager.Instance?.PlaySFX(_cleaningSfx);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != _activePointerId)
            {
                return;
            }

            float delta = eventData.delta.magnitude * _progressPerPixel;
            _repairController?.TryAddProgress(ToolType.Sponge, delta);
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

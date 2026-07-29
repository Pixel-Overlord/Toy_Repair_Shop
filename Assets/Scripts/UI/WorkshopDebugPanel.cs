#if UNITY_EDITOR
using ToyRepairShop.Managers;
using UnityEngine;

namespace ToyRepairShop.UI
{
    /// <summary>
    /// Editor-only debug overlay for exercising the repair loop without
    /// playing through it normally: spawn any toy, skip the current step,
    /// force-complete the toy, or reset the workshop. Wrapped entirely in
    /// #if UNITY_EDITOR, so it is compiled out of player builds - not
    /// just hidden at runtime.
    /// </summary>
    public sealed class WorkshopDebugPanel : MonoBehaviour
    {
        [SerializeField] private WorkshopController _workshopController;

        [SerializeField]
        private string[] _debugToyIds =
        {
            "toy_broken_teddy",
            "toy_broken_robot",
            "toy_broken_car",
        };

        private bool _isOpen;
        private Rect _windowRect = new Rect(10f, 50f, 260f, 260f);

        private void OnGUI()
        {
            _isOpen = GUI.Toggle(new Rect(10f, 10f, 150f, 30f), _isOpen, "Debug Panel");

            if (!_isOpen)
            {
                return;
            }

            _windowRect = GUI.Window(GetInstanceID(), _windowRect, DrawWindow, "Workshop Debug");
        }

        private void DrawWindow(int id)
        {
            GUILayout.BeginVertical();

            GUILayout.Label("Spawn Toy:");
            foreach (string toyId in _debugToyIds)
            {
                if (GUILayout.Button(toyId))
                {
                    _workshopController.DebugSpawnToy(toyId);
                }
            }

            GUILayout.Space(8f);

            if (GUILayout.Button("Skip Current Step"))
            {
                _workshopController.DebugSkipCurrentStep();
            }

            if (GUILayout.Button("Complete Toy"))
            {
                _workshopController.DebugCompleteToy();
            }

            if (GUILayout.Button("Reset Workshop"))
            {
                _workshopController.DebugResetWorkshop();
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
}
#endif

using UnityEngine;
using UnityEngine.UI;

namespace ToyRepairShop.UI
{
    /// <summary>
    /// Displays repair progress as a filled bar. Purely visual - it has no
    /// idea what a "repair step" is, it just draws a 0-1 value.
    /// </summary>
    public sealed class ProgressBarView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Image _fillImage;

        /// <summary>Sets the fill amount, clamped to 0-1.</summary>
        public void SetProgress(float normalizedProgress)
        {
            if (_fillImage != null)
            {
                _fillImage.fillAmount = Mathf.Clamp01(normalizedProgress);
            }
        }

        /// <summary>Shows or hides the whole progress bar.</summary>
        public void SetVisible(bool visible)
        {
            if (_root != null)
            {
                _root.SetActive(visible);
            }
        }
    }
}

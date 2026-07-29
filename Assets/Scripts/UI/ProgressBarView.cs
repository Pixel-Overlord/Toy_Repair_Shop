using System.Collections;
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

        [Header("Success Pulse")]
        [SerializeField] private float _pulseDuration = 0.15f;

        private Coroutine _pulseRoutine;

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

        /// <summary>Plays a brief white flash to celebrate a completed step.</summary>
        public void PlaySuccessPulse()
        {
            if (_fillImage == null)
            {
                return;
            }

            if (_pulseRoutine != null)
            {
                StopCoroutine(_pulseRoutine);
            }

            _pulseRoutine = StartCoroutine(PulseRoutine());
        }

        private IEnumerator PulseRoutine()
        {
            Color originalColor = _fillImage.color;
            float elapsed = 0f;

            while (elapsed < _pulseDuration)
            {
                elapsed += Time.deltaTime;
                _fillImage.color = Color.Lerp(Color.white, originalColor, elapsed / _pulseDuration);
                yield return null;
            }

            _fillImage.color = originalColor;
            _pulseRoutine = null;
        }
    }
}

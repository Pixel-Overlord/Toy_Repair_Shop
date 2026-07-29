using System.Collections;
using ToyRepairShop.Gameplay.Models;
using UnityEngine;
using UnityEngine.UI;

namespace ToyRepairShop.UI
{
    /// <summary>
    /// Displays the sprite for a runtime Toy. Purely visual - it reacts to
    /// the model it's bound to and plays simple built-in transitions
    /// (fade on sprite swap, a small bounce on repair) with no gameplay
    /// logic and no third-party tweening.
    /// </summary>
    public sealed class ToyView : MonoBehaviour
    {
        [SerializeField] private Image _toyImage;

        [Header("Repaired Transition")]
        [SerializeField, Tooltip("Seconds to fade out/in when swapping to the repaired sprite.")]
        private float _fadeDuration = 0.2f;

        [SerializeField, Tooltip("Peak scale of the bounce played after the repaired sprite appears.")]
        private float _bounceScale = 1.15f;

        [SerializeField, Tooltip("Seconds the bounce takes, up and back down.")]
        private float _bounceDuration = 0.25f;

        private Toy _toy;
        private Coroutine _transitionRoutine;

        public Toy BoundToy => _toy;

        /// <summary>Binds a freshly spawned toy and shows its broken sprite.</summary>
        public void Bind(Toy toy)
        {
            _toy = toy;

            if (_transitionRoutine != null)
            {
                StopCoroutine(_transitionRoutine);
                _transitionRoutine = null;
            }

            transform.localScale = Vector3.one;
            SetSprite(toy != null ? toy.Data.BrokenSprite : null);
            SetAlpha(1f);
        }

        /// <summary>Plays the broken-to-repaired sprite transition plus a small bounce.</summary>
        public void PlayRepairedTransition()
        {
            if (_toy == null)
            {
                return;
            }

            if (_transitionRoutine != null)
            {
                StopCoroutine(_transitionRoutine);
            }

            _transitionRoutine = StartCoroutine(RepairedTransitionRoutine());
        }

        private void SetSprite(Sprite sprite)
        {
            if (_toyImage != null)
            {
                _toyImage.sprite = sprite;
            }
        }

        private void SetAlpha(float alpha)
        {
            if (_toyImage == null)
            {
                return;
            }

            Color color = _toyImage.color;
            color.a = alpha;
            _toyImage.color = color;
        }

        private IEnumerator RepairedTransitionRoutine()
        {
            yield return FadeTo(0f, _fadeDuration);
            SetSprite(_toy.Data.RepairedSprite);
            yield return FadeTo(1f, _fadeDuration);
            yield return Bounce();
            _transitionRoutine = null;
        }

        private IEnumerator FadeTo(float targetAlpha, float duration)
        {
            if (_toyImage == null || duration <= 0f)
            {
                SetAlpha(targetAlpha);
                yield break;
            }

            float startAlpha = _toyImage.color.a;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration));
                yield return null;
            }

            SetAlpha(targetAlpha);
        }

        private IEnumerator Bounce()
        {
            if (_bounceDuration <= 0f)
            {
                yield break;
            }

            float halfDuration = _bounceDuration * 0.5f;
            float elapsed = 0f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float scale = Mathf.Lerp(1f, _bounceScale, elapsed / halfDuration);
                transform.localScale = Vector3.one * scale;
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float scale = Mathf.Lerp(_bounceScale, 1f, elapsed / halfDuration);
                transform.localScale = Vector3.one * scale;
                yield return null;
            }

            transform.localScale = Vector3.one;
        }
    }
}

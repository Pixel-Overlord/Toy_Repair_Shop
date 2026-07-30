using System.Collections;
using System.Collections.Generic;
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
        private static readonly int AdvanceTrigger = Animator.StringToHash("Advance");
        private static readonly int CompleteTrigger = Animator.StringToHash("Complete");

        [SerializeField] private Image _toyImage;

        [Header("Animator (optional - assign your own AnimatorController)")]
        [SerializeField, Tooltip("Fires 'Advance' when a non-final step completes and 'Complete' when the toy is fully repaired. Leave unassigned until you have clips to play.")]
        private Animator _animator;

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

        /// <summary>
        /// Advances the toy's look after a non-final repair step completes.
        /// completedStepIndex is zero-based (0 for the first step, 1 for
        /// the second, ...). Swaps in the matching RepairStateSprites
        /// entry if one is assigned; otherwise the sprite is left as-is,
        /// but the Advance trigger still fires so an assigned Animator
        /// can react (e.g. a shake/sparkle) even before per-step art exists.
        /// </summary>
        public void AdvanceToState(int completedStepIndex)
        {
            if (_toy == null)
            {
                return;
            }

            IReadOnlyList<Sprite> states = _toy.Data.RepairStateSprites;
            if (completedStepIndex >= 0 && completedStepIndex < states.Count && states[completedStepIndex] != null)
            {
                if (_transitionRoutine != null)
                {
                    StopCoroutine(_transitionRoutine);
                }

                _transitionRoutine = StartCoroutine(SwapSpriteWithFade(states[completedStepIndex]));
            }

            _animator?.SetTrigger(AdvanceTrigger);
        }

        /// <summary>Plays the broken-to-repaired sprite transition plus a small bounce.</summary>
        public void PlayRepairedTransition()
        {
            if (_toy == null)
            {
                return;
            }

            _animator?.SetTrigger(CompleteTrigger);

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

        private IEnumerator SwapSpriteWithFade(Sprite sprite)
        {
            yield return FadeTo(0f, _fadeDuration);
            SetSprite(sprite);
            yield return FadeTo(1f, _fadeDuration);
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

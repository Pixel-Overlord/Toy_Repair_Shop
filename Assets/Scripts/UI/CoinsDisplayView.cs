using TMPro;
using UnityEngine;

namespace ToyRepairShop.UI
{
    /// <summary>
    /// Placeholder session coin counter shown in the Workshop's top bar.
    /// Intentionally not persisted - Stage 4 explicitly excludes an
    /// economy/save system for coins; this resets every session.
    /// </summary>
    public sealed class CoinsDisplayView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _coinsText;

        private int _coins;

        private void Awake()
        {
            Refresh();
        }

        /// <summary>Adds to the session coin total and refreshes the display.</summary>
        public void AddCoins(int amount)
        {
            _coins += amount;
            Refresh();
        }

        private void Refresh()
        {
            if (_coinsText != null)
            {
                _coinsText.text = _coins.ToString();
            }
        }
    }
}

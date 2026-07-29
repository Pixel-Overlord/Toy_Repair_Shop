using System.Collections.Generic;
using UnityEngine;

namespace ToyRepairShop.Data.ScriptableObjects
{
    /// <summary>
    /// Static catalogue of every ToyData asset in the game. A new toy is
    /// added by creating a ToyData asset and dropping it into this list -
    /// no code changes required.
    /// </summary>
    [CreateAssetMenu(fileName = "ToyDatabase", menuName = "ToyRepairShop/Toy Database")]
    public sealed class ToyDatabase : ScriptableObject
    {
        [SerializeField, Tooltip("Every toy available in the game.")]
        private List<ToyData> _toys = new List<ToyData>();

        public IReadOnlyList<ToyData> Toys => _toys;

        /// <summary>Finds a toy by its Toy ID, or null if none matches.</summary>
        public ToyData GetToyById(string toyId)
        {
            for (int i = 0; i < _toys.Count; i++)
            {
                if (_toys[i] != null && _toys[i].ToyId == toyId)
                {
                    return _toys[i];
                }
            }

            return null;
        }

        /// <summary>Returns every toy whose unlock level is at or below playerLevel.</summary>
        public List<ToyData> GetAllUnlocked(int playerLevel)
        {
            var unlocked = new List<ToyData>();
            for (int i = 0; i < _toys.Count; i++)
            {
                if (_toys[i] != null && _toys[i].UnlockLevel <= playerLevel)
                {
                    unlocked.Add(_toys[i]);
                }
            }

            return unlocked;
        }

        /// <summary>Returns a uniformly random toy from the full catalogue, or null if empty.</summary>
        public ToyData GetRandomToy()
        {
            if (_toys.Count == 0)
            {
                return null;
            }

            return _toys[Random.Range(0, _toys.Count)];
        }

        private void OnValidate()
        {
            var seenIds = new HashSet<string>();
            foreach (ToyData toy in _toys)
            {
                if (toy == null)
                {
                    continue;
                }

                if (!seenIds.Add(toy.ToyId))
                {
                    Debug.LogWarning($"ToyDatabase '{name}' contains a duplicate Toy ID: '{toy.ToyId}'.", this);
                }
            }
        }
    }
}

using System;
using ToyRepairShop.Data.ScriptableObjects;
using ToyRepairShop.Gameplay.Factories;
using ToyRepairShop.Gameplay.Models;
using ToyRepairShop.Gameplay.Repositories;
using UnityEngine;

namespace ToyRepairShop.Gameplay.Spawning
{
    /// <summary>
    /// Spawns one runtime Toy at a time from data. Stage 4 only ever shows
    /// a single toy, but Spawn() is already the single seam a future
    /// object pool or multi-toy queue would sit behind - callers would not
    /// need to change.
    /// </summary>
    public sealed class ToySpawner : MonoBehaviour
    {
        [SerializeField, Tooltip("Catalogue of every toy in the game.")]
        private ToyDatabase _toyDatabase;

        [SerializeField, Tooltip("Toy ID to spawn. Stage 4 only supports one toy: the Broken Teddy.")]
        private string _toyIdToSpawn = "toy_broken_teddy";

        /// <summary>Raised after a toy is successfully spawned.</summary>
        public event Action<Toy> ToySpawned;

        public Toy CurrentToy { get; private set; }

        private IToyRepository _repository;
        private ToyFactory _factory;

        private void Awake()
        {
            _repository = new ToyRepository(_toyDatabase);
            _factory = new ToyFactory();
        }

        /// <summary>Spawns the configured toy and raises ToySpawned. Returns null if its data can't be found.</summary>
        public Toy Spawn()
        {
            ToyData data = _repository.GetById(_toyIdToSpawn);
            if (data == null)
            {
                Debug.LogWarning($"ToySpawner: no ToyData found for id '{_toyIdToSpawn}'.", this);
                return null;
            }

            CurrentToy = _factory.Create(data);
            ToySpawned?.Invoke(CurrentToy);
            return CurrentToy;
        }
    }
}

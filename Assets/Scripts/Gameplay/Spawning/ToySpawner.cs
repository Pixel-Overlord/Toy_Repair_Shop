using System;
using ToyRepairShop.Data.ScriptableObjects;
using ToyRepairShop.Gameplay.Factories;
using ToyRepairShop.Gameplay.Models;
using ToyRepairShop.Gameplay.Repositories;
using UnityEngine;

namespace ToyRepairShop.Gameplay.Spawning
{
    /// <summary>
    /// Spawns one runtime Toy at a time from data. Only ever holds a
    /// single active Toy; Spawn()/SpawnRandom() are the seam a future
    /// object pool or multi-toy queue would sit behind - callers would
    /// not need to change.
    /// </summary>
    public sealed class ToySpawner : MonoBehaviour
    {
        [SerializeField, Tooltip("Catalogue of every toy in the game.")]
        private ToyDatabase _toyDatabase;

        [SerializeField, Tooltip("Default toy ID used by Spawn() with no argument.")]
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

        /// <summary>Spawns the configured default toy. Returns null if its data can't be found.</summary>
        public Toy Spawn()
        {
            return Spawn(_toyIdToSpawn);
        }

        /// <summary>Spawns the toy with the given ID and raises ToySpawned. Returns null if its data can't be found.</summary>
        public Toy Spawn(string toyId)
        {
            ToyData data = _repository.GetById(toyId);
            if (data == null)
            {
                Debug.LogWarning($"ToySpawner: no ToyData found for id '{toyId}'.", this);
                return null;
            }

            return SpawnFromData(data);
        }

        /// <summary>Spawns a uniformly random toy from the full catalogue. Returns null if the catalogue is empty.</summary>
        public Toy SpawnRandom()
        {
            ToyData data = _repository.GetRandom();
            if (data == null)
            {
                Debug.LogWarning("ToySpawner: ToyDatabase is empty, nothing to spawn.", this);
                return null;
            }

            return SpawnFromData(data);
        }

        private Toy SpawnFromData(ToyData data)
        {
            CurrentToy = _factory.Create(data);
            ToySpawned?.Invoke(CurrentToy);
            return CurrentToy;
        }
    }
}

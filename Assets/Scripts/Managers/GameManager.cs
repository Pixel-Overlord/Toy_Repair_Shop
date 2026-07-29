using System;
using ToyRepairShop.Core;
using ToyRepairShop.Data;
using UnityEngine;

namespace ToyRepairShop.Managers
{
    /// <summary>
    /// Owns application-level state and the pause/resume lifecycle. Other
    /// systems should read state from here instead of tracking their own.
    /// </summary>
    public sealed class GameManager : MonoSingleton<GameManager>
    {
        /// <summary>Raised whenever the application state changes.</summary>
        public event Action<ApplicationState> StateChanged;

        /// <summary>Raised when the game is paused.</summary>
        public event Action Paused;

        /// <summary>Raised when the game is resumed.</summary>
        public event Action Resumed;

        public ApplicationState CurrentState { get; private set; } = ApplicationState.Initializing;
        public bool IsPaused { get; private set; }

        protected override void Initialize()
        {
            SetState(ApplicationState.Initializing);
        }

        /// <summary>Changes the current application state and notifies listeners.</summary>
        public void SetState(ApplicationState newState)
        {
            if (CurrentState == newState)
            {
                return;
            }

            CurrentState = newState;
            StateChanged?.Invoke(newState);
        }

        /// <summary>Pauses the game by freezing time. Safe to call when already paused.</summary>
        public void Pause()
        {
            if (IsPaused)
            {
                return;
            }

            IsPaused = true;
            Time.timeScale = 0f;
            Paused?.Invoke();
        }

        /// <summary>Resumes the game after a pause. Safe to call when not paused.</summary>
        public void Resume()
        {
            if (!IsPaused)
            {
                return;
            }

            IsPaused = false;
            Time.timeScale = 1f;
            Resumed?.Invoke();
        }
    }
}

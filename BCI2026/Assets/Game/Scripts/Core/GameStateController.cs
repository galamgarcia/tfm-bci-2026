/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;
using UnityEngine;

namespace Bit.Core
{
    /// <summary>Persists the current high-level application state across scenes.</summary>
    [DefaultExecutionOrder(-90)]
    public sealed class GameStateController : MonoBehaviour
    {
        public static GameStateController Instance { get; private set; }
        // Current high-level application state.
        private GameState _state = GameState.MainMenu;
        /// <summary>Trigger after the application state changes.</summary>
        public event Action<GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>Gets the current application state.</summary>
        /// <returns>The current application state.</returns>
        public GameState GetState()
        {
            return _state;
        }

        /// <summary>Changes the application state and notifies subscribers when it differs.</summary>
        /// <param name="state">New application state.</param>
        public void SetState(GameState state)
        {
            if (_state == state) { return; }
            _state = state;
            OnStateChanged?.Invoke(state);
        }
    }
}

/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;
using Bit.Core;
using Bit.Gameplay;
using Bit.Input;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Bit.EditorInput
{
    /// <summary>Manages Editor-only keyboard input.</summary>
    [InitializeOnLoad]
    public sealed class KeyboardInputManager : IHeadInputSource, IMentalInputSource, IBlinkInputSource
    {
        private static KeyboardInputManager _instance;
        private static readonly float[] MentalStateValues = { 0.1f, 0.9f };
        // Shared input actions used by the keyboard simulator.
        private InputActionAsset _actions;
        // Player movement action from the shared input asset.
        private InputAction _moveAction;
        // Keyboard action used to simulate a blink gesture.
        private InputAction _blinkAction;
        // Relaxation level actions from the shared input asset.
        private InputAction[] _relaxationActions;
        // Concentration level actions from the shared input asset.
        private InputAction[] _concentrationActions;
        // Current index in the simulated relaxation cycle.
        private int _relaxationIndex;
        // Current index in the simulated concentration cycle.
        private int _concentrationIndex;
        public bool HasFace => true;
        public float HorizontalInput => _moveAction == null ? 0f : _moveAction.ReadValue<Vector2>().x;
        public bool HasValidSignal => true;
        public float Relaxation => MentalStateValues[_relaxationIndex];
        public float Concentration => MentalStateValues[_concentrationIndex];
        /// <summary>Triggered when the keyboard source reports a nod gesture.</summary>
        public event Action NodDetected;
        /// <summary>Triggered when Space simulates a validated blink gesture.</summary>
        public event Action OnBlinkDetected;

        static KeyboardInputManager()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            InputSystem.onAfterUpdate += UpdateInstance;
        }

        /// <summary>Creates or disposes the Editor input instance as Play Mode changes.</summary>
        /// <param name="state">New Unity Editor Play Mode state.</param>
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.delayCall += CreateInstance;
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                DisposeInstance();
            }
        }

        /// <summary>Creates the singleton keyboard manager after the Play Mode scene loads.</summary>
        private static void CreateInstance()
        {
            if (!EditorApplication.isPlaying || _instance != null) { return; }
            _instance = new KeyboardInputManager();
            _instance.Initialize();
            Debug.Log("KeyboardInputManager: created for Editor Play Mode.");
        }

        /// <summary>Forwards Input System updates to the active keyboard manager.</summary>
        private static void UpdateInstance()
        {
            if (EditorApplication.isPlaying)
            {
                _instance?.Update();
            }
        }

        /// <summary>Disposes the keyboard manager and releases its input controllers.</summary>
        private static void DisposeInstance()
        {
            _instance?.Dispose();
            _instance = null;
        }

        /// <summary>Loads and enables the shared keyboard actions for Editor Play Mode.</summary>
        public void Initialize()
        {
            BciSettings settings = BciSettings.Instance;
            _actions = settings == null ? null : settings.InputActions;
            if (_actions == null)
            {
                Debug.LogError("KeyboardInputManager: no input actions asset is assigned in BciSettings.");
                return;
            }

            InputActionMap player = _actions.FindActionMap("Player");
            _moveAction = player?.FindAction("Move");
            _blinkAction = player?.FindAction("Blink");
            _relaxationActions = GetActions(player, "RelaxationLow", "RelaxationHigh");
            _concentrationActions = GetActions(player, "ConcentrationLow", "ConcentrationHigh");
            Debug.Log($"KeyboardInputManager: loaded Player actions (Move: {_moveAction != null}, Blink: {_blinkAction != null}).");
            _moveAction?.Enable();
            _blinkAction?.Enable();
            EnableActions(_relaxationActions);
            EnableActions(_concentrationActions);
        }

        /// <summary>Processes keyboard actions and updates all active input controllers.</summary>
        public void Update()
        {
            if (_actions == null) { return; }

            SetLevelWhenPressed(_relaxationActions, ref _relaxationIndex);
            SetLevelWhenPressed(_concentrationActions, ref _concentrationIndex);

            foreach (InputController input in UnityEngine.Object.FindObjectsByType<InputController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                input.ConfigureSources(this, this, this);
            }

            if (_blinkAction != null && _blinkAction.WasPressedThisFrame())
            {
                OnBlinkDetected?.Invoke();
            }
        }

        /// <summary>Disables the keyboard actions when Editor Play Mode ends.</summary>
        public void Dispose()
        {
            _moveAction?.Disable();
            _blinkAction?.Disable();
            DisableActions(_relaxationActions);
            DisableActions(_concentrationActions);
        }

        /// <summary>Gets the low and high actions from the Player action map.</summary>
        /// <param name="map">Player action map containing the level actions.</param>
        /// <param name="lowName">Name of the low-level action.</param>
        /// <param name="highName">Name of the high-level action.</param>
        /// <returns>The requested actions, or an empty array when the map is unavailable.</returns>
        private static InputAction[] GetActions(InputActionMap map, string lowName, string highName)
        {
            return map == null ? Array.Empty<InputAction>() : new[]
            {
                map.FindAction(lowName),
                map.FindAction(highName)
            };
        }

        /// <summary>Enables each action in an action collection.</summary>
        /// <param name="actions">Actions to enable.</param>
        private static void EnableActions(InputAction[] actions)
        {
            foreach (InputAction action in actions)
            {
                action?.Enable();
            }
        }

        /// <summary>Disables each action in an action collection.</summary>
        /// <param name="actions">Actions to disable.</param>
        private static void DisableActions(InputAction[] actions)
        {
            foreach (InputAction action in actions ?? Array.Empty<InputAction>())
            {
                action?.Disable();
            }
        }

        /// <summary>Sets a mental-state index when one of its actions is pressed.</summary>
        /// <param name="actions">Actions representing the available levels.</param>
        /// <param name="index">Index to update when an action is pressed.</param>
        private static void SetLevelWhenPressed(InputAction[] actions, ref int index)
        {
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] != null && actions[i].WasPressedThisFrame())
                {
                    index = i;
                    return;
                }
            }
        }
    }
}

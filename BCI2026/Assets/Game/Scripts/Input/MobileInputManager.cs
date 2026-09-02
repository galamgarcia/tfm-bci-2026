/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.Gameplay;
using UnityEngine;

namespace Bit.Input
{
    /// <summary>Connects mobile gameplay input controllers to head tracking and BrainLink input sources.</summary>
    public sealed class MobileInputManager : MonoBehaviour
    {
        [Header("Gameplay Input")]
        [Tooltip("Gameplay input controllers that receive mobile input sources.")]
        [SerializeField] private InputController[] inputControllers;

        [Header("Head Tracking")]
        [Tooltip("Head tracker used to provide horizontal movement and nod gestures.")]
        [SerializeField] private HeadPoseTracker headPoseTracker;

        private void Start()
        {
            BrainLinkBlinkInputSource blinkSource = BrainLinkBlinkInputSource.Instance;
            Debug.Log($"MobileInputManager: configuring {inputControllers.Length} input controller(s); head tracker assigned: {headPoseTracker != null}; blink source available: {blinkSource != null}.");
            foreach (InputController inputController in inputControllers)
            {
                inputController?.ConfigureSources(headPoseTracker, null, blinkSource);
            }
        }
    }
}

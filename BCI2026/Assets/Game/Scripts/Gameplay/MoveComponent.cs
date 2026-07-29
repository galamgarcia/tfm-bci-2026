/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using BciGame.Input;
using BciGame.Services;
using UnityEngine;

namespace BciGame.Gameplay
{
    /// <summary>
    /// Base component that exposes head-tracking and EEG input to movement implementations.
    /// </summary>
    public abstract class MoveComponent : MonoBehaviour
    {
        // Scene services that supply head pose and BrainLink EEG values.
        private BrainLinkConnection _brainLinkConnection;
        private HeadPoseTracker _headPoseTracker;

        protected virtual void Awake()
        {
            _brainLinkConnection = BrainLinkConnection.Instance;
            _headPoseTracker = FindFirstObjectByType<HeadPoseTracker>();
        }

        /// <summary>
        /// Reads the normalized horizontal head input when face tracking is available.
        /// </summary>
        /// <param name="input">Normalized yaw input, where negative is left and positive is right.</param>
        /// <returns>Whether a valid head input sample is available.</returns>
        protected bool TryGetHeadInput(out float input)
        {
            if (_headPoseTracker == null || !_headPoseTracker.HasFace)
            {
                input = 0f;
                return false;
            }

            input = _headPoseTracker.HorizontalInput;
            return true;
        }

        /// <summary>
        /// Determines wether the current BrainLink signal quality is suitable for an interaction.
        /// </summary>
        /// <returns>Wether a BrainLink signal quality is good.</returns>
        private bool HasDeviceGoodSignal()
        {
            return _brainLinkConnection != null && _brainLinkConnection.HasGoodSignal;
        }

        /// <summary>
        /// Reads a normalized relaxation value when BrainLink signal quality is sufficient.
        /// </summary>
        /// <param name="value">Normalized relaxation input.</param>
        /// <returns>Whether a valid EEG sample is available.</returns>
        protected bool TryGetRelaxationInput(out float value)
        {
            if (HasDeviceGoodSignal())
            {
                value = 0f;
                return false;
            }

            value = _brainLinkConnection.Relaxation;
            return true;
        }

        /// <summary>
        /// Reads a normalized concentration value when BrainLink signal quality is sufficient.
        /// </summary>
        /// <param name="value">Normalized concentration input.</param>
        /// <returns>Whether a valid EEG sample is available.</returns>
        protected bool TryGetConcentrationInput(out float value)
        {
            if (HasDeviceGoodSignal())
            {
                value = 0f;
                return false;
            }

            value = _brainLinkConnection.Concentration;
            return true;
        }
    }
}

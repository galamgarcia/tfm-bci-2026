/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using BciGame.Input;
using BciGame.Services;
using BciGame.Utilities;
using UnityEngine;

namespace BciGame.Gameplay
{
    /// <summary>
    /// Base component that exposes head-tracking and EEG input to movement implementations.
    /// </summary>
    public abstract class MoveComponent : MonoBehaviour
    {
        // Scene services that supply head pose and BrainLink EEG values.
        private HeadPoseTracker _headPoseTracker;

        protected virtual void Awake()
        {
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
        /// Reads a normalized relaxation value when BrainLink signal quality is sufficient.
        /// </summary>
        /// <param name="value">Normalized relaxation input.</param>
        /// <returns>Whether a valid EEG sample is available.</returns>
        protected bool TryGetRelaxationInput(out float value)
        {
            if (!Utils.IsBrainLinkConnectionGood())
            {
                value = 0f;
                return false;
            }

            value = BrainLinkConnection.Instance.Relaxation;
            return true;
        }

        /// <summary>
        /// Reads a normalized concentration value when BrainLink signal quality is sufficient.
        /// </summary>
        /// <param name="value">Normalized concentration input.</param>
        /// <returns>Whether a valid EEG sample is available.</returns>
        protected bool TryGetConcentrationInput(out float value)
        {
            if (!Utils.IsBrainLinkConnectionGood())
            {
                value = 0f;
                return false;
            }

            value = BrainLinkConnection.Instance.Concentration;
            return true;
        }

        /// <summary>Applies a movement delta using the concrete movement implementation.</summary>
        /// <param name="delta">Movement delta produced from the current input.</param>
        protected abstract void Move(Vector2 delta);
    }
}

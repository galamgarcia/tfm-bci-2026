/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;

namespace Bit.Input
{
    /// <summary>Detects discrete blinks from BrainLink intensity samples.</summary>
    public sealed class BlinkDetector
    {
        private readonly int _minIntensity;
        private readonly float _cooldown;
        // Requires a low sample before another high sample can trigger a blink.
        private bool _isReady = true;
        // Time at which the previous blink was accepted.
        private float _lastBlinkAt = float.NegativeInfinity;

        /// <summary>Creates a detector with an intensity threshold and refractory period.</summary>
        /// <param name="intensity">Minimum intensity required to detect a blink.</param>
        /// <param name="cooldown">Minimum time between accepted blinks.</param>
        public BlinkDetector(int intensity, float cooldown)
        {
            _minIntensity = Mathf.Max(1, intensity);
            _cooldown = Mathf.Max(0f, cooldown);
        }

        /// <summary>Processes one provider sample.</summary>
        /// <param name="intensity">Blink intensity reported by BrainLink.</param>
        /// <param name="hasValidSignal">Whether the EEG signal is currently valid.</param>
        /// <param name="timestamp">Unscaled time of the sample.</param>
        /// <returns>True when this sample completes a new blink detection.</returns>
        public bool Process(int intensity, bool hasValidSignal, float timestamp)
        {
            if (!hasValidSignal)
            {
                Reset();
                return false;
            }

            if (intensity < _minIntensity)
            {
                _isReady = true;
                return false;
            }

            if (!_isReady || (timestamp - _lastBlinkAt) < _cooldown) { return false; }

            _isReady = false;
            _lastBlinkAt = timestamp;
            return true;
        }

        /// <summary>Resets detection state after the signal becomes invalid.</summary>
        public void Reset()
        {
            _isReady = true;
            _lastBlinkAt = float.NegativeInfinity;
        }
    }
}

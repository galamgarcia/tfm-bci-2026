/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;
using UnityEngine;

namespace Bit.Input
{
    /// <summary>Extends the BrainLink SDK manager with application-specific EEG packet freshness tracking.</summary>
    public sealed class BrainLinkManager : ThinkGearManager
    {
        // Timestamp of the latest signal-quality packet.
        private float _lastPoorSignalAt = float.NegativeInfinity;
        // Timestamp of the latest attention packet.
        private float _lastAttentionAt = float.NegativeInfinity;
        // Timestamp of the latest meditation packet.
        private float _lastMeditationAt = float.NegativeInfinity;

        // Maximum age accepted for an EEG packet.
        private const float DataTimeoutSeconds = 5f;

        /// <summary>Updates the application packet state after the SDK processes a connection callback.</summary>
        /// <param name="data">Connection state sent by the BrainLink SDK.</param>
        protected override void ReceiveContentState(string data)
        {
            base.ReceiveContentState(data);
            if (!string.Equals(data, "yes", StringComparison.OrdinalIgnoreCase))
            {
                ResetEegTimestamps();
            }
        }

        /// <summary>Updates the signal-quality packet timestamp after the SDK processes its value.</summary>
        /// <param name="data">Signal-quality value sent by the BrainLink SDK.</param>
        protected override void ReceivePoorSignal(string data)
        {
            base.ReceivePoorSignal(data);
            _lastPoorSignalAt = Time.realtimeSinceStartup;
        }

        /// <summary>Updates the attention packet timestamp after the SDK processes its value.</summary>
        /// <param name="data">Attention value sent by the BrainLink SDK.</param>
        protected override void ReceiveAttention(string data)
        {
            base.ReceiveAttention(data);
            _lastAttentionAt = Time.realtimeSinceStartup;
        }

        /// <summary>Updates the meditation packet timestamp after the SDK processes its value.</summary>
        /// <param name="data">Meditation value sent by the BrainLink SDK.</param>
        protected override void ReceiveMeditation(string data)
        {
            base.ReceiveMeditation(data);
            _lastMeditationAt = Time.realtimeSinceStartup;
        }

        /// <summary>Returns whether any essential EEG packet was received recently.</summary>
        /// <returns>True when at least one essential EEG packet is current.</returns>
        public bool HasRecentEegData()
        {
            return IsRecent(_lastPoorSignalAt) || IsRecent(_lastAttentionAt) || IsRecent(_lastMeditationAt);
        }

        /// <summary>Returns whether all essential EEG packets were received recently.</summary>
        /// <returns>True when quality, attention and meditation packets are current.</returns>
        public bool HasRecentCompleteEegData()
        {
            return IsRecent(_lastPoorSignalAt) && IsRecent(_lastAttentionAt) && IsRecent(_lastMeditationAt);
        }

        /// <summary>Resets all EEG packet timestamps after a disconnection.</summary>
        private void ResetEegTimestamps()
        {
            _lastPoorSignalAt = float.NegativeInfinity;
            _lastAttentionAt = float.NegativeInfinity;
            _lastMeditationAt = float.NegativeInfinity;
        }

        /// <summary>Determines whether an EEG packet timestamp is still current.</summary>
        /// <param name="timestamp">Timestamp of the received EEG packet.</param>
        /// <returns>True when the packet is within the configured timeout.</returns>
        private static bool IsRecent(float timestamp)
        {
            return Time.realtimeSinceStartup - timestamp <= DataTimeoutSeconds;
        }
    }
}

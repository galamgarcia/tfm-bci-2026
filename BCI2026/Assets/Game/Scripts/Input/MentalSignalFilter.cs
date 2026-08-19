using System.Collections.Generic;
using UnityEngine;

namespace BciGame.Input
{
    /// <summary>Averages recent mental-state samples and removes isolated extreme values.</summary>
    public sealed class MentalSignalFilter
    {
        // Duration of the rolling window used to retain recent EEG samples.
        private readonly float _windowSeconds;
        // Minimum time between consecutive filtered-value publications.
        private readonly float _publishIntervalSeconds;
        // Proportion of extreme values removed from each end before averaging.
        private readonly float _trimPercentage;
        // Samples retained inside the current rolling window.
        private readonly List<Sample> _samples = new();
        // Timestamp when the most recent filtered value was published.
        private float _lastPublishedAt = float.NegativeInfinity;

        private readonly struct Sample
        {
            // Unscaled timestamp at which this sample was received.
            public readonly float Time;
            // Normalized relaxation or concentration sample value.
            public readonly float Value;

            public Sample(float time, float value)
            {
                Time = time;
                Value = value;
            }
        }

        public MentalSignalFilter(float windowSeconds, float publishIntervalSeconds, float trimPercentage)
        {
            _windowSeconds = windowSeconds;
            _publishIntervalSeconds = publishIntervalSeconds;
            _trimPercentage = trimPercentage;
        }

        /// <summary>Adds a sample and returns a filtered value when its publish interval elapses.</summary>
        public bool TryPublish(float value, float currentTime, out float filteredValue)
        {
            _samples.Add(new Sample(currentTime, Mathf.Clamp01(value)));
            _samples.RemoveAll(sample => currentTime - sample.Time > _windowSeconds);
            if (currentTime - _lastPublishedAt < _publishIntervalSeconds)
            {
                filteredValue = 0f;
                return false;
            }

            _lastPublishedAt = currentTime;
            List<float> values = new(_samples.Count);
            foreach (Sample sample in _samples) { values.Add(sample.Value); }
            values.Sort();
            int trimCount = Mathf.Min(Mathf.FloorToInt(values.Count * _trimPercentage), (values.Count - 1) / 2);
            float total = 0f;
            for (int index = trimCount; index < values.Count - trimCount; index++) { total += values[index]; }
            filteredValue = total / (values.Count - trimCount * 2);
            return true;
        }

        /// <summary>Clears buffered values after the EEG signal becomes invalid.</summary>
        public void Reset()
        {
            _samples.Clear();
            _lastPublishedAt = float.NegativeInfinity;
        }
    }
}

/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System.Collections.Generic;
using UnityEngine;

namespace Bit.Input
{
    /// <summary>Averages recent mental-state samples and removes isolated extreme values.</summary>
    public sealed class MentalSignalFilter
    {
        /// <summary>Stores one normalized mental-signal sample and its timestamp.</summary>
        private readonly struct Sample
        {
            // Unscaled timestamp at which this sample was received.
            public readonly float Time;
            // Normalized relaxation or concentration sample value.
            public readonly float Value;

            /// <summary>Creates a timestamped normalized mental-signal sample.</summary>
            /// <param name="time">Unscaled time at which the sample was received.</param>
            /// <param name="value">Normalized mental-signal value.</param>
            public Sample(float time, float value)
            {
                Time = time;
                Value = value;
            }
        }

        // Duration of the rolling window used to retain recent EEG samples.
        private readonly float _duration;
        // Minimum time between consecutive filtered-value publications.
        private readonly float _interval;
        // Proportion of extreme values removed from each end before averaging.
        private readonly float _trimPct;
        // Samples retained inside the current rolling window.
        private readonly List<Sample> _samples = new();
        // Timestamp when the most recent filtered value was published.
        private float _lastUpdatedAt = float.NegativeInfinity;

        /// <summary>Creates a filter for recent normalized mental-signal samples.</summary>
        /// <param name="duration">Seconds of samples kept for each average.</param>
        /// <param name="interval">Minimum seconds between published values.</param>
        /// <param name="trim">Proportion removed from each end before averaging.</param>
        public MentalSignalFilter(float duration, float interval, float trim)
        {
            _duration = duration;
            _interval = interval;
            _trimPct = trim;
        }

        /// <summary>Adds a sample and returns a filtered value when its publish interval elapses.</summary>
        /// <param name="value">Normalized mental-signal value to add.</param>
        /// <param name="time">Current unscaled time in seconds.</param>
        /// <param name="filteredValue">Updated filtered value, or zero when none is updated.</param>
        /// <returns>True, a filtered value was updated.</returns>
        public bool TryUpdate(float value, float time, out float filteredValue)
        {
            AddSample(value, time);
            RemoveExpiredSamples(time);
            if (!CanUpdate(time))
            {
                filteredValue = 0f;
                return false;
            }

            _lastUpdatedAt = time;
            filteredValue = CalculateFilteredValue();
            return true;
        }

        /// <summary>Adds one normalized value to the sample window.</summary>
        /// <param name="value">Mental-signal value to add.</param>
        /// <param name="time">Current unscaled time in seconds.</param>
        private void AddSample(float value, float time)
        {
            _samples.Add(new Sample(time, Mathf.Clamp01(value)));
        }

        /// <summary>Removes samples outside the configured time window.</summary>
        /// <param name="time">Current unscaled time in seconds.</param>
        private void RemoveExpiredSamples(float time)
        {
            _samples.RemoveAll(sample => time - sample.Time > _duration);
        }

        /// <summary>Determines if enough time passed to publish a value.</summary>
        /// <param name="time">Current unscaled time in seconds.</param>
        /// <returns>Whether a filtered value can be published.</returns>
        private bool CanUpdate(float time)
        {
            return time - _lastUpdatedAt >= _interval;
        }

        /// <summary>Calculates the average after excluding low and high values.</summary>
        /// <returns>The filtered mental-signal value.</returns>
        private float CalculateFilteredValue()
        {
            List<float> values = GetSortedValues();
            return CalculateAverage(values, GetTrimCount(values.Count));
        }

        /// <summary>Gets the current sample values in ascending order.</summary>
        /// <returns>The sorted sample values.</returns>
        private List<float> GetSortedValues()
        {
            List<float> values = new(_samples.Count);
            foreach (Sample sample in _samples)
            {
                values.Add(sample.Value);
            }
            values.Sort();
            return values;
        }

        /// <summary>Gets the number of low and high values excluded from the average.</summary>
        /// <param name="count">Number of available sorted values.</param>
        /// <returns>The number of values excluded from each end.</returns>
        private int GetTrimCount(int count)
        {
            return Mathf.Min(Mathf.FloorToInt(count * _trimPct), (count - 1) / 2);
        }

        /// <summary>Calculates the average of values between the excluded ends.</summary>
        /// <param name="values">Sorted values used for the average.</param>
        /// <param name="count">Number of values excluded from each end.</param>
        /// <returns>The average of the remaining values.</returns>
        private static float CalculateAverage(List<float> values, int count)
        {
            float total = 0f;
            for (int index = count; index < values.Count - count; index++)
            {
                total += values[index];
            }
            return total / (values.Count - count * 2);
        }

        /// <summary>Clears buffered values and publication timing.</summary>
        public void Reset()
        {
            _samples.Clear();
            _lastUpdatedAt = float.NegativeInfinity;
        }
    }
}

/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using BciGame.Input;
using NUnit.Framework;

namespace BciGame.Tests.Editor
{
    /// <summary>Validates mental-signal filtering behavior.</summary>
    public sealed class MentalSignalFilterTests
    {
        /// <summary>Verifies that values are published at the configured interval.</summary>
        [Test]
        public void TryUpdate_WaitsForConfiguredInterval()
        {
            MentalSignalFilter filter = new MentalSignalFilter(3f, 1f, 0.2f);

            Assert.That(filter.TryUpdate(0.5f, 0f, out _), Is.True);
            Assert.That(filter.TryUpdate(0.6f, 0.5f, out _), Is.False);
            Assert.That(filter.TryUpdate(0.6f, 1f, out _), Is.True);
        }

        /// <summary>Verifies that an isolated extreme sample is excluded from the average.</summary>
        [Test]
        public void TryUpdate_ExcludesAnIsolatedExtremeSample()
        {
            MentalSignalFilter filter = new MentalSignalFilter(3f, 1f, 0.2f);

            filter.TryUpdate(0.5f, 0f, out _);
            filter.TryUpdate(0.5f, 0.2f, out _);
            filter.TryUpdate(0.5f, 0.4f, out _);
            filter.TryUpdate(0.5f, 0.6f, out _);
            filter.TryUpdate(0.5f, 0.8f, out _);

            Assert.That(filter.TryUpdate(1f, 1f, out float value), Is.True);
            Assert.That(value, Is.EqualTo(0.5f).Within(0.001f));
        }
    }
}

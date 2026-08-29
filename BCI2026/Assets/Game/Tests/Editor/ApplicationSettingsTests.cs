/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.Core;
using NUnit.Framework;
using UnityEngine;

namespace Bit.Tests.Editor
{
    /// <summary>Validates the shared application settings asset.</summary>
    public sealed class ApplicationSettingsTests
    {
        /// <summary>Verifies that the configured target frame rate is positive.</summary>
        [Test]
        public void ApplicationSettings_HasValidTargetFrameRate()
        {
            ApplicationSettings settings = Resources.Load<ApplicationSettings>("ApplicationSettings");
            Assert.That(settings, Is.Not.Null, "ApplicationSettings must be available in Resources.");
            Assert.That(settings.GetTargetFrameRate(), Is.GreaterThan(0));
        }
    }
}

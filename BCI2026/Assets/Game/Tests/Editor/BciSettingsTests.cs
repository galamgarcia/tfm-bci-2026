/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.Core;
using Bit.Input;
using Bit.Services;
using NUnit.Framework;
using UnityEngine;

namespace Bit.Tests.Editor
{
    /// <summary>Validates the shared BCI settings and data-status rules.</summary>
    public sealed class BciSettingsTests
    {
        /// <summary>Verifies that each data status resolves to a configured icon.</summary>
        /// <param name="status">BrainLink data status whose icon is requested.</param>
        [TestCase(BrainLinkDataStatus.Disconnected)]
        [TestCase(BrainLinkDataStatus.ConnectedNoData)]
        [TestCase(BrainLinkDataStatus.PartialData)]
        [TestCase(BrainLinkDataStatus.CompleteData)]
        public void GetConnectionStatusIcon_ReturnsConfiguredIcon(BrainLinkDataStatus status)
        {
            BciSettings settings = Resources.Load<BciSettings>("BciSettings");
            Assert.That(settings, Is.Not.Null, "BciSettings must be available in Resources.");
            Assert.That(settings.GetConnectionStatusIcon(status), Is.Not.Null, $"Missing icon for {status}.");
        }

        /// <summary>Verifies classification of a BrainLink connection state.</summary>
        /// <param name="connected">Whether the device is connected.</param>
        /// <param name="hasData">Whether recent EEG data is available.</param>
        /// <param name="hasCompleteData">Whether recent complete EEG data is available.</param>
        /// <param name="hasValidSignal">Whether the EEG signal quality is valid.</param>
        /// <param name="expected">Expected data status.</param>
        [TestCase(false, false, false, false, BrainLinkDataStatus.Disconnected)]
        [TestCase(true, false, false, false, BrainLinkDataStatus.ConnectedNoData)]
        [TestCase(true, true, false, false, BrainLinkDataStatus.PartialData)]
        [TestCase(true, true, true, false, BrainLinkDataStatus.PartialData)]
        [TestCase(true, true, true, true, BrainLinkDataStatus.CompleteData)]
        public void Resolve_ClassifiesConnectionState(bool connected, bool hasData, bool hasCompleteData, bool hasValidSignal, BrainLinkDataStatus expected)
        {
            Assert.That(BrainLinkDataManager.Resolve(connected, hasData, hasCompleteData, hasValidSignal), Is.EqualTo(expected));
        }
    }
}

using BciGame.Core;
using BciGame.Services;
using NUnit.Framework;
using UnityEngine;

namespace BciGame.Tests.Editor
{
    public sealed class BciSettingsTests
    {
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

/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.Input;
using Bit.Gameplay;
using NUnit.Framework;
using System;
using UnityEngine;

namespace Bit.Tests.Editor
{
    /// <summary>Validates BrainLink blink gesture detection.</summary>
    public sealed class BlinkDetectorTests
    {
        /// <summary>Verifies that a sustained intensity produces only one event until rearmed.</summary>
        [Test]
        public void Process_EmitsOnceUntilIntensityReturnsBelowThreshold()
        {
            BlinkDetector detector = new BlinkDetector(50, 0.35f);

            Assert.That(detector.Process(60, true, 0f), Is.True);
            Assert.That(detector.Process(80, true, 0.1f), Is.False);
            Assert.That(detector.Process(20, true, 0.2f), Is.False);
            Assert.That(detector.Process(60, true, 0.3f), Is.True);
        }

        /// <summary>Verifies that a rearmed blink inside the refractory period is ignored.</summary>
        [Test]
        public void Process_RespectsCooldownAfterBlink()
        {
            BlinkDetector detector = new BlinkDetector(50, 0.35f);

            Assert.That(detector.Process(60, true, 0f), Is.True);
            Assert.That(detector.Process(20, true, 0.1f), Is.False);
            Assert.That(detector.Process(60, true, 0.2f), Is.False);
            Assert.That(detector.Process(20, true, 0.3f), Is.False);
            Assert.That(detector.Process(60, true, 0.36f), Is.True);
        }

        /// <summary>Verifies that invalid signal resets the detector before the next sample.</summary>
        [Test]
        public void Process_ResetsAndIgnoresSamplesWhenSignalIsInvalid()
        {
            BlinkDetector detector = new BlinkDetector(50, 0.35f);

            Assert.That(detector.Process(60, true, 0f), Is.True);
            Assert.That(detector.Process(60, false, 0.1f), Is.False);
            Assert.That(detector.Process(60, true, 0.2f), Is.True);
        }

        /// <summary>Verifies that InputController forwards a configured blink source event.</summary>
        [Test]
        public void InputController_ForwardsBlinkEvent()
        {
            GameObject gameObject = new GameObject("InputControllerTest");
            InputController input = gameObject.AddComponent<InputController>();
            TestBlinkInputSource source = new TestBlinkInputSource();
            int detected = 0;
            input.OnBlinkDetected += () => detected++;

            input.ConfigureSources(null, null, source);
            source.RaiseBlink();

            Assert.That(detected, Is.EqualTo(1));
            UnityEngine.Object.DestroyImmediate(gameObject);
        }

        private sealed class TestBlinkInputSource : IBlinkInputSource
        {
            public bool HasValidSignal => true;
            public event Action OnBlinkDetected;

            public void RaiseBlink()
            {
                OnBlinkDetected?.Invoke();
            }
        }
    }
}

/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Bit.Gameplay
{
    /// <summary>
    /// Verifies that procedural pupil offsets remain inside their eyes.
    /// </summary>
    public sealed class BitVisualTests
    {
        /// <summary>Verifies that neutral gaze leaves the pupil centered.</summary>
        [Test]
        public void NeutralDirectionReturnsCenter()
        {
            Vector2 result = BitEyeMovement.GetPupilOffset(Vector2.zero, new Vector2(0.18f, 0.58f), new Vector2(0.07f, 0.4f));

            Assert.That(result, Is.EqualTo(Vector2.zero));
        }

        /// <summary>Verifies that a cardinal gaze uses the available eye margin.</summary>
        [Test]
        public void CardinalDirectionsUseAvailableMargins()
        {
            Vector2 result = BitEyeMovement.GetPupilOffset(Vector2.right, new Vector2(0.18f, 0.58f), new Vector2(0.07f, 0.4f));

            Assert.That(result.x, Is.EqualTo(0.055f).Within(0.0001f));
            Assert.That(result.y, Is.EqualTo(0).Within(0.0001f));
        }

        /// <summary>Verifies that an oversized pupil cannot move outside its eye.</summary>
        [Test]
        public void OversizedPupilCannotLeaveEye()
        {
            Vector2 result = BitEyeMovement.GetPupilOffset(new Vector2(4, -4), new Vector2(0.18f, 0.58f), new Vector2(0.3f, 0.8f));

            Assert.That(result, Is.EqualTo(Vector2.zero));
        }

        /// <summary>Verifies that a blink starts and ends with the eyes open.</summary>
        [Test]
        public void BlinkScaleStartsAndEndsOpen()
        {
            Assert.That(BitEyeController.GetBlinkScaleFactor(0f, 0.08f), Is.EqualTo(1f).Within(0.0001f));
            Assert.That(BitEyeController.GetBlinkScaleFactor(1f, 0.08f), Is.EqualTo(1f).Within(0.0001f));
        }

        /// <summary>Verifies that the blink reaches its configured closed scale.</summary>
        [Test]
        public void BlinkScaleReachesClosedValue()
        {
            Assert.That(BitEyeController.GetBlinkScaleFactor(0.5f, 0.08f), Is.EqualTo(0.08f).Within(0.0001f));
        }

        /// <summary>Verifies that full relaxation produces the horizontal eye expression.</summary>
        [Test]
        public void RelaxationScaleReachesConfiguredValue()
        {
            Assert.That(BitEyeController.GetRelaxationScale(1f, 0.2f), Is.EqualTo(0.2f).Within(0.0001f));
        }

        /// <summary>Verifies that relaxation changes only the vertical eye scale.</summary>
        [Test]
        public void RelaxationKeepsEyeWidthUnchanged()
        {
            float originalWidth = 0.18f;
            float originalHeight = 0.58f;
            float relaxedHeight = originalHeight * BitEyeController.GetRelaxationScale(1f, 0.2f);

            Assert.That(originalWidth, Is.EqualTo(0.18f).Within(0.0001f));
            Assert.That(relaxedHeight, Is.EqualTo(0.116f).Within(0.0001f));
        }

        /// <summary>Verifies that concentration reaches the dark color at mid-transition.</summary>
        [Test]
        public void ConcentrationColorReachesTarget()
        {
            Color target = new Color(0.019608f, 0.435294f, 0.545098f, 1f);
            Color result = BitBodyController.GetConcentrationColor(Color.cyan, target, 0.5f);

            Assert.That(result, Is.EqualTo(target));
        }

        /// <summary>Verifies that a bounded idle gaze remains inside the eye margin.</summary>
        [Test]
        public void IdleGazeRemainsInsideEye()
        {
            Vector2 gaze = new Vector2(Mathf.Sin(1.7f), Mathf.Sin(2.39f));
            Vector2 result = BitEyeMovement.GetPupilOffset(gaze, new Vector2(0.18f, 0.58f), new Vector2(0.07f, 0.4f));

            Assert.That(Mathf.Abs(result.x), Is.LessThanOrEqualTo(0.055f));
            Assert.That(Mathf.Abs(result.y), Is.LessThanOrEqualTo(0.09f));
        }

        /// <summary>Verifies that the body idle offset reaches both cycle extremes.</summary>
        [Test]
        public void IdleBodyOffsetUsesConfiguredAmplitude()
        {
            Assert.That(BitBodyController.GetIdleBodyOffset(Mathf.PI * 0.5f, 0.015f), Is.EqualTo(0.015f).Within(0.0001f));
            Assert.That(BitBodyController.GetIdleBodyOffset(Mathf.PI * 1.5f, 0.015f), Is.EqualTo(-0.015f).Within(0.0001f));
        }

        /// <summary>Verifies that breathing expands width while compressing height.</summary>
        [Test]
        public void BreathingScaleDeformsTheBody()
        {
            Vector3 result = BitBodyController.GetBreathingScale(new Vector3(1.8f, 1.8f, 1f), Mathf.PI * 0.5f, 0.015f);

            Assert.That(result.x, Is.EqualTo(1.827f).Within(0.0001f));
            Assert.That(result.y, Is.EqualTo(1.773f).Within(0.0001f));
        }

        /// <summary>Verifies that a zero breathing base scale remains visible.</summary>
        [Test]
        public void BreathingScaleRecoversZeroBaseScale()
        {
            Vector3 result = BitBodyController.GetBreathingScale(Vector3.zero, 0f, 0.015f);

            Assert.That(result.x, Is.EqualTo(1.8135f).Within(0.0001f));
            Assert.That(result.y, Is.EqualTo(1.7865f).Within(0.0001f));
        }

        /// <summary>Verifies that jump stretch preserves width and expands height.</summary>
        [Test]
        public void JumpStretchDeformsTheBody()
        {
            Vector3 result = BitBodyController.GetJumpStretchScale(new Vector3(1.8f, 1.8f, 1f), 0.5f, 0.12f);

            Assert.That(result.x, Is.EqualTo(1.8f).Within(0.0001f));
            Assert.That(result.y, Is.EqualTo(2.016f).Within(0.0001f));
        }

        /// <summary>Verifies that landing squash preserves width and compresses height.</summary>
        [Test]
        public void LandingSquashDeformsTheBody()
        {
            Vector3 result = BitBodyController.GetLandingSquashScale(new Vector3(1.8f, 1.8f, 1f), 0.2f, 0.6f);

            Assert.That(result.x, Is.EqualTo(1.8f).Within(0.0001f));
            Assert.That(result.y, Is.EqualTo(0.72f).Within(0.0001f));
        }

    }
}

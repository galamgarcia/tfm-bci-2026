/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.Gameplay;
using Bit.Core;
using NUnit.Framework;
using UnityEngine;

namespace Bit.Tests
{
    /// <summary>Validates the camera calculations exposed by shared utilities.</summary>
    public sealed class CameraTests
    {
        /// <summary>Verifies a target inside the rope does not move the camera.</summary>
        [Test]
        public void TargetInsideRope_KeepsCameraPosition()
        {
            Vector2 result = Utils.GetCameraRopePosition(
                new Vector2(4f, 3f), new Vector2(0.5f, 0.5f),
                new Rect(0.3f, 0.3f, 0.4f, 0.4f), new Vector2(10f, 6f));

            Assert.That(result, Is.EqualTo(new Vector2(4f, 3f)));
        }

        /// <summary>Verifies horizontal and vertical corrections use viewport size independently.</summary>
        [Test]
        public void TargetOutsideRope_MovesOnlyTheCrossedAxes()
        {
            Vector2 result = Utils.GetCameraRopePosition(
                Vector2.zero, new Vector2(0.2f, 0.8f),
                new Rect(0.3f, 0.3f, 0.4f, 0.4f), new Vector2(10f, 6f));

            Assert.That(result, Is.EqualTo(new Vector2(-1f, 0.6f)));
        }

        /// <summary>Verifies camera bounds leave room for the complete orthographic viewport.</summary>
        [Test]
        public void ClampCameraCenter_AccountsForVisibleViewport()
        {
            Vector2 result = Utils.ClampCameraCenter(
                new Vector2(-10f, 10f), new Bounds(Vector3.zero, new Vector3(20f, 10f, 1f)),
                new Vector2(8f, 4f));

            Assert.That(result, Is.EqualTo(new Vector2(-6f, 3f)));
        }

        /// <summary>Verifies a viewport larger than its bounds uses the bounds center.</summary>
        [Test]
        public void ClampCameraCenter_WhenViewportIsLarger_UsesBoundsCenter()
        {
            Vector2 result = Utils.ClampCameraCenter(
                new Vector2(10f, -10f), new Bounds(new Vector3(3f, 4f, 0f), new Vector3(4f, 2f, 1f)),
                new Vector2(8f, 6f));

            Assert.That(result, Is.EqualTo(new Vector2(3f, 4f)));
        }
    }
}

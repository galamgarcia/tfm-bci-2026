/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;
using Bit.Core;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Bit.Input
{
    /// <summary>Tracks the user's head pose using AR Foundation face tracking and exposes normalized horizontal movement together with nod gesture detection.</summary>
    /// <remarks>The tracker must be calibrated before reporting meaningful values. Calibration is automatically started when the component is enabled.</remarks>
    public sealed class HeadPoseTracker : MonoBehaviour, IHeadInputSource
    {
        [Header("References")]
        [Tooltip("AR Face Manager used to track the user's head pose.")]
        [SerializeField] private ARFaceManager faceManager;
        
        [Header("Nod Detection")]
        [Tooltip("Minimum downward pitch (in degrees) required to detect the start of a nod.")]
        [SerializeField] private float nodMinPitch = 12f;
        [Tooltip("Pitch (in degrees) below which the head must return to complete the nod.")]
        [SerializeField] private float nodReturnPitch = 4f;
        [Tooltip("Maximum horizontal head deviation (yaw, in degrees) allowed while detecting a nod.")]
        [SerializeField] private float nodMaxYaw = 8f;
        [Tooltip("Minimum duration (in seconds) for a valid nod.")]
        [SerializeField] private float nodMinDuration = 0.2f;
        [Tooltip("Maximum duration (in seconds) for a valid nod.")]
        [SerializeField] private float nodMaxDuration = 0.9f;

        [Header("Horizontal Movement")]
        [Tooltip("Horizontal rotation (yaw, in degrees) that produces full left or right input.")]
        [SerializeField] private float inputYaw = 12f;

        // Neutral horizontal angle (as signed degrees) set during calibration.
        private float _centerYaw;

        // Neutral vertical angle (as signed degrees) set during calibration.
        private float _centerPitch;

        // Latest horizontal face angle in signed degrees.
        private float _yaw;

        // Latest vertical face angle in signed degrees.
        private float _pitch;

        // Time when the current calibration started.
        private float _calibrationStartedAt;

        // Determines whether calibration is active.
        private bool _isCalibrating;

        // Current state of nod detection.
        private NodState _nodState;

        // Time when the current nod started.
        private float _nodStartedAt;

        /// <summary>Indicates if AR Foundation currently tracks a face.</summary>
        public bool HasFace { get; private set; }

        /// <summary>Gets horizontal head input from minus one to one.</summary>
        public float HorizontalInput { get; private set; }

        /// <summary>Triggered when the user completes a valid nod.</summary>
        public event Action NodDetected;

        /// <summary>Defines the current step of nod detection.</summary>
        private enum NodState
        {
            Waiting,
            Returning
        }

        private void OnEnable()
        {
            faceManager.trackablesChanged.AddListener(OnFacesChanged);
            BeginCalibration();
        }

        private void OnDisable()
        {
            faceManager.trackablesChanged.RemoveListener(OnFacesChanged);
        }

        /// <summary>Starts a calibration process using recent head orientation samples.</summary>
        public void BeginCalibration()
        {
            _isCalibrating = true;
            _calibrationStartedAt = Time.unscaledTime;
            ResetNodState();
        }

        /// <summary>Updates head input whenever face tracking data changes.</summary>
        /// <param name="changes">Face changes received from AR Foundation.</param>
        private void OnFacesChanged(ARTrackablesChangedEventArgs<ARFace> changes)
        {
            ARFace face = GetChangedFace(changes);
            HasFace = face != null;
            if (!HasFace)
            {
                ResetNodState();
                return;
            }

            Vector3 rotation = face.transform.localEulerAngles;
            _yaw = Utils.NormalizeAngle(rotation.y);
            _pitch = Utils.NormalizeAngle(rotation.x);

            if (_isCalibrating)
            {
                UpdateCalibration();
                return;
            }

            UpdateHeadInput();
        }

        /// <summary>Gets the first added or updated face from an AR Foundation change.</summary>
        /// <param name="changes">Face changes received from AR Foundation.</param>
        /// <returns>The first added or updated face, or null when none is available.</returns>
        private static ARFace GetChangedFace(ARTrackablesChangedEventArgs<ARFace> changes)
        {
            foreach (ARFace face in changes.added)
            {
                return face;
            }

            foreach (ARFace face in changes.updated)
            {
                return face;
            }

            return null;
        }

        /// <summary>Updates the neutral pose while calibration is active.</summary>
        private void UpdateCalibration()
        {
            _centerYaw = Mathf.Lerp(_centerYaw, _yaw, 0.2f);
            _centerPitch = Mathf.Lerp(_centerPitch, _pitch, 0.2f);
            if (Time.unscaledTime - _calibrationStartedAt >= 0.8f)
            {
                _isCalibrating = false;
            }
        }

        /// <summary>Updates horizontal input and nod detection from the current pose.</summary>
        private void UpdateHeadInput()
        {
            float yawDelta = Mathf.DeltaAngle(_centerYaw, _yaw);
            float pitchDelta = Mathf.DeltaAngle(_centerPitch, _pitch);
            HorizontalInput = Mathf.Clamp(yawDelta / inputYaw, -1f, 1f);
            UpdateNodDetection(pitchDelta, yawDelta);
        }

        /// <summary>Evaluates if the current head movement matches a valid nod gesture.</summary>
        /// <param name="pitch">Vertical rotation relative to the calibrated pose, in degrees.</param>
        /// <param name="yaw">Horizontal rotation relative to the calibrated pose, in degrees.</param>
        private void UpdateNodDetection(float pitch, float yaw)
        {
            if (Mathf.Abs(yaw) > nodMaxYaw)
            {
                ResetNodState();
                return;
            }

            if (_nodState == NodState.Waiting)
            {
                if (pitch >= nodMinPitch)
                {
                    _nodState = NodState.Returning;
                    _nodStartedAt = Time.unscaledTime;
                }

                return;
            }

            float duration = Time.unscaledTime - _nodStartedAt;
            if (duration > nodMaxDuration)
            {
                ResetNodState();
                return;
            }

            if (pitch > nodReturnPitch || duration < nodMinDuration) { return; }

            ResetNodState();
            NodDetected?.Invoke();
        }

        /// <summary>Resets the internal nod detection state machine.</summary>
        private void ResetNodState()
        {
            _nodState = NodState.Waiting;
            _nodStartedAt = 0f;
        }

    }
}

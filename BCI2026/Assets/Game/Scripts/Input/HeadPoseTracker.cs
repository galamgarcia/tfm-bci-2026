/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;
using BciGame.Utilities;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace BciGame.Input
{
    /// <summary>
    /// Tracks the user's head pose using AR Foundation face tracking and exposes
    /// normalized horizontal movement together with nod gesture detection.
    /// </summary>
    /// <remarks>
    /// The tracker must be calibrated before reporting meaningful values.
    /// Calibration is automatically started when the component is enabled.
    /// </remarks>
    public sealed class HeadPoseTracker : MonoBehaviour
    {
        [Tooltip("AR Face Manager used to track the user's head pose.")]
        [SerializeField] private ARFaceManager faceManager;
        
        [Header("Nod Detection")]
        [Tooltip("Minimum downward pitch (in degrees) required to detect the start of a nod.")]
        [SerializeField] private float nodActivationPitchDegrees = 12f;
        [Tooltip("Pitch (in degrees) below which the head must return to complete the nod.")]
        [SerializeField] private float nodReturnPitchDegrees = 4f;
        [Tooltip("Maximum horizontal head deviation (yaw, in degrees) allowed while detecting a nod.")]
        [SerializeField] private float nodMaximumYawDeviationDegrees = 8f;
        [Tooltip("Minimum duration (in seconds) for a valid nod.")]
        [SerializeField] private float nodMinimumDurationSeconds = 0.2f;
        [Tooltip("Maximum duration (in seconds) for a valid nod.")]
        [SerializeField] private float nodMaximumDurationSeconds = 0.9f;

        [Header("Horizontal Movement")]
        [Tooltip("Minimum horizontal rotation (yaw, in degrees) required to trigger a left or right movement.")]
        [SerializeField] private float horizontalThresholdDegrees = 12f;

        // Neutral orientation established during calibration, expressed as signed degrees.
        private float _centerYaw;
        private float _centerPitch;
        // Most recent face pose sample, converted from Unity's unsigned Euler angles.
        private float _yaw;
        private float _pitch;
        // Timing and state for the calibration and active nod sequence.
        private float _calibrationStartedAt;
        private bool _isCalibrating;
        private NodState _nodState;
        private float _nodStartedAt;

        public bool HasFace { get; private set; }
        public float HorizontalInput { get; private set; }
        public event Action NodDetected;

        private enum NodState
        {
            Waiting,
            Returning
        }

        private void Awake()
        {
            if (faceManager == null)
            {
                faceManager = GetComponent<ARFaceManager>();
            }
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

        /// <summary>
        /// Starts a new calibration process using the current head orientation as the
        /// neutral reference pose.
        /// </summary>
        public void BeginCalibration()
        {
            _isCalibrating = true;
            _calibrationStartedAt = Time.unscaledTime;
            ResetNodState();
        }

        /// <summary>
        /// Updates the tracked head pose and evaluates horizontal movement and nod
        /// gestures whenever face tracking data changes.
        /// </summary>
        /// <param name="changes">Collection of tracked face changes reported by AR Foundation.</param>
        private void OnFacesChanged(ARTrackablesChangedEventArgs<ARFace> changes)
        {
            ARFace face = null;
            foreach (ARFace trackedFace in faceManager.trackables)
            {
                face = trackedFace;
                break;
            }

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
                _centerYaw = Mathf.Lerp(_centerYaw, _yaw, 0.2f);
                _centerPitch = Mathf.Lerp(_centerPitch, _pitch, 0.2f);
                if (Time.unscaledTime - _calibrationStartedAt >= 0.8f)
                {
                    _isCalibrating = false;
                }
                return;
            }

            float yawDelta = Mathf.DeltaAngle(_centerYaw, _yaw);
            float pitchDelta = Mathf.DeltaAngle(_centerPitch, _pitch);
            HorizontalInput = Mathf.Clamp(yawDelta / horizontalThresholdDegrees, -1f, 1f);
            UpdateNodDetection(pitchDelta, yawDelta);
        }

        /// <summary>
        /// Evaluates whether the current head movement matches a valid nod gesture.
        /// </summary>
        /// <param name="pitchDelta"> Vertical rotation relative to the calibrated pose, in degrees. </param>
        /// <param name="yawDelta"> Horizontal rotation relative to the calibrated pose, in degrees. </param>
        private void UpdateNodDetection(float pitchDelta, float yawDelta)
        {
            if (Mathf.Abs(yawDelta) > nodMaximumYawDeviationDegrees)
            {
                ResetNodState();
                return;
            }

            if (_nodState == NodState.Waiting)
            {
                if (pitchDelta >= nodActivationPitchDegrees)
                {
                    _nodState = NodState.Returning;
                    _nodStartedAt = Time.unscaledTime;
                }

                return;
            }

            float duration = Time.unscaledTime - _nodStartedAt;
            if (duration > nodMaximumDurationSeconds)
            {
                ResetNodState();
                return;
            }

            if (pitchDelta > nodReturnPitchDegrees || duration < nodMinimumDurationSeconds)
            {
                return;
            }

            ResetNodState();
            NodDetected?.Invoke();
        }

        /// <summary>
        /// Resets the internal nod detection state machine.
        /// </summary>
        private void ResetNodState()
        {
            _nodState = NodState.Waiting;
            _nodStartedAt = 0f;
        }

    }
}

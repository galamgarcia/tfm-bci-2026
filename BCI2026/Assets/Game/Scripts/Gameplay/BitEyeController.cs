/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;

namespace Bit.Gameplay
{
    /// <summary>Procedurally moves Bit's pupils while preserving the front-facing body.</summary>
    [ExecuteAlways]
    public sealed class BitEyeController : MonoBehaviour
    {
        [Header("Eyes")]
        [Tooltip("White rectangular visual of the left eye.")]
        [SerializeField] private Transform leftWhite;

        [Tooltip("Pupil transform inside the left eye.")]
        [SerializeField] private Transform leftPupil;

        [Tooltip("White rectangular visual of the right eye.")]
        [SerializeField] private Transform rightWhite;

        [Tooltip("Pupil transform inside the right eye.")]
        [SerializeField] private Transform rightPupil;

        [Header("Transition")]
        [Tooltip("Time in seconds used to smooth pupil movement.")]
        [SerializeField, Min(0.01f)] private float smoothTime = 0.08f;

        [Header("Look")]
        [Tooltip("Maximum fraction of the available eye margin used by the pupils.")]
        [Range(0f, 1f)]
        [SerializeField] private float lookAmount = 0.2f;

        [Header("Blink")]
        [Tooltip("Total time in seconds used to close and reopen the eyes.")]
        [SerializeField, Min(0.02f)] private float blinkDuration = 0.18f;

        [Tooltip("Vertical eye scale retained when the blink is fully closed.")]
        [SerializeField, Range(0f, 1f)] private float blinkClosedScale = 0.08f;

        [Header("Landing")]
        [Tooltip("Vertical eye scale retained during the landing impact.")]
        [SerializeField, Range(0f, 1f)] private float landingEyeScale = 0.2f;

        [Tooltip("Time in seconds used to restore the eyes after landing.")]
        [SerializeField, Min(0.02f)] private float landingEyeDuration = 0.45f;

        [Header("Relaxation")]
        [Tooltip("Vertical scale retained by the eyes at full relaxation.")]
        [SerializeField, Range(0f, 1f)] private float relaxedEyeScale = 0.4f;

        [Header("Idle Blink")]
        [Tooltip("Minimum and maximum time in seconds between automatic idle blinks.")]
        [SerializeField] private Vector2 idleBlinkInterval = new Vector2(3f, 6f);

        // The target positions for both pupils.
        private Vector3 _leftTarget;
        private Vector3 _rightTarget;
        // The velocity state used by SmoothDamp.
        private Vector3 _leftVelocity;
        private Vector3 _rightVelocity;
        // The serialized eye and pupil dimensions used for safe offsets.
        private Vector2 _leftEyeSize;
        private Vector2 _leftPupilSize;
        private Vector2 _rightEyeSize;
        private Vector2 _rightPupilSize;
        // The original scales restored after each blink.
        private Vector3 _leftWhiteScale;
        private Vector3 _leftPupilScale;
        private Vector3 _rightWhiteScale;
        private Vector3 _rightPupilScale;
        // The elapsed time of the active blink.
        private float _blinkElapsed;
        // Whether the controller is currently closing or opening the eyes.
        private bool _isBlinking;
        // Whether the eyes are currently showing the landing expression.
        private bool _isLanding;
        // Elapsed time used by the landing eye expression.
        private float _landingElapsed;
        // Current normalized relaxation expression intensity.
        private float _relaxationIntensity;
        // Whether automatic idle blinks are active.
        private bool _isIdle;
        // Elapsed time used to schedule the next idle blink.
        private float _idleBlinkElapsed;
        // Random interval currently used for the next idle blink.
        private float _nextIdleBlinkInterval;

        private void Awake()
        {
            _leftEyeSize = GetSize(leftWhite);
            _leftPupilSize = GetSize(leftPupil);
            _rightEyeSize = GetSize(rightWhite);
            _rightPupilSize = GetSize(rightPupil);
            _leftWhiteScale = leftWhite.localScale;
            _leftPupilScale = leftPupil.localScale;
            _rightWhiteScale = rightWhite.localScale;
            _rightPupilScale = rightPupil.localScale;
            _blinkElapsed = 0f;
            _isBlinking = false;
            _isIdle = false;
            _idleBlinkElapsed = 0f;
            SetLookDirection(BitLookDirection.Neutral);
        }

        private void Update()
        {
            float duration = Mathf.Max(0.01f, smoothTime);
            leftPupil.localPosition = Vector3.SmoothDamp(leftPupil.localPosition, _leftTarget, ref _leftVelocity, duration);
            rightPupil.localPosition = Vector3.SmoothDamp(rightPupil.localPosition, _rightTarget, ref _rightVelocity, duration);

            if (_isBlinking)
            {
                UpdateBlink();
            }

            if (_isLanding)
            {
                UpdateLandingExpression();
            }

            if (!_isBlinking && !_isLanding)
            {
                UpdateRelaxationExpression();
            }

            if (_isIdle)
            {
                UpdateIdle();
            }
        }

        /// <summary>Sets the cardinal direction represented by both pupils.</summary>
        /// <param name="direction">The direction Bit should look toward.</param>
        public void SetLookDirection(BitLookDirection direction)
        {
            _isIdle = false;

            Vector2 gaze = direction switch
            {
                BitLookDirection.Left   => Vector2.left,
                BitLookDirection.Right  => Vector2.right,
                BitLookDirection.Up     => Vector2.up,
                BitLookDirection.Down   => Vector2.down,
                _                       => Vector2.zero
            };

            _leftTarget = GetTarget(gaze, _leftEyeSize, _leftPupilSize, lookAmount);
            _rightTarget = GetTarget(gaze, _rightEyeSize, _rightPupilSize, lookAmount);
        }

        /// <summary>Starts subtle autonomous pupil movement and periodic blinking.</summary>
        public void StartEyesIdle()
        {
            _isIdle = true;
            _idleBlinkElapsed = 0f;
            _nextIdleBlinkInterval = GetNextBlinkInterval();
        }

        /// <summary>Stops automatic idle blinks while preserving the current gaze.</summary>
        public void StopEyesIdle()
        {
            _isIdle = false;
            _idleBlinkElapsed = 0f;
        }

        /// <summary>Starts a complete procedural blink without changing the current gaze.</summary>
        public void Blink()
        {
            _blinkElapsed = 0f;
            _isBlinking = true;
        }

        /// <summary>Shows the compressed eye expression used during landing impact.</summary>
        public void PlayLandingExpression()
        {
            _landingElapsed = 0f;
            _isLanding = true;
        }

        /// <summary>Stops transient eye expressions and restores the original eye scales.</summary>
        public void ResetEyeState()
        {
            _isBlinking = false;
            _isLanding = false;
            _blinkElapsed = 0f;
            _landingElapsed = 0f;
            leftWhite.localScale = _leftWhiteScale;
            leftPupil.localScale = _leftPupilScale;
            rightWhite.localScale = _rightWhiteScale;
            rightPupil.localScale = _rightPupilScale;
        }

        /// <summary>Sets the normalized relaxation expression intensity.</summary>
        /// <param name="intensity">Relaxation value in the range from zero to one.</param>
        public void SetRelaxation(float intensity)
        {
            _relaxationIntensity = Mathf.Clamp01(intensity);
        }

        /// <summary>Clears the relaxation expression and restores the original eye scales.</summary>
        public void ResetRelaxation()
        {
            _relaxationIntensity = 0f;
            ResetEyeState();
        }

        /// <summary>Applies the relaxed horizontal-bar eye expression.</summary>
        private void UpdateRelaxationExpression()
        {
            float scale = GetRelaxationScale(_relaxationIntensity, relaxedEyeScale);
            SetVerticalScale(leftWhite, _leftWhiteScale, scale);
            SetVerticalScale(leftPupil, _leftPupilScale, scale);
            SetVerticalScale(rightWhite, _rightWhiteScale, scale);
            SetVerticalScale(rightPupil, _rightPupilScale, scale);
        }

        /// <summary>Calculates the vertical eye scale for a normalized relaxation intensity.</summary>
        /// <param name="intensity">Relaxation value in the range from zero to one.</param>
        /// <param name="relaxedScale">Scale retained at full relaxation.</param>
        /// <returns>The interpolated vertical scale.</returns>
        public static float GetRelaxationScale(float intensity, float relaxedScale)
        {
            return Mathf.Lerp(1f, Mathf.Clamp01(relaxedScale), Mathf.Clamp01(intensity));
        }

        /// <summary>Restores the eyes after the landing expression duration.</summary>
        private void UpdateLandingExpression()
        {
            float duration = Mathf.Max(0.02f, landingEyeDuration);
            float delta = Application.isPlaying ? Time.deltaTime : 1f / 60f;
            _landingElapsed += delta;
            float progress = Mathf.Clamp01(_landingElapsed / duration);
            float scale = Mathf.Lerp(Mathf.Clamp01(landingEyeScale), 1f, Mathf.SmoothStep(0f, 1f, progress));
            SetVerticalScale(leftWhite, _leftWhiteScale, scale);
            SetVerticalScale(leftPupil, _leftPupilScale, scale);
            SetVerticalScale(rightWhite, _rightWhiteScale, scale);
            SetVerticalScale(rightPupil, _rightPupilScale, scale);

            if (progress >= 1f)
            {
                leftWhite.localScale = _leftWhiteScale;
                leftPupil.localScale = _leftPupilScale;
                rightWhite.localScale = _rightWhiteScale;
                rightPupil.localScale = _rightPupilScale;
                _isLanding = false;
            }
        }

        /// <summary>Advances the idle blink timer without changing the user's gaze direction.</summary>
        private void UpdateIdle()
        {
            float delta = Application.isPlaying ? Time.deltaTime : 1f / 60f;
            _idleBlinkElapsed += delta;

            if (!_isBlinking && _idleBlinkElapsed >= _nextIdleBlinkInterval)
            {
                Blink();
                _idleBlinkElapsed = 0f;
                _nextIdleBlinkInterval = GetNextBlinkInterval();
            }
        }

        /// <summary>Chooses the delay before the next automatic idle blink.</summary>
        /// <returns>A random blink interval within the configured range.</returns>
        private float GetNextBlinkInterval()
        {
            float min = Mathf.Max(0.5f, idleBlinkInterval.x);
            float max = Mathf.Max(min, idleBlinkInterval.y);
            return Random.Range(min, max);
        }

        /// <summary>Advances the blink and restores the original eye scales when it ends.</summary>
        private void UpdateBlink()
        {
            float duration = Mathf.Max(0.02f, blinkDuration);
            float delta = Application.isPlaying ? Time.deltaTime : 1f / 60f;
            _blinkElapsed += delta;

            float progress = _blinkElapsed / duration;
            float scale = GetBlinkScaleFactor(progress, blinkClosedScale);
            SetVerticalScale(leftWhite, _leftWhiteScale, scale);
            SetVerticalScale(leftPupil, _leftPupilScale, scale);
            SetVerticalScale(rightWhite, _rightWhiteScale, scale);
            SetVerticalScale(rightPupil, _rightPupilScale, scale);

            if (progress >= 1f)
            {
                leftWhite.localScale = _leftWhiteScale;
                leftPupil.localScale = _leftPupilScale;
                rightWhite.localScale = _rightWhiteScale;
                rightPupil.localScale = _rightPupilScale;
                _isBlinking = false;
            }
        }

        /// <summary>Applies a vertical scale while preserving the original width and depth.</summary>
        /// <param name="visual">Visual transform to scale.</param>
        /// <param name="originalScale">Scale captured before the blink.</param>
        /// <param name="factor">Vertical scale factor.</param>
        private static void SetVerticalScale(Transform visual, Vector3 originalScale, float factor)
        {
            visual.localScale = new Vector3(originalScale.x, originalScale.y * factor, originalScale.z);
        }

        /// <summary>Returns a smooth vertical scale from open to closed and back.</summary>
        /// <param name="normalizedTime">Blink progress from zero to one.</param>
        /// <param name="closedScale">Vertical scale retained at the blink midpoint.</param>
        /// <returns>The scale factor to apply to the original eye height.</returns>
        public static float GetBlinkScaleFactor(float normalizedTime, float closedScale)
        {
            float progress = Mathf.Clamp01(normalizedTime);
            float closure = Mathf.Sin(progress * Mathf.PI);
            return Mathf.Lerp(1f, Mathf.Clamp01(closedScale), closure);
        }


        /// <summary>Gets the absolute local width and height of a visual transform. </summary>
        /// <param name="visual">Transform whose local scale represents its dimensions.</param>
        /// <returns>The positive local width and height.</returns>
        private static Vector2 GetSize(Transform visual)
        {
            return new Vector2(Mathf.Abs(visual.localScale.x), Mathf.Abs(visual.localScale.y));
        }

        /// <summary>Converts a gaze direction into a constrained local pupil target.</summary>
        /// <param name="direction">The normalized gaze direction.</param>
        /// <param name="eyeSize">The eye width and height in local units.</param>
        /// <param name="pupilSize">The pupil width and height in local units.</param>
        /// <param name="amount">The fraction of the available eye margin to use.</param>
        /// <returns>A local pupil position that remains inside the eye.</returns>
        private static Vector3 GetTarget(Vector2 direction, Vector2 eyeSize, Vector2 pupilSize, float amount)
        {
            Vector2 offset = BitEyeMovement.GetPupilOffset(direction, eyeSize, pupilSize);
            offset *= Mathf.Clamp01(amount);
            return new Vector3(offset.x, offset.y, 0);
        }
    }
}

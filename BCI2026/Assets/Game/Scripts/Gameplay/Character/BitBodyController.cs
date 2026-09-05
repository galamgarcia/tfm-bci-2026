/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;

namespace Bit.Gameplay
{
    /// <summary>Controls Bit's visual body motion without changing its physics root.</summary>
    [ExecuteAlways]
    public sealed class BitBodyController : MonoBehaviour
    {
        [Header("Visual")]
        [Tooltip("Visual root animated by this controller. Keep it separate from the physics root.")]
        [SerializeField] private Transform visualRoot;

        [Tooltip("Body visual deformed by the breathing idle. Keep it separate from the physics root.")]
        [SerializeField] private Transform bodyVisual;

        [Tooltip("Body renderer used for temporary concentration color feedback.")]
        [SerializeField] private SpriteRenderer bodyRenderer;

        [Header("Idle")]
        [Tooltip("Vertical distance in local units used by the idle bob.")]
        [SerializeField, Min(0f)] private float idleBobAmount = 0.015f;

        [Tooltip("Time in seconds for one complete idle body cycle.")]
        [SerializeField, Min(0.5f)] private float idlePeriod = 4f;

        [Tooltip("Minimum and maximum seconds between the floating and breathing idle phases.")]
        [SerializeField] private Vector2 idleInterval = new Vector2(3f, 6f);

        [Tooltip("Maximum proportional body deformation used by the breathing idle.")]
        [SerializeField, Range(0f, 0.05f)] private float breathingAmount = 0.015f;

        [Header("Jump")]
        [Tooltip("Proportional vertical stretch maintained while Bit is airborne.")]
        [SerializeField, Range(0f, 0.3f)] private float jumpStretchAmount = 0.12f;

        [Tooltip("Maximum proportional squash used when Bit lands.")]
        [SerializeField, Range(0f, 0.8f)] private float landingSquashAmount = 0.6f;

        [Tooltip("Duration in seconds of the landing squash pose.")]
        [SerializeField, Min(0.02f)] private float landingPoseDuration = 0.45f;

        [Header("Concentration")]
        [Tooltip("Strong blue body color shown while concentration is high.")]
        [SerializeField] private Color concentrationColor = new Color(0f, 0.545098f, 1f, 1f);

        [Tooltip("Total time in seconds for the body to darken and return to normal.")]
        [SerializeField, Min(0.02f)] private float concentrationTransitionDuration = 2.4f;

        // Current visual state of the body.
        private BodyState _state;
        // State restored after the landing pose ends.
        private BodyState _stateAfterJump;
        // Elapsed time used by the active visual state.
        private float _stateElapsed;
        // The original visual root position restored when Idle stops.
        private Vector3 _visualRootPosition;
        // The original body scale restored when Idle stops.
        private Vector3 _bodyVisualScale;
        // Whether the original visual state has been captured.
        private bool _hasVisualRootState;
        // Original body color restored after concentration feedback.
        private Color _bodyNormalColor;
        // Original renderer tint restored after concentration feedback.
        private Color _bodyNormalRendererColor;
        // Whether the concentration color transition is active.
        private bool _isConcentrationTransitioning;
        // Elapsed time used by the concentration color transition.
        private float _concentrationElapsed;
        // Reusable property block for temporary body color overrides.
        private MaterialPropertyBlock _bodyPropertyBlock;
        // Time elapsed in the current idle phase.
        private float _idleElapsed;
        // Duration selected for the current idle phase.
        private float _nextIdleInterval;
        private enum BodyState
        {
            None,
            FloatingIdle,
            BreathingIdle,
            Jumping,
            LandingSquash
        }

        private void Awake()
        {
            if (visualRoot == null || bodyVisual == null || bodyRenderer == null)
            {
                return;
            }

            _visualRootPosition = visualRoot.localPosition;
            _bodyVisualScale = GetBodyBaseScale(bodyVisual.localScale);
            bodyVisual.localScale = _bodyVisualScale;
            _bodyNormalColor = bodyRenderer.sharedMaterial != null && bodyRenderer.sharedMaterial.HasProperty("_Color") ? bodyRenderer.sharedMaterial.GetColor("_Color") : Color.white;
            _bodyNormalRendererColor = bodyRenderer.color;
            _bodyPropertyBlock = new MaterialPropertyBlock();
            SetBodyColor(_bodyNormalColor);
            _hasVisualRootState = true;
            _state = BodyState.None;
            _stateAfterJump = BodyState.None;
            _stateElapsed = 0f;
            _idleElapsed = 0f;
            _nextIdleInterval = GetNextIdleInterval();
        }

        private void OnDisable()
        {
            _state = BodyState.None;
            _stateAfterJump = BodyState.None;
            _stateElapsed = 0f;
            _idleElapsed = 0f;
            _nextIdleInterval = GetNextIdleInterval();
            ResetConcentrationTransition();
        }

        private void Update()
        {
            if (!_hasVisualRootState || visualRoot == null || bodyVisual == null)
            {
                _state = BodyState.None;
                return;
            }

            float delta = Application.isPlaying ? Time.deltaTime : 1f / 60f;
            if (_isConcentrationTransitioning)
            {
                UpdateConcentrationTransition(delta);
            }

            if (_state == BodyState.Jumping)
            {
                bodyVisual.localScale = GetJumpStretchScale(_bodyVisualScale, 0.5f, jumpStretchAmount);
                return;
            }

            if (_state == BodyState.LandingSquash)
            {
                UpdateLandingSquash(delta);
                return;
            }

            if (_state == BodyState.None)
            {
                return;
            }

            if (IsIdleState(_state))
            {
                UpdateIdleAlternation(delta);
            }

            _stateElapsed += delta;
            float period = Mathf.Max(0.5f, idlePeriod);
            float angle = _stateElapsed / period * Mathf.PI * 2f;
            if (_state == BodyState.FloatingIdle)
            {
                visualRoot.localPosition = _visualRootPosition + new Vector3(0f, GetIdleBodyOffset(angle, idleBobAmount), 0f);
            }
            else
            {
                bodyVisual.localScale = GetBreathingScale(_bodyVisualScale, angle, breathingAmount);
            }
        }

        /// <summary>Starts the alternating floating and breathing body idle sequence.</summary>
        public void StartIdle()
        {
            StartFloatingIdle();
        }

        /// <summary>Starts the floating body idle that moves the visual root vertically.</summary>
        public void StartFloatingIdle()
        {
            if (!_hasVisualRootState)
            {
                return;
            }

            ResetVisualState();
            _state = BodyState.FloatingIdle;
            _stateElapsed = 0f;
            _idleElapsed = 0f;
            _nextIdleInterval = GetNextIdleInterval();
        }

        /// <summary>Starts the breathing body idle that deforms the body without moving it.</summary>
        public void StartBreathingIdle()
        {
            if (!_hasVisualRootState)
            {
                return;
            }

            ResetVisualState();
            _state = BodyState.BreathingIdle;
            _stateElapsed = 0f;
            _idleElapsed = 0f;
            _nextIdleInterval = GetNextIdleInterval();
        }

        /// <summary>Stops either body idle mode and restores the original visual state.</summary>
        public void StopBodyIdle()
        {
            if (IsIdleState(_state))
            {
                _state = BodyState.None;
            }

            _stateElapsed = 0f;
            _idleElapsed = 0f;
            ResetVisualState();
        }

        /// <summary>Stops every visual body state and restores its original transform.</summary>
        public void ResetBodyState()
        {
            _state = BodyState.None;
            _stateAfterJump = BodyState.None;
            _stateElapsed = 0f;
            ResetVisualState();
            ResetConcentrationTransition();
        }

        /// <summary>Starts the temporary dark-blue concentration transition.</summary>
        public void PlayConcentrationTransition()
        {
            if (!_hasVisualRootState) { return; }
            _concentrationElapsed = Mathf.Max(0.02f, concentrationTransitionDuration) * 0.5f;
            _isConcentrationTransitioning = true;
            SetBodyColor(concentrationColor);
        }

        /// <summary>Shows or clears the persistent high-concentration body feedback.</summary>
        /// <param name="isHigh">Whether concentration is currently high.</param>
        public void SetConcentrationHigh(bool isHigh)
        {
            if (isHigh)
            {
                _isConcentrationTransitioning = false;
                SetBodyColor(concentrationColor);
            }
            else
            {
                ResetConcentrationTransition();
            }
        }

        /// <summary>Gets the visual root moved by the floating and jump previews.</summary>
        /// <returns>The configured visual root transform.</returns>
        public Transform GetVisualRoot()
        {
            return visualRoot;
        }

        /// <summary>Starts the visual stretch maintained while Bit is airborne.</summary>
        public void StartJump()
        {
            if (!_hasVisualRootState) { return; }
            ResetVisualState();
            _stateAfterJump = IsIdleState(_state) ? _state : BodyState.None;
            _state = BodyState.Jumping;
            _stateElapsed = 0f;
        }

        /// <summary>Plays the visual squash used when Bit lands.</summary>
        public void PlayLandingSquash()
        {
            if (!_hasVisualRootState) { return; }
            ResetVisualState();
            _state = BodyState.LandingSquash;
            _stateElapsed = 0f;
        }

        /// <summary>Restores the visual body state captured before animation started.</summary>
        public void ResetVisualState()
        {
            if (!_hasVisualRootState || _bodyVisualScale.sqrMagnitude <= 0.000001f || visualRoot == null || bodyVisual == null) { return; }
            visualRoot.localPosition = _visualRootPosition;
            bodyVisual.localScale = _bodyVisualScale;
        }

        /// <summary>Advances the concentration transition and restores the normal color at its end.</summary>
        /// <param name="delta">Elapsed time since the previous update.</param>
        private void UpdateConcentrationTransition(float delta)
        {
            _concentrationElapsed += delta;
            float progress = _concentrationElapsed / Mathf.Max(0.02f, concentrationTransitionDuration);
            SetBodyColor(GetConcentrationColor(_bodyNormalColor, concentrationColor, progress));

            if (progress >= 1f)
            {
                ResetConcentrationTransition();
            }
        }

        /// <summary>Alternates the floating and breathing idle phases.</summary>
        /// <param name="delta">Elapsed time since the previous update.</param>
        private void UpdateIdleAlternation(float delta)
        {
            _idleElapsed += delta;
            if (_idleElapsed < _nextIdleInterval) { return; }

            if (_state == BodyState.FloatingIdle)
            {
                StartBreathingIdle();
            }
            else
            {
                StartFloatingIdle();
            }
        }

        /// <summary>Chooses the duration of the next body idle phase.</summary>
        /// <returns>A random idle interval within the configured range.</returns>
        private float GetNextIdleInterval()
        {
            float min = Mathf.Max(0.5f, idleInterval.x);
            float max = Mathf.Max(min, idleInterval.y);
            return Random.Range(min, max);
        }

        /// <summary>Restores the normal body color and stops concentration feedback.</summary>
        private void ResetConcentrationTransition()
        {
            _isConcentrationTransitioning = false;
            _concentrationElapsed = 0f;
            if (bodyRenderer != null)
            {
                bodyRenderer.SetPropertyBlock(null);
                bodyRenderer.color = _bodyNormalRendererColor;
            }
        }

        /// <summary>Applies a temporary body color without changing the shared material.</summary>
        /// <param name="color">Color assigned through the renderer property block.</param>
        private void SetBodyColor(Color color)
        {
            if (bodyRenderer == null) { return; }
            _bodyPropertyBlock ??= new MaterialPropertyBlock();
            bodyRenderer.GetPropertyBlock(_bodyPropertyBlock);
            _bodyPropertyBlock.SetColor("_Color", color);
            bodyRenderer.SetPropertyBlock(_bodyPropertyBlock);
        }

        /// <summary>Advances the landing squash and restores the previous idle when it ends.</summary>
        /// <param name="delta">Elapsed time since the previous update.</param>
        private void UpdateLandingSquash(float delta)
        {
            _stateElapsed += delta;
            float duration = landingPoseDuration;
            float progress = _stateElapsed / Mathf.Max(0.02f, duration);
            Vector3 scale = GetLandingSquashScale(_bodyVisualScale, progress, landingSquashAmount);
            bodyVisual.localScale = scale;

            if (progress < 1f)
            {
                return;
            }

            _state = _stateAfterJump;
            _stateAfterJump = BodyState.None;
            _stateElapsed = 0f;
            ResetVisualState();
        }

        /// <summary>Checks whether a body state is one of the autonomous idle states.</summary>
        /// <param name="state">Body state to inspect.</param>
        /// <returns>True when the state is a floating or breathing Idle.</returns>
        private static bool IsIdleState(BodyState state)
        {
            return state == BodyState.FloatingIdle || state == BodyState.BreathingIdle;
        }

        /// <summary>Returns the vertical offset for a body idle cycle.</summary>
        /// <param name="angle">Current idle cycle angle in radians.</param>
        /// <param name="amount">Maximum local vertical offset.</param>
        /// <returns>The local vertical offset.</returns>
        public static float GetIdleBodyOffset(float angle, float amount)
        {
            return Mathf.Sin(angle) * Mathf.Max(0f, amount);
        }

        /// <summary>Calculates the breathing deformation from an original body scale.</summary>
        /// <param name="originalScale">Body scale captured before the breathing animation.</param>
        /// <param name="angle">Current breathing cycle angle in radians.</param>
        /// <param name="amount">Maximum proportional deformation.</param>
        /// <returns>The deformed body scale.</returns>
        public static Vector3 GetBreathingScale(Vector3 originalScale, float angle, float amount)
        {
            originalScale = GetBodyScaleOrDefault(originalScale);

            float progress = (Mathf.Sin(angle) + 1f) * 0.5f;
            float deformation = Mathf.Clamp01(Mathf.Max(0f, amount)) * progress;
            return new Vector3(originalScale.x * (1f + deformation), originalScale.y * (1f - deformation), originalScale.z);
        }

        /// <summary>Calculates the smooth darken-and-return concentration color transition.</summary>
        /// <param name="normalColor">Body color outside the transition.</param>
        /// <param name="targetColor">Dark color reached at the transition midpoint.</param>
        /// <param name="normalizedTime">Transition progress from zero to one.</param>
        /// <returns>The current transition color.</returns>
        public static Color GetConcentrationColor(Color normalColor, Color targetColor, float normalizedTime)
        {
            float progress = Mathf.Sin(Mathf.Clamp01(normalizedTime) * Mathf.PI);
            return Color.Lerp(normalColor, targetColor, progress);
        }

        /// <summary>Calculates the temporary stretch scale for a jump start.</summary>
        /// <param name="originalScale">Body scale captured before the pose.</param>
        /// <param name="normalizedTime">Pose progress from zero to one.</param>
        /// <param name="amount">Maximum proportional stretch.</param>
        /// <returns>The interpolated body scale.</returns>
        public static Vector3 GetJumpStretchScale(Vector3 originalScale, float normalizedTime, float amount)
        {
            originalScale = GetBodyScaleOrDefault(originalScale);
            float deformation = GetPoseDeformation(normalizedTime, amount);
            return new Vector3(originalScale.x, originalScale.y * (1f + deformation), originalScale.z);
        }

        /// <summary>Calculates the temporary squash scale for a landing.</summary>
        /// <param name="originalScale">Body scale captured before the pose.</param>
        /// <param name="normalizedTime">Pose progress from zero to one.</param>
        /// <param name="amount">Maximum proportional squash.</param>
        /// <returns>The interpolated body scale.</returns>
        public static Vector3 GetLandingSquashScale(Vector3 originalScale, float normalizedTime, float amount)
        {
            originalScale = GetBodyScaleOrDefault(originalScale);
            float deformation = GetLandingDeformation(normalizedTime, amount);
            return new Vector3(originalScale.x, originalScale.y * (1f - deformation), originalScale.z);
        }

        /// <summary>Calculates the impact squash, rebound and final settling deformation.</summary>
        /// <param name="normalizedTime">Pose progress from zero to one.</param>
        /// <param name="amount">Maximum proportional squash.</param>
        /// <returns>The current proportional landing deformation.</returns>
        private static float GetLandingDeformation(float normalizedTime, float amount)
        {
            float progress = Mathf.Clamp01(normalizedTime);
            float squash = Mathf.Clamp01(amount);

            if (progress <= 0.2f)
            {
                return Mathf.SmoothStep(0f, squash, progress / 0.2f);
            }

            if (progress <= 0.6f)
            {
                float rebound = Mathf.SmoothStep(0f, 1f, (progress - 0.2f) / 0.4f);
                return Mathf.Lerp(squash, -squash * 0.15f, rebound);
            }

            return Mathf.Lerp(-squash * 0.15f, 0f, Mathf.SmoothStep(0f, 1f, (progress - 0.6f) / 0.4f));
        }

        /// <summary>Calculates a smooth midpoint deformation for a temporary body pose.</summary>
        /// <param name="normalizedTime">Pose progress from zero to one.</param>
        /// <param name="amount">Maximum proportional deformation.</param>
        /// <returns>The current proportional deformation.</returns>
        private static float GetPoseDeformation(float normalizedTime, float amount)
        {
            return Mathf.Sin(Mathf.Clamp01(normalizedTime) * Mathf.PI) * Mathf.Clamp01(amount);
        }

        /// <summary>Returns the project body scale when an invalid zero scale is supplied.</summary>
        /// <param name="scale">Scale to validate.</param>
        /// <returns>A non-zero body scale.</returns>
        private static Vector3 GetBodyScaleOrDefault(Vector3 scale)
        {
            return scale.sqrMagnitude > 0.000001f ? scale : new Vector3(1.8f, 1.8f, 1f);
        }

        /// <summary>Returns the expected body scale when the editor has left a zero scale behind.</summary>
        /// <param name="scale">Scale currently stored on the body visual.</param>
        /// <returns>A valid body scale for the BIT prefab.</returns>
        private static Vector3 GetBodyBaseScale(Vector3 scale)
        {
            return GetBodyScaleOrDefault(scale);
        }
    }
}

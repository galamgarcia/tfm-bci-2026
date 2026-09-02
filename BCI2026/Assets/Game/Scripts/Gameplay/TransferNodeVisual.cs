/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;

namespace Bit.Gameplay
{
    /// <summary>Displays the segmented cyan system silhouette of a Transfer Node.</summary>
    public sealed class TransferNodeVisual : MonoBehaviour
    {
        [Header("Outline")]
        [Tooltip("Renderer segments ordered from least to most complete.")]
        [SerializeField] private Renderer[] outlineSegments;

        [Tooltip("Cyan data fragments around the node.")]
        [SerializeField] private ParticleSystem dataFragments;

        [Header("Visual Timing")]
        [Tooltip("Emission strength used by the idle system pulse.")]
        [SerializeField, Min(0f)] private float idleEmission = 0.35f;

        [Tooltip("Emission strength used while synchronizing Bit.")]
        [SerializeField, Min(0f)] private float synchronizingEmission = 0.8f;

        [Tooltip("Emission strength used by the brief completion confirmation.")]
        [SerializeField, Min(0f)] private float completeEmission = 1.5f;

        // Current activation progress shown by the node.
        private float _progress;
        // Whether the node is currently synchronizing.
        private bool _isSynchronizing;
        // Whether the node has reached its complete visual state.
        private bool _isComplete;
        // Whether the node is currently animating its completion.
        private bool _isCompleting;
        // First segment used by the current completion sequence.
        private int _completionStart;
        // Reusable property blocks that avoid instantiating segment materials.
        private MaterialPropertyBlock _propertyBlock;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            SetIdle();
        }

        private void Update()
        {
            if (!_isCompleting && !_isComplete)
            {
                AnimateIdle();
            }

            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 0.7f);
            float baseEmission = _isComplete || _isCompleting ? completeEmission : _isSynchronizing ? synchronizingEmission : idleEmission;
            SetEmission(baseEmission * Mathf.Lerp(0.85f, 1.15f, pulse));
        }

        /// <summary>Returns the visual to its incomplete idle state.</summary>
        public void SetIdle()
        {
            _progress = 0f;
            _isSynchronizing = false;
            _isComplete = false;
            _isCompleting = false;
            _completionStart = 0;
            AnimateIdle();
            StopFragments();
        }

        /// <summary>Starts the synchronization visual at zero progress.</summary>
        public void BeginSynchronizing()
        {
            _progress = 0f;
            _isSynchronizing = true;
            _isComplete = false;
            _isCompleting = false;
            _completionStart = 0;
            SetVisibleSegments(0);
            StopFragments();
        }

        /// <summary>Shows normalized synchronization progress on the outline.</summary>
        /// <param name="progress">Progress from zero to one.</param>
        public void SetProgress(float progress)
        {
            _progress = Mathf.Clamp01(progress);
            SetVisibleSegments(Mathf.CeilToInt(outlineSegments.Length * _progress));
            if (dataFragments != null && _progress > 0f && !dataFragments.isPlaying)
            {
                dataFragments.Play();
            }
        }

        /// <summary>Shows the complete outline and emits restrained confirmation fragments.</summary>
        public void SetComplete()
        {
            _progress = 1f;
            _isSynchronizing = false;
            _isCompleting = false;
            _isComplete = true;
            SetVisibleSegments(outlineSegments.Length);
            if (dataFragments != null)
            {
                dataFragments.Emit(4);
            }
        }

        /// <summary>Starts the completion animation after the player reaches the node center.</summary>
        public void BeginCompleting()
        {
            _isSynchronizing = false;
            _isCompleting = true;
            _completionStart = GetIdleStart();
            _progress = 3f / 8f;
            SetVisibleSegments(3);
        }

        /// <summary>Activates the requested number of outline segments.</summary>
        /// <param name="count">Number of segments to activate in visual order.</param>
        private void SetVisibleSegments(int count)
        {
            for (int i = 0; i < outlineSegments.Length; i++)
            {
                if (outlineSegments[i] != null)
                {
                    int distance = (i - _completionStart + outlineSegments.Length) % outlineSegments.Length;
                    outlineSegments[i].gameObject.SetActive(distance < count);
                }
            }
        }

        /// <summary>Moves a short discontinuous signal around the outline while idle.</summary>
        private void AnimateIdle()
        {
            if (outlineSegments.Length == 0) { return; }

            int start = GetIdleStart();
            for (int i = 0; i < outlineSegments.Length; i++)
            {
                if (outlineSegments[i] == null) { continue; }
                int distance = (i - start + outlineSegments.Length) % outlineSegments.Length;
                outlineSegments[i].gameObject.SetActive(distance < 3);
            }
        }

        /// <summary>Gets the current first segment of the idle signal.</summary>
        /// <returns>The current idle segment index.</returns>
        private int GetIdleStart()
        {
            return Mathf.FloorToInt(Time.unscaledTime * 1.6f) % outlineSegments.Length;
        }

        /// <summary>Applies the current emission strength to every outline renderer.</summary>
        /// <param name="value">Emission strength to apply.</param>
        private void SetEmission(float value)
        {
            if (_propertyBlock == null) { return; }
            foreach (Renderer segment in outlineSegments)
            {
                if (segment == null) { continue; }
                segment.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetFloat("_EmissionStrength", value);
                _propertyBlock.SetFloat("_GlowIntensity", value * 1.5f);
                segment.SetPropertyBlock(_propertyBlock);
            }
        }

        /// <summary>Stops and clears optional data fragments.</summary>
        private void StopFragments()
        {
            if (dataFragments != null)
            {
                dataFragments.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}

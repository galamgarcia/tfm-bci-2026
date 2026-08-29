/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;

namespace Bit.Gameplay
{
    /// <summary>Controls procedural visual effects driven by Bit's gameplay state.</summary>
    [RequireComponent(typeof(ParticleSystem))]
    [ExecuteAlways]
    public sealed class BitVfxManager : MonoBehaviour
    {
        [Header("Relaxation bubbles")]
        [Tooltip("Material used by the geometric relaxation bubbles.")]
        [SerializeField] private Material relaxationBubbleMaterial;

        [Tooltip("Maximum bubble emission rate at full relaxation.")]
        [SerializeField, Min(0f)] private float maximumRelaxationEmission = 4f;

        [Tooltip("Current relaxation intensity from zero to one.")]
        [SerializeField, Range(0f, 1f)] private float relaxationIntensity;

        // Particle system used for relaxation bubbles.
        private ParticleSystem _relaxationBubbles;
        // Particle renderer used to assign the procedural bubble material.
        private ParticleSystemRenderer _bubbleRenderer;
        // Whether the particle system has received its relaxation configuration.
        private bool _isConfigured;
        // Reusable particle buffer used for individual floating motion.
        private ParticleSystem.Particle[] _particles;

        private void Awake()
        {
            EnsureBubbles();
            SetRelaxationIntensity(relaxationIntensity);
        }

        private void Update()
        {
            if (Application.isPlaying || _relaxationBubbles == null || relaxationIntensity <= 0f)
            {
                return;
            }

            _relaxationBubbles.Simulate(1f / 60f, false, false);
            UpdateParticleMotion();
        }

        /// <summary>Sets the normalized relaxation intensity driving bubble emission.</summary>
        /// <param name="intensity">Relaxation value in the range from zero to one.</param>
        public void SetRelaxationIntensity(float intensity)
        {
            relaxationIntensity = Mathf.Clamp01(intensity);
            if (!EnsureBubbles())
            {
                return;
            }

            var emission = _relaxationBubbles.emission;
            emission.rateOverTime = relaxationIntensity * maximumRelaxationEmission;
            if (relaxationIntensity > 0f && !_relaxationBubbles.isPlaying)
            {
                _relaxationBubbles.Play();
            }

            if (relaxationIntensity > 0f && !Application.isPlaying)
            {
                _relaxationBubbles.Emit(Mathf.Max(1, Mathf.RoundToInt(relaxationIntensity * 4f)));
                _relaxationBubbles.Simulate(1f / 60f, false, false);
                UpdateParticleMotion();
            }
            else if (relaxationIntensity <= 0f && _relaxationBubbles.isPlaying)
            {
                _relaxationBubbles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        /// <summary>Stops relaxation bubbles and clears their particles.</summary>
        public void StopRelaxationVfx()
        {
            relaxationIntensity = 0f;
            if (_relaxationBubbles == null)
            {
                return;
            }

            _relaxationBubbles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _relaxationBubbles.Clear(true);
        }

        /// <summary>Configures slow cyan geometric bubbles around Bit.</summary>
        private void ConfigureBubbles()
        {
            var main = _relaxationBubbles.main;
            main.loop = true;
            main.playOnAwake = false;
            main.maxParticles = 100;
            main.startLifetime = new ParticleSystem.MinMaxCurve(3.5f, 5f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.06f);
            main.startColor = new Color(1f, 1f, 1f, 0.75f);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            var emission = _relaxationBubbles.emission;
            emission.rateOverTime = 0f;

            var shape = _relaxationBubbles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(1.9f, 1.9f, 0f);

            var velocity = _relaxationBubbles.velocityOverLifetime;
            velocity.enabled = false;

            var size = _relaxationBubbles.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.15f),
                new Keyframe(0.15f, 1f),
                new Keyframe(0.8f, 0.8f),
                new Keyframe(1f, 0f)));

            var color = _relaxationBubbles.colorOverLifetime;
            color.enabled = true;
            color.color = new ParticleSystem.MinMaxGradient(new Gradient
            {
                colorKeys = new[]
                {
                    new GradientColorKey(new Color(0.3f, 0.9f, 1f), 0f),
                    new GradientColorKey(new Color(0.1f, 0.7f, 1f), 1f)
                },
                alphaKeys = new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.65f, 0.15f),
                    new GradientAlphaKey(0.45f, 0.8f),
                    new GradientAlphaKey(0f, 1f)
                }
            });

            var rotation = _relaxationBubbles.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-8f, 8f);

            var renderer = _bubbleRenderer;
            renderer.enabled = true;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = -1;
            renderer.material = relaxationBubbleMaterial;
        }

        /// <summary>Ensures the required particle system exists and is configured.</summary>
        /// <returns>True when the relaxation bubble system is ready.</returns>
        private bool EnsureBubbles()
        {
            if (_relaxationBubbles == null)
            {
                _relaxationBubbles = GetComponent<ParticleSystem>();
                if (_relaxationBubbles == null)
                {
                    _relaxationBubbles = gameObject.AddComponent<ParticleSystem>();
                }
            }

            if (_bubbleRenderer == null)
            {
                _bubbleRenderer = GetComponent<ParticleSystemRenderer>();
                if (_bubbleRenderer == null)
                {
                    _bubbleRenderer = gameObject.AddComponent<ParticleSystemRenderer>();
                }
            }

            if (!_isConfigured && _bubbleRenderer != null)
            {
                ConfigureBubbles();
                _isConfigured = true;
            }

            return _bubbleRenderer != null;
        }

        /// <summary>Updates each bubble with a slow asynchronous floating trajectory.</summary>
        private void UpdateParticleMotion()
        {
            int count = _relaxationBubbles.particleCount;
            if (_particles == null || _particles.Length < count)
            {
                _particles = ResizeParticleBuffer(Mathf.Max(1, count));
            }

            count = _relaxationBubbles.GetParticles(_particles);
            for (int i = 0; i < count; i++)
            {
                ParticleSystem.Particle particle = _particles[i];
                float phase = particle.randomSeed * 0.000001f;
                float age = particle.startLifetime - particle.remainingLifetime;
                float speed = Mathf.Lerp(0.018f, 0.035f, Mathf.Abs(Mathf.Sin(phase * 3.1f)));
                float vertical = speed * (0.62f + 0.55f * Mathf.Sin(age * 0.55f + phase));
                float horizontal = speed * 0.3f * Mathf.Sin(age * 0.8f + phase * 1.7f);
                particle.velocity = new Vector3(horizontal, vertical, 0f);
                _particles[i] = particle;
            }

            _relaxationBubbles.SetParticles(_particles, count);
        }

        /// <summary>Creates a larger reusable particle buffer when the system needs it.</summary>
        /// <param name="count">Current particle count.</param>
        /// <returns>A buffer sized for the current particle count.</returns>
        private static ParticleSystem.Particle[] ResizeParticleBuffer(int count)
        {
            return new ParticleSystem.Particle[Mathf.NextPowerOfTwo(Mathf.Max(128, count * 2))];
        }
    }
}

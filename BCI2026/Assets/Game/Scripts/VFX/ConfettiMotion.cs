/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System.Collections.Generic;
using UnityEngine;

namespace Bit.UI
{
    /// <summary>Animates UI confetti units.</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class ConfettiMotion : MonoBehaviour
    {
        [Header("Play")]
        [Tooltip("Plays the confetti when the object is enabled.")]
        [SerializeField] private bool playOnEnable = true;
        [Tooltip("Keeps creating confetti.")]
        [SerializeField] private bool loop;
        [Tooltip("Number of confetti units created each time.")]
        [SerializeField] private int confettiTotal = 50;

        [Header("Emission")]
        [Tooltip("Minimum time between groups of confetti.")]
        [SerializeField] private float minSpawnTime = 0.5f;
        [Tooltip("Maximum time between groups of confetti.")]
        [SerializeField] private float maxSpawnTime = 0.8f;
        [Tooltip("Time that confetti is created when loop is disabled.")]
        [SerializeField] private float playTime = 8f;

        [Header("Spawn")]
        [Tooltip("Height where the confetti starts.")]
        [SerializeField] private float startHeight = 120f;
        [Tooltip("Minimum fall speed.")]
        [SerializeField] private float minFallSpeed = 140f;
        [Tooltip("Maximum fall speed.")]
        [SerializeField] private float maxFallSpeed = 280f;
        [Tooltip("Maximum horizontal speed.")]
        [SerializeField] private float horizontalSpeed = 90f;
        [Tooltip("Gravity applied to the confetti.")]
        [SerializeField] private float gravity = 90f;

        [Header("Rotation")]
        [Tooltip("Maximum rotation speed.")]
        [SerializeField] private float rotationSpeed = 360f;

        // Inactive UI units used as visual templates for runtime clones.
        private RectTransform[] _templates;
        // Runtime units currently falling.
        private readonly List<ConfettiUnit> _units = new();
        // Random source for spawn variation and loop timing.
        private System.Random _random;
        // Remaining time before the next burst.
        private float _nextSpawnTime;
        // Remaining time in the current non-loop emission window.
        private float _remainingPlayTime;
        // Indicates if new bursts are currently being emitted.
        private bool _isPlaying;

        private RectTransform Rect => (RectTransform)transform;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_random != null && _templates != null) { return; }

            RectTransform[] transforms = GetComponentsInChildren<RectTransform>(true);
            _templates = new RectTransform[Mathf.Max(0, transforms.Length - 1)];
            _random = new System.Random();

            int index = 0;
            foreach (RectTransform child in transforms)
            {
                if (child == transform) { continue; }
                _templates[index++] = child;
                child.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (playOnEnable) { Play(); }
        }

        private void OnDisable()
        {
            ClearConfetti();
            _isPlaying = false;
        }

        private void Update()
        {
            float deltaTime = Time.unscaledDeltaTime;
            UpdateSpawn(deltaTime);
            UpdateConfettiUnits(deltaTime);
        }

        /// <summary>Starts the confetti animation.</summary>
        public void Play()
        {
            Initialize();
            if (_templates.Length == 0) { return; }
            if (!gameObject.activeSelf) { gameObject.SetActive(true); }
            ClearConfetti();
            _isPlaying = true;
            _nextSpawnTime = Mathf.Max(0.01f, GetRandomValue(minSpawnTime, maxSpawnTime));
            _remainingPlayTime = loop ? float.PositiveInfinity : playTime;
            CreateConfetti();
        }

        /// <summary>Stops emission, destroys active pieces and hides the confetti object.</summary>
        public void Hide()
        {
            ClearConfetti();
            _isPlaying = false;
            gameObject.SetActive(false);
        }

        /// <summary>Updates the creation of new confetti units.</summary>
        private void UpdateSpawn(float deltaTime)
        {
            if (!_isPlaying) { return; }
            if (!loop)
            {
                _remainingPlayTime -= deltaTime;

                if (_remainingPlayTime <= 0f)
                {
                    _isPlaying = false;
                    return;
                }
            }

            _nextSpawnTime -= deltaTime;

            if (_nextSpawnTime <= 0f)
            {
                CreateConfetti();
                _nextSpawnTime = Mathf.Max(0.01f, GetRandomValue(minSpawnTime, maxSpawnTime));
            }
        }

        /// <summary>Updates the movement of all confetti units.</summary>
        private void UpdateConfettiUnits(float deltaTime)
        {
            for (int i = _units.Count - 1; i >= 0; i--)
            {
                ConfettiUnit unit = _units[i];
                unit.velocity.y -= gravity * deltaTime;
                unit.transform.anchoredPosition += unit.velocity * deltaTime;
                unit.transform.Rotate(0f, 0f, unit.rotationSpeed * deltaTime);

                if (IsOutsideScreen(unit))
                {
                    Destroy(unit.transform.gameObject);
                    _units.RemoveAt(i);
                }
            }
        }

        /// <summary>Creates a group of confetti units.</summary>
        private void CreateConfetti()
        {
            Canvas.ForceUpdateCanvases();

            int count = Mathf.Max(0, confettiTotal);
            for (int i = 0; i < count; i++)
            {
                RectTransform template = _templates[i % _templates.Length];
                GameObject instance = Instantiate(template.gameObject, transform);
                instance.SetActive(true);

                RectTransform unitTransform = instance.GetComponent<RectTransform>();
                float x = GetRandomValue(Rect.rect.xMin, Rect.rect.xMax);
                float y = Rect.rect.yMax + startHeight + GetRandomValue(0f, startHeight);
                unitTransform.anchoredPosition = new Vector2(x, y);
                unitTransform.localRotation = Quaternion.Euler(0f, 0f, GetRandomValue(0f, 360f));

                float fallSpeed = GetRandomValue(minFallSpeed, maxFallSpeed);
                Vector2 velocity = new(GetRandomValue(-horizontalSpeed, horizontalSpeed), -fallSpeed);
                float unitRotationSpeed = GetRandomValue(-rotationSpeed, rotationSpeed);
                _units.Add(new ConfettiUnit(unitTransform, velocity, unitRotationSpeed, unitTransform.rect.width, unitTransform.rect.height));
            }
        }

        /// <summary>Checks if a confetti unit is outside the UI area.</summary>
        private bool IsOutsideScreen(ConfettiUnit unit)
        {
            Vector2 position = unit.transform.anchoredPosition;
            return position.y < Rect.rect.yMin - unit.height
                || position.x < Rect.rect.xMin - unit.width
                || position.x > Rect.rect.xMax + unit.width;
        }

        private float GetRandomValue(float min, float max)
        {
            return Mathf.Lerp(min, max, (float)_random.NextDouble());
        }

        /// <summary>Removes all active confetti units.</summary>
        private void ClearConfetti()
        {
            foreach (ConfettiUnit unit in _units)
            {
                if (unit.transform != null)
                {
                    Destroy(unit.transform.gameObject);
                }
            }

            _units.Clear();
        }

        private sealed class ConfettiUnit
        {
            public ConfettiUnit(RectTransform transform, Vector2 velocity, float rotationSpeed, float width, float height)
            {
                this.transform = transform;
                this.velocity = velocity;
                this.rotationSpeed = rotationSpeed;
                this.width = width;
                this.height = height;
            }

            // Runtime UI transform.
            public readonly RectTransform transform;
            // Current movement velocity.
            public Vector2 velocity;
            // Current rotation velocity.
            public readonly float rotationSpeed;
            // Width used for exit detection.
            public readonly float width;
            // Height used for exit detection.
            public readonly float height;
        }
    }
}

/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;

namespace Bit.Gameplay
{
    /// <summary>Materializes one reusable platform according to Bit's stabilized concentration state.</summary>
    [RequireComponent(typeof(BoxCollider))]
    public sealed class MentalPlatform : AnchoredSurface
    {
        [Header("Input")]
        [Tooltip("Bit controller that publishes the stabilized concentration state.")]
        [SerializeField] private BitController bitSource;

        [Header("Components")]
        [Tooltip("Renderer used for the platform interior fill.")]
        [SerializeField] private Renderer fillRenderer;
        [Tooltip("Renderers used for the persistent neon outline, ordered top, bottom, left, right.")]
        [SerializeField] private Renderer[] outlineRenderers;
        [Tooltip("Existing 3D collider enabled while the platform is active.")]
        [SerializeField] private BoxCollider platformCollider;

        [Header("Graphic Settings")]
        [Tooltip("Visual thickness of the neon outline.")]
        [SerializeField, Min(0.001f)] private float outline = 0.02f;
        [Tooltip("Color used by Focus platforms for the fill and neon outline.")]
        [SerializeField] private Color focusColor = new Color32(255, 49, 88, 255);
        [Tooltip("Color used by Unfocus platforms for the fill and neon outline.")]
        [SerializeField] private Color unfocusColor = new Color32(255, 217, 40, 255);
        [Tooltip("Seconds used to materialize or dematerialize the fill.")]
        [SerializeField, Min(0f)] private float transitionDuration = 0.2f;

        [Header("Mental State")]
        [Tooltip("Requires concentration when enabled; otherwise activates in the unfocused state.")]
        [SerializeField] private bool requiresFocus = true;
        
        // Current materialization amount used by the shader.
        private float _activeAmount;
        // Target materialization amount requested by the mental state.
        private float _targetAmount;
        // Neon color selected by the platform configuration.
        private Color _color;
        // Reusable property block for the fill renderer.
        private MaterialPropertyBlock _fillProperties;
        // Reusable property block for the outline renderers.
        private MaterialPropertyBlock _outlineProperties;

        private void Awake()
        {
            if (Application.isPlaying && bitSource == null)
            {
                bitSource = FindFirstObjectByType<BitController>();
            }
            platformCollider ??= GetComponent<BoxCollider>();
            NormalizeDimensions();
            ApplyGeometry();
        }

        private void OnEnable()
        {
            if (bitSource != null)
            {
                bitSource.OnConcentrationChanged += OnConcentrationChanged;
                ApplyState(bitSource.GetConcentrationLevel(), true);
            }
            else
            {
                ApplyState(MentalStateLevel.None, true);
            }
            ApplyColor();
        }

        private void OnDisable()
        {
            if (bitSource != null)
            {
                bitSource.OnConcentrationChanged -= OnConcentrationChanged;
            }
        }

        /// <summary>Applies the current mental level to the platform.</summary>
        /// <param name="level">Confirmed concentration level.</param>
        private void OnConcentrationChanged(MentalStateLevel level)
        {
            ApplyState(level, false);
        }

        /// <summary>Updates the collider immediately and the visuals through their transition.</summary>
        /// <param name="level">Current stabilized concentration level.</param>
        /// <param name="isInitialState">Whether this is initial setup.</param>
        private void ApplyState(MentalStateLevel level, bool isInitialState)
        {
            bool isActive = IsActive(level);
            if (platformCollider != null)
            {
                platformCollider.enabled = true;
                platformCollider.isTrigger = !isActive;
            }
            if (isInitialState)
            {
                _activeAmount = isActive ? 1f : 0f;
                _targetAmount = _activeAmount;
                ApplyGeometry();
                ApplyVisuals();
                return;
            }

            _targetAmount = isActive ? 1f : 0f;
            if (transitionDuration <= 0f)
            {
                _activeAmount = _targetAmount;
                ApplyGeometry();
                ApplyVisuals();
            }
        }

        private void Update()
        {
            if (Mathf.Approximately(_activeAmount, _targetAmount)) { return; }
            if (transitionDuration <= 0f) { return; }
            float step = Time.deltaTime / transitionDuration;
            _activeAmount = Mathf.MoveTowards(_activeAmount, _targetAmount, step);
            ApplyVisuals();
        }

        private void OnValidate()
        {
            NormalizeDimensions();
            outline = Mathf.Max(0.001f, outline);
            transitionDuration = Mathf.Max(0f, transitionDuration);
            ApplyGeometry();
            ApplyColor();
        }

        /// <summary>Applies the anchored rectangular geometry to the fill and outline.</summary>
        private void ApplyGeometry()
        {
            Vector2 size = GetSize();
            float depth = GetDepth();

            if (fillRenderer != null)
            {
                fillRenderer.transform.localPosition = new Vector3(size.x * 0.5f, -size.y * 0.5f, 0f);
                fillRenderer.transform.localScale = new Vector3(size.x, size.y, depth);
            }

            if (outlineRenderers == null || outlineRenderers.Length < 4) { return; }
            float edgeZ = -(depth * 0.5f + 0.001f);
            SetOutlineGeometry(outlineRenderers[0], new Vector3(size.x * 0.5f, -outline * 0.5f, edgeZ), new Vector3(size.x, outline, 0.01f));
            SetOutlineGeometry(outlineRenderers[1], new Vector3(size.x * 0.5f, -size.y + outline * 0.5f, edgeZ), new Vector3(size.x, outline, 0.01f));
            SetOutlineGeometry(outlineRenderers[2], new Vector3(outline * 0.5f, -size.y * 0.5f, edgeZ), new Vector3(outline, size.y, 0.01f));
            SetOutlineGeometry(outlineRenderers[3], new Vector3(size.x - outline * 0.5f, -size.y * 0.5f, edgeZ), new Vector3(outline, size.y, 0.01f));
            ApplyColliderGeometry(platformCollider);
        }

        /// <summary>Applies one outline segment's local geometry.</summary>
        /// <param name="renderer">Outline renderer to resize.</param>
        /// <param name="position">Local position of the segment.</param>
        /// <param name="scale">Local scale of the segment.</param>
        private static void SetOutlineGeometry(Renderer renderer, Vector3 position, Vector3 scale)
        {
            if (renderer == null) { return; }
            renderer.gameObject.SetActive(scale.x > 0f && scale.y > 0f);
            renderer.transform.localPosition = position;
            renderer.transform.localScale = scale;
        }

        /// <summary>Applies the configured Focus or Unfocus color to all visual renderers.</summary>
        private void ApplyColor()
        {
            _color = requiresFocus ? focusColor : unfocusColor;
            ApplyVisuals();
        }

        /// <summary>Determines whether this platform is active for a concentration level.</summary>
        /// <param name="level">Current stabilized concentration level.</param>
        /// <returns>True, if the platform should be solid and filled.</returns>
        private bool IsActive(MentalStateLevel level)
        {
            bool isFocused = (level == MentalStateLevel.High);
            return requiresFocus == isFocused;
        }

        /// <summary>Publishes materialization and outline values to the shared shader.</summary>
        private void ApplyVisuals()
        {
            _fillProperties ??= new MaterialPropertyBlock();
            _outlineProperties ??= new MaterialPropertyBlock();
            SetProperties(fillRenderer, _fillProperties, _activeAmount, false, 0f);
            if (outlineRenderers == null) { return; }

            Vector2 size = GetSize();
            for (int i = 0; i < outlineRenderers.Length; i++)
            {
                float sideLength = i < 2 ? size.x : size.y;
                SetProperties(outlineRenderers[i], _outlineProperties, _activeAmount, true, i < 2 ? 0f : 1f, GetDashFrequency(sideLength));
            }
        }

        /// <summary>Calculates a dash frequency that keeps dash lengths stable across platform sizes.</summary>
        /// <param name="sideLength">Length of the outline side in world units.</param>
        /// <returns>The shader frequency for the selected side.</returns>
        private static float GetDashFrequency(float sideLength)
        {
            const float referenceLength = 2f;
            const float referenceFrequency = 12f;
            return Mathf.Max(1f, referenceFrequency * sideLength / referenceLength);
        }

        /// <summary>Sets shared shader properties for one renderer.</summary>
        /// <param name="renderer">Renderer receiving the values.</param>
        /// <param name="properties">Reusable property block.</param>
        /// <param name="activeAmount">Materialization amount in the range [0,1].</param>
        /// <param name="isOutline">Whether the renderer represents the neon outline.</param>
        /// <param name="dashAxis">Local axis used for outline dashes.</param>
        private void SetProperties(Renderer renderer, MaterialPropertyBlock properties, float activeAmount, bool isOutline, float dashAxis, float dashFrequency = 12f)
        {
            if (renderer == null) { return; }
            renderer.GetPropertyBlock(properties);
            properties.SetColor("_BaseColor", _color);
            properties.SetColor("_EmissionColor", _color);
            properties.SetFloat("_GlowIntensity", isOutline ? 1.2f : 0f);
            properties.SetFloat("_ActiveAmount", activeAmount);
            properties.SetFloat("_IsOutline", isOutline ? 1f : 0f);
            properties.SetFloat("_DashAxis", dashAxis);
            properties.SetFloat("_DashFrequency", dashFrequency);
            properties.SetFloat("_DashGap", 0.45f);
            renderer.SetPropertyBlock(properties);
        }
    }
}

/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;
using UnityEngine.UI;

namespace Bit.UI
{
    /// <summary>Provides position and color operations for an image in a canvas.</summary>
    [RequireComponent(typeof(RectTransform), typeof(Image))]
    public abstract class CanvasImage : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("UI transform used to position this image in canvas space.")]
        [SerializeField] private RectTransform rectTransform;
        [Tooltip("Image used to display this canvas object.")]
        [SerializeField] private Image image;

        // Minimum position allowed for horizontal movement.
        private Vector2 _minPosition;

        // Maximum position allowed for horizontal movement.
        private Vector2 _maxPosition;

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            image = GetComponent<Image>();
        }

        /// <summary>Gets the image position in its parent canvas space.</summary>
        /// <returns>The current image position.</returns>
        public Vector2 GetPosition()
        {
            return rectTransform.anchoredPosition;
        }

        /// <summary>Sets the image position in its parent canvas space.</summary>
        /// <param name="position">New image position.</param>
        public void SetPosition(Vector2 position)
        {
            rectTransform.anchoredPosition = position;
        }

        /// <summary>Configures the horizontal movement bounds.</summary>
        /// <param name="min">Minimum allowed position.</param>
        /// <param name="max">Maximum allowed position.</param>
        public virtual void Configure(Vector2 min, Vector2 max)
        {
            _minPosition = min;
            _maxPosition = max;
        }

        /// <summary>Gets a horizontal position constrained to the configured bounds.</summary>
        /// <param name="position">Current horizontal position.</param>
        /// <param name="delta">Horizontal displacement to apply.</param>
        /// <returns>The horizontal position within the configured bounds.</returns>
        protected float GetBoundedHorizontal(float position, float delta)
        {
            return Mathf.Clamp(position + delta, _minPosition.x, _maxPosition.x);
        }

        /// <summary>Applies a color to the image.</summary>
        /// <param name="color">Color applied to the image.</param>
        protected void SetColor(Color color)
        {
            image.color = color;
        }
    }
}

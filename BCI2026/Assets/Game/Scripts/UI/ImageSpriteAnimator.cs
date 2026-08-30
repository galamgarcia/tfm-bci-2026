/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;
using UnityEngine.UI;

namespace Bit.UI
{
    /// <summary>Animates a UI image through a serialized sprite sequence.</summary>
    [RequireComponent(typeof(Image))]
    public sealed class ImageSpriteAnimator : MonoBehaviour
    {
        [Header("Animation")]
        [Tooltip("Image that displays the animation frames.")]
        [SerializeField] private Image targetImage;

        [Header("Animation")]
        [Tooltip("Frames displayed in order by the animation.")]
        [SerializeField] private Sprite[] frames;

        [Header("Animation")]
        [Tooltip("Number of frames displayed per second.")]
        [SerializeField] private float framesPerSecond = 12f;

        [Header("Animation")]
        [Tooltip("Whether the animation starts again after its last frame.")]
        [SerializeField] private bool isLooping = true;

        // Elapsed unscaled time since the current frame started.
        private float _elapsed;
        // Current frame index.
        private int _frameIndex;

        private void Awake()
        {
            if (targetImage == null)
            {
                targetImage = GetComponent<Image>();
            }

            ApplyFrame();
        }

        private void Update()
        {
            if (targetImage == null || frames == null || frames.Length < 2 || framesPerSecond <= 0f) { return; }

            _elapsed += Time.unscaledDeltaTime;
            float frameDuration = 1f / framesPerSecond;
            if (_elapsed < frameDuration) { return; }

            _elapsed -= frameDuration;
            _frameIndex++;
            if (_frameIndex >= frames.Length)
            {
                if (!isLooping)
                {
                    _frameIndex = frames.Length - 1;
                    enabled = false;
                    ApplyFrame();
                    return;
                }

                _frameIndex = 0;
            }

            ApplyFrame();
        }

        /// <summary>Applies the current serialized frame to the target image.</summary>
        private void ApplyFrame()
        {
            if (targetImage != null && frames != null && frames.Length > 0)
            {
                targetImage.sprite = frames[Mathf.Clamp(_frameIndex, 0, frames.Length - 1)];
            }
        }
    }
}

/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bit.Gameplay
{
    /// <summary>Plays a brief cyan screen-space glitch using configured digital strips.</summary>
    public sealed class ScreenGlitch : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Canvas group controlling the full-screen glitch overlay.")]
        [SerializeField] private CanvasGroup canvasGroup;
        [Tooltip("Rectangular digital strips used by the glitch overlay.")]
        [SerializeField] private Image[] strips;

        [Header("Timing")]
        [Tooltip("Duration in unscaled seconds of the glitch.")]
        [SerializeField, Min(0.01f)] private float duration = 0.18f;
        [Tooltip("Maximum alpha of the cyan glitch strips.")]
        [SerializeField, Range(0f, 1f)] private float maximumAlpha = 0.8f;

        [Header("Color")]
        [Tooltip("Color used when no alternate glitch colors are configured.")]
        [SerializeField] private Color glitchColor = new Color(0.1f, 0.8f, 1f, 1f);
        [Tooltip("Optional colors randomly selected for each glitch strip.")]
        [SerializeField] private Color[] glitchColors;

        // Prevents overlapping glitch sequences.
        private bool _isPlaying;

        private void Awake()
        {
            SetVisible(false);
        }

        /// <summary>Plays one brief cyan glitch and waits until it finishes.</summary>
        /// <returns>An enumerator for the unscaled glitch sequence.</returns>
        public IEnumerator Play()
        {
            if (_isPlaying) { yield break; }
            _isPlaying = true;
            SetVisible(true);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float amount = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
                if (canvasGroup != null) { canvasGroup.alpha = Mathf.Lerp(maximumAlpha, 0f, amount); }
                if (strips != null)
                {
                    for (int i = 0; i < strips.Length; i++)
                    {
                        if (strips[i] == null) { continue; }
                        RectTransform rect = strips[i].rectTransform;
                        float y = Random.value;
                        float height = Random.Range(0.02f, 0.08f);
                        rect.anchorMin = new Vector2(0f, y);
                        rect.anchorMax = new Vector2(1f, Mathf.Min(1f, y + height));
                        Color color = GetColor();
                        color.a = Random.Range(0.2f, 1f);
                        strips[i].color = color;
                    }
                }
                yield return null;
            }

            SetVisible(false);
            _isPlaying = false;
        }

        /// <summary>Gets a random glitch color.</summary>
        /// <returns>A color from the glitch colors, or the default color if none are available.</returns>
        private Color GetColor()
        {
            if (glitchColors != null && glitchColors.Length > 0)
            {
                return glitchColors[Random.Range(0, glitchColors.Length)];
            }

            return glitchColor;
        }

        private void SetVisible(bool isVisible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = isVisible ? maximumAlpha : 0f;
            }

            if (strips == null) { return; }
            for (int i = 0; i < strips.Length; i++)
            {
                if (strips[i] == null) { continue; }
                strips[i].gameObject.SetActive(true);
                strips[i].enabled = true;
            }
        }
    }
}

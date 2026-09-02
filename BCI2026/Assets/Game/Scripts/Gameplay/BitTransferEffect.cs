/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System.Collections;
using UnityEngine;

namespace Bit.Gameplay
{
    /// <summary>Plays the restrained digital dissolve used when Bit leaves a level.</summary>
    public sealed class BitTransferEffect : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Visual hierarchy faded during the transfer without affecting physics.")]
        [SerializeField] private Transform visualRoot;

        [Tooltip("Sprite renderers that make up Bit's visual body and eyes.")]
        [SerializeField] private SpriteRenderer[] renderers;

        [Tooltip("Cyan fragments emitted as Bit is transferred.")]
        [SerializeField] private ParticleSystem fragments;

        [Header("Transfer")]
        [Tooltip("Duration in seconds of the digital dissolve.")]
        [SerializeField, Min(0.05f)] private float duration = 0.35f;

        // Prevents two transfer effects from starting concurrently.
        private bool _isPlaying;
        // Original renderer colors restored if the object is reused.
        private Color[] _originalColors;

        private void Awake()
        {
            _originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null) { _originalColors[i] = renderers[i].color; }
            }
        }

        /// <summary>Starts the one-shot digital transfer effect.</summary>
        public void PlayTransferOut()
        {
            if (_isPlaying) { return; }
            StartCoroutine(TransferOut());
        }

        private IEnumerator TransferOut()
        {
            _isPlaying = true;
            if (fragments != null)
            {
                fragments.Emit(8);
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float amount = 1f - Mathf.Clamp01(elapsed / duration);
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] == null) { continue; }
                    Color color = _originalColors[i];
                    color.a *= amount;
                    renderers[i].color = color;
                }

                if (visualRoot != null)
                {
                    visualRoot.localScale = Vector3.one * Mathf.Lerp(0.92f, 1f, amount);
                }

                yield return null;
            }

            if (visualRoot != null)
            {
                visualRoot.gameObject.SetActive(false);
            }
            _isPlaying = false;
        }
    }
}

/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System;
using Bit.Gameplay;
using UnityEngine;

namespace Bit.UI
{
    /// <summary>Defines the shared metadata and fade group for one tutorial screen prefab.</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class TutorialScreen : MonoBehaviour
    {
        [Header("Screen")]
        [Tooltip("ID screen")]
        [SerializeField] private TutorialScreenType screenType;
        [Header("Screen")]
        [Tooltip("Canvas group used by the tutorial flow to fade this screen and enable or block its interaction.")]
        [SerializeField] private CanvasGroup canvasGroup;

        // Gets the CanvasGroup used to fade and block interaction for this screen.
        public CanvasGroup CanvasGroup => canvasGroup;
        // Gets the functional tutorial step represented by this screen.
        public TutorialScreenType ScreenType => screenType;
        // Gets the delay shown after this screen completes before advancing.
        public virtual float CompletionDelay => 0f;
        // Triggered when this screen completes its own interaction.
        public event Action OnComplete;

        /// <summary>Starts this screen's specialized interaction lifecycle.</summary>
        public virtual void Activate() { }

        /// <summary>Stops this screen's specialized interaction lifecycle.</summary>
        public virtual void Deactivate() { }

        /// <summary>Notifies the tutorial flow that this screen has completed.</summary>
        protected virtual void Complete()
        {
            OnComplete?.Invoke();
        }

        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }
}

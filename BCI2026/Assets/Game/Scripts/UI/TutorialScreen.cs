/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;

namespace BciGame.UI
{
    /// <summary>
    /// Defines the shared metadata and fade group for one tutorial screen prefab.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class TutorialScreen : MonoBehaviour
    {
        [Header("Screen")]
        [Tooltip("Tutorial step represented by this screen.")]
        [SerializeField] private TutorialScreenType screenType;
        [SerializeField] private CanvasGroup canvasGroup;

        // Gets the CanvasGroup used to fade and block interaction for this screen.
        public CanvasGroup CanvasGroup => canvasGroup;
        // Gets the functional tutorial step represented by this screen.
        public TutorialScreenType ScreenType => screenType;

        /// <summary>
        /// Assigns the required CanvasGroup when the component is added or reset in the Inspector.
        /// </summary>
        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }
}

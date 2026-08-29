/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using BciGame.Input;
using UnityEngine;
using UnityEngine.UI;

namespace BciGame.UI
{
    /// <summary>
    /// Invokes a UI button from a touch click or a detected head nod.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class NodButton : MonoBehaviour
    {
        [Header("Activation")]
        [Tooltip("Indicates if this button can only be activated once.")]
        [SerializeField] private bool isSingleUse = true;
        [Tooltip("Unity button invoked after a valid activation.")]
        [SerializeField] private Button button;
        // Head-tracking service used to trigger the button from a nod gesture.
        private HeadPoseTracker _headPoseTracker;

        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (isSingleUse)
            {
                button.onClick.AddListener(() => { button.interactable = false; });
            }

            _headPoseTracker = FindFirstObjectByType<HeadPoseTracker>();
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }

        private void OnEnable()
        {
            if (_headPoseTracker == null)
            {
                _headPoseTracker = FindFirstObjectByType<HeadPoseTracker>();
            }

            if (_headPoseTracker != null)
            {
                _headPoseTracker.NodDetected += OnNodClick;
                _headPoseTracker.BeginCalibration();
            }
        }

        private void OnDisable()
        {
            if (_headPoseTracker != null)
            {
                _headPoseTracker.NodDetected -= OnNodClick;
            }
        }

        /// <summary>Invokes the button after a detected nod when it remains interactable.</summary>
        private void OnNodClick()
        {
            if (!button.interactable) { return; }
            button.onClick.Invoke();
        }
    }
}

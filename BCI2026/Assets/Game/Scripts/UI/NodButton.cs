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
    [RequireComponent(typeof(Button))]
    public sealed class NodButton : MonoBehaviour
    {
        [SerializeField] private bool isSingleUse = true;
        [SerializeField] private Button button;
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

        private void OnNodClick()
        {
            if (!button.interactable) { return; }
            button.onClick.Invoke();
        }
    }
}

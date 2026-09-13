/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Bit.UI
{
    /// <summary>Connects a UI button to the active gameplay pause controller.</summary>
    [RequireComponent(typeof(Button))]
    public sealed class PauseButton : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Pause controller that receives the pause request.")]
        [SerializeField] private PauseMenuController pauseMenuController;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            GameStateController stateController = GameStateController.Instance;
            if (stateController == null || stateController.GetState() != GameState.Game)
            {
                gameObject.SetActive(false);
                return;
            }

            _button ??= GetComponent<Button>();
            _button.onClick.AddListener(RequestPause);
        }

        private void OnDisable()
        {
            _button?.onClick.RemoveListener(RequestPause);
        }

        /// <summary>Requests the active gameplay controller to open the pause menu.</summary>
        public void RequestPause()
        {
            pauseMenuController ??= FindFirstObjectByType<PauseMenuController>();
            pauseMenuController?.OpenPause();
        }
    }
}

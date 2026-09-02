/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.Gameplay;
using Bit.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bit.UI
{
    /// <summary>Provides nod navigation and blink confirmation for a selectable UI menu.</summary>
    public sealed class MenuInputController : MonoBehaviour
    {
        [Header("Input")]
        [Tooltip("Input component that publishes nod and blink events.")]
        [SerializeField] private InputController inputController;

        [Tooltip("Head tracker that publishes confirmed nod gestures.")]
        [SerializeField] private HeadPoseTracker headPoseTracker;

        [Tooltip("BrainLink source that publishes validated blink gestures.")]
        [SerializeField] private BrainLinkBlinkInputSource blinkInputSource;

        [Tooltip("Connection popup that blocks head gestures while it is visible.")]
        [SerializeField] private ConnectionPopup connectionPopup;

        [Header("Options")]
        [Tooltip("Buttons navigated in order by head nods.")]
        [SerializeField] private Button[] options;

        [Tooltip("Button selected when the menu becomes active.")]
        [SerializeField] private Button initialOption;

        // Current selected menu button.
        private Button _selectedOption;

        private void Awake()
        {
            if (inputController != null)
            {
                inputController.SetInputTracking(false, true, false, false, true);
            }
        }

        private void OnEnable()
        {
            if (inputController == null) { return; }
            inputController.OnNodDetected += OnNodDetected;
            inputController.OnBlinkDetected += OnBlinkDetected;
        }

        private void OnDisable()
        {
            if (inputController == null) { return; }
            inputController.OnNodDetected -= OnNodDetected;
            inputController.OnBlinkDetected -= OnBlinkDetected;
        }

        private void Start()
        {
            if (inputController != null)
            {
                inputController.ConfigureSources(headPoseTracker, null, blinkInputSource);
            }
            SelectOption(IsUsable(initialOption) ? initialOption : GetFirstUsableOption());
        }

        /// <summary>Moves selection to the next usable option after a nod.</summary>
        private void OnNodDetected()
        {
            if (IsBlocked()) { return; }
            Button current = GetSelectedOption();
            int index = GetOptionIndex(current);
            if (index < 0) { SelectOption(GetFirstUsableOption()); return; }

            for (int offset = 1; offset <= options.Length; offset++)
            {
                Button next = options[(index + offset) % options.Length];
                if (IsUsable(next))
                {
                    SelectOption(next);
                    return;
                }
            }
        }

        /// <summary>Confirms the selected option after a blink.</summary>
        private void OnBlinkDetected()
        {
            if (IsBlocked()) { return; }
            GetSelectedOption()?.onClick.Invoke();
        }

        /// <summary>Selects a usable option through Unity's EventSystem.</summary>
        /// <param name="option">Button to select.</param>
        private void SelectOption(Button option)
        {
            if (!IsUsable(option)) { return; }
            _selectedOption = option;
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(option.gameObject);
            }
        }

        /// <summary>Gets the currently selected usable menu option.</summary>
        /// <returns>The selected option or the cached option when Unity has no EventSystem selection.</returns>
        private Button GetSelectedOption()
        {
            GameObject selected = EventSystem.current == null ? null : EventSystem.current.currentSelectedGameObject;
            Button button = selected == null ? null : selected.GetComponent<Button>();
            return IsUsable(button) ? button : (IsUsable(_selectedOption) ? _selectedOption : GetFirstUsableOption());
        }

        /// <summary>Gets the first usable option from the configured order.</summary>
        /// <returns>The first interactable option, or null when none is available.</returns>
        private Button GetFirstUsableOption()
        {
            foreach (Button option in options)
            {
                if (IsUsable(option)) { return option; }
            }

            return null;
        }

        /// <summary>Gets an option's index in the configured navigation order.</summary>
        /// <param name="option">Option to locate.</param>
        /// <returns>The option index or -1 when it is not configured.</returns>
        private int GetOptionIndex(Button option)
        {
            for (int i = 0; i < options.Length; i++)
            {
                if (options[i] == option) { return i; }
            }

            return -1;
        }

        /// <summary>Indicates whether a button can be selected or confirmed.</summary>
        /// <param name="option">Button to inspect.</param>
        /// <returns>True when the button exists and is interactable.</returns>
        private static bool IsUsable(Button option)
        {
            return option != null && option.interactable && option.gameObject.activeInHierarchy;
        }

        /// <summary>Indicates whether the connection popup currently blocks input.</summary>
        /// <returns>True when menu gestures must be ignored.</returns>
        private bool IsBlocked()
        {
            return connectionPopup != null && connectionPopup.IsVisible();
        }
    }
}

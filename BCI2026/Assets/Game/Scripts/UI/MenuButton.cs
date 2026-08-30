/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bit.UI
{
    /// <summary>Provides the main menu button's cyan system selection state.</summary>
    [ExecuteAlways]
    public sealed class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        private static readonly Color ActiveColor = new(0.08f, 0.74f, 0.86f, 1f);
        private static readonly Color DisabledColor = new(0.28f, 0.39f, 0.43f, 1f);

        [Header("Visuals")]
        [Tooltip("Button label rendered with the standard Unity UI Text component.")]
        [SerializeField] private Text label;

        [Header("Visuals")]
        [Tooltip("Dashed frame shown while the pointer is over the button.")]
        [SerializeField] private GameObject selectionFrame;

        [Header("Visuals")]
        [Tooltip("Background image darkened while the button is hovered or focused.")]
        [SerializeField] private Image background;

        // Button component that owns the interactable state.
        private Button _button;
        // Indicates whether the pointer is currently over the button.
        private bool _isPointerOver;
        // Indicates whether Unity navigation currently selects the button.
        private bool _isSelected;

        private void Awake()
        {
            _button = GetComponent<Button>();
            RefreshVisualState();
        }

        private void OnEnable()
        {
            RefreshVisualState();
        }

        /// <summary>Refreshes text and frame colors from the current button state.</summary>
        public void RefreshVisualState()
        {
            if (_button == null) { _button = GetComponent<Button>(); }
            bool isEnabled = _button == null || _button.interactable;
            bool isActive = isEnabled && (_isPointerOver || _isSelected);
            if (label != null) { label.color = isEnabled ? (isActive ? ActiveColor : Color.white) : DisabledColor; }
            if (background != null) { background.color = isEnabled && isActive ? new Color(0f, 0f, 0f, 0.82f) : new Color(0f, 0f, 0f, 0f); }
            if (selectionFrame != null) { selectionFrame.SetActive(isActive); }
        }

        /// <summary>Shows the cyan dashed selection frame when the button is enabled.</summary>
        /// <param name="eventData">Pointer event received from the EventSystem.</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_button == null || !_button.interactable) { return; }
            _isPointerOver = true;
            RefreshVisualState();
        }

        /// <summary>Restores the unselected visual state after the pointer leaves.</summary>
        /// <param name="eventData">Pointer event received from the EventSystem.</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            _isPointerOver = false;
            RefreshVisualState();
        }

        /// <summary>Marks the button as focused by Unity navigation.</summary>
        /// <param name="eventData">Selection event received from the EventSystem.</param>
        public void OnSelect(BaseEventData eventData)
        {
            _isSelected = true;
            RefreshVisualState();
        }

        /// <summary>Removes the focused visual state from the button.</summary>
        /// <param name="eventData">Deselection event received from the EventSystem.</param>
        public void OnDeselect(BaseEventData eventData)
        {
            _isSelected = false;
            RefreshVisualState();
        }

    }
}

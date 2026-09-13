/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;
using UnityEngine.UI;

namespace Bit.UI
{
    /// <summary>Controls global audio and displays the matching volume state.</summary>
    [RequireComponent(typeof(Button), typeof(Image))]
    public sealed class VolumeButton : MonoBehaviour
    {
        [Header("Sprites")]
        [Tooltip("Sprite shown while audio is enabled.")]
        [SerializeField] private Sprite volumeButtonSprite;
        [Tooltip("Sprite shown while audio is muted.")]
        [SerializeField] private Sprite muteButtonSprite;

        // Button that toggles global audio.
        private Button _button;
        // Image displaying the current audio state.
        private Image _image;
        // Audio state represented by the button.
        private bool _isVolumeEnabled;
        // Volume restored when audio is unmuted.
        private float _volumeBeforeMute;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _image = GetComponent<Image>();
            _volumeBeforeMute = Mathf.Max(0.01f, AudioListener.volume);
            _isVolumeEnabled = AudioListener.volume > 0.001f;
            ApplyState();
        }

        private void OnEnable()
        {
            _button ??= GetComponent<Button>();
            _button.onClick.AddListener(ToggleVolume);
        }

        private void OnDisable()
        {
            _button?.onClick.RemoveListener(ToggleVolume);
        }

        /// <summary>Toggles global audio and updates the displayed sprite.</summary>
        public void ToggleVolume()
        {
            if (_isVolumeEnabled)
            {
                _volumeBeforeMute = Mathf.Max(0.01f, AudioListener.volume);
                AudioListener.volume = 0f;
                _isVolumeEnabled = false;
            }
            else
            {
                AudioListener.volume = _volumeBeforeMute;
                _isVolumeEnabled = true;
            }

            ApplyState();
        }

        /// <summary>Updates the icon to match the current global audio state.</summary>
        private void ApplyState()
        {
            if (_image != null)
            {
                _image.sprite = _isVolumeEnabled ? volumeButtonSprite : muteButtonSprite;
            }
        }
    }
}

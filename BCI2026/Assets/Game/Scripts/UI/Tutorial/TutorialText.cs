/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using UnityEngine;
using UnityEngine.UI;
using Bit.Core;

namespace Bit.UI
{
    /// <summary>Assigns a centrally configured tutorial string to a UI label.</summary>
    [RequireComponent(typeof(Text))]
    public sealed class TutorialText : MonoBehaviour
    {
        [Header("Content")]
        [Tooltip("Identifier of the text displayed by this component.")]
        [SerializeField] private TutorialTextId textId;

        // UI label that displays the configured tutorial text.
        private Text _label;

        private void Awake()
        {
            _label = GetComponent<Text>();
            Refresh();
        }

        /// <summary>Changes the configured text identifier and refreshes the label.</summary>
        /// <param name="id">Identifier of the text to display.</param>
        public void SetTextId(TutorialTextId id)
        {
            textId = id;
            Refresh();
        }

        /// <summary>Refreshes the configured text shown for the current identifier.</summary>
        private void Refresh()
        {
            _label.text = TutorialSettings.Instance.GetText(textId);
        }
    }
}

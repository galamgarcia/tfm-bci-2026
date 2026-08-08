using UnityEngine;
using UnityEngine.UI;
using BciGame.Core;

namespace BciGame.UI
{
    /// <summary>
    /// Assigns a centrally configured tutorial string to a UI label.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public sealed class TutorialText : MonoBehaviour
    {
        [SerializeField] private TutorialTextId textId;
        private Text _label;

        private void Awake()
        {
            _label = GetComponent<Text>();
            Refresh();
        }

        /// <summary>
        /// Changes the configured text identifier and refreshes the label.
        /// </summary>
        /// <param name="id">Identifier of the text to display.</param>
        public void SetTextId(TutorialTextId id)
        {
            textId = id;
            Refresh();
        }

        /// <summary>
        /// Refreshes the localized text displayed using the current text identifier.
        /// </summary>
        private void Refresh()
        {
            _label.text = TutorialSettings.Instance.GetText(textId);
        }
    }
}

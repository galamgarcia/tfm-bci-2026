/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.UI;
using NUnit.Framework;
using UnityEngine;

namespace Bit.Tests
{
    /// <summary>Validates the simulated connection lifecycle without BrainLink hardware.</summary>
    public sealed class ConnectionPopupEditorTests
    {
        /// <summary>Verifies the popup's searching, connecting, connected and hidden states.</summary>
        [Test]
        public void Popup_TransitionsThroughConnectionStates()
        {
            GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/UI/ConnectionPopup.prefab");
            GameObject instance = Object.Instantiate(prefab);
            ConnectionPopup popup = instance.GetComponent<ConnectionPopup>();
            try
            {
                Transform instructions = instance.transform.Find("ConnectionModalPanel/HeadsetInstructionsLabel");
                Transform connectedVisuals = instance.transform.Find("ConnectionModalPanel/DeviceConnectedStateVisuals");
                Assert.That(instructions, Is.Not.Null);
                Assert.That(connectedVisuals, Is.Not.Null);

                popup.ShowSearching();
                Assert.That(popup.IsVisible(), Is.True);
                Assert.That(instructions.gameObject.activeSelf, Is.False);

                popup.ShowConnecting();
                Assert.That(instructions.gameObject.activeSelf, Is.True);

                popup.ShowConnected();
                Assert.That(connectedVisuals.gameObject.activeSelf, Is.True);

                popup.Hide();
                Assert.That(popup.IsVisible(), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }
    }
}

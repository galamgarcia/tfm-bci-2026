/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Bit.Tests
{
    /// <summary>Validates the connection popup prefab states and critical references.</summary>
    public sealed class ConnectionPopupTests
    {
        private GameObject _instance;
        private ConnectionPopup _popup;

        /// <summary>Loads a fresh popup prefab instance before each test.</summary>
        [SetUp]
        public void SetUp()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/UI/ConnectionPopup.prefab");
            _instance = Object.Instantiate(prefab);
            _popup = _instance.GetComponent<ConnectionPopup>();
        }

        /// <summary>Destroys the popup instance created for the current test.</summary>
        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_instance);
        }

        /// <summary>Verifies that all critical popup references are assigned.</summary>
        [Test]
        public void CriticalReferencesAreAssigned()
        {
            SerializedObject serializedPopup = new SerializedObject(_popup);
            string[] references = { "canvasGroup", "blockingOverlay", "activityText", "titleText", "descriptionText", "statusText", "instructionsText", "searchingStateVisuals", "connectingStateVisuals", "connectedStateVisuals", "bitConnectionIcon", "bitIconManager" };
            foreach (string reference in references)
            {
                Assert.That(serializedPopup.FindProperty(reference).objectReferenceValue, Is.Not.Null, reference);
            }
        }

        /// <summary>Verifies that searching hides connection-only visuals.</summary>
        [Test]
        public void SearchingShowsOnlySearchingVisuals()
        {
            _popup.ShowSearching();

            Assert.That(Find("SearchingStateVisuals").gameObject.activeSelf, Is.True);
            Assert.That(Find("DeviceConnectingStateVisuals").gameObject.activeSelf, Is.False);
            Assert.That(Find("DeviceConnectedStateVisuals").gameObject.activeSelf, Is.False);
            Assert.That(Find("HeadsetInstructionsLabel").gameObject.activeSelf, Is.False);
            Assert.That(_popup.IsVisible(), Is.True);
        }

        /// <summary>Verifies that connecting shows the headset instructions and BIT link.</summary>
        [Test]
        public void ConnectingShowsInstructionsAndEmptyLink()
        {
            _popup.ShowConnecting();

            Assert.That(Find("DeviceConnectingStateVisuals").gameObject.activeSelf, Is.True);
            Assert.That(Find("HeadsetInstructionsLabel").gameObject.activeSelf, Is.True);
            Assert.That(Find("BitIconConnectionFill").GetComponent<Image>().fillAmount, Is.EqualTo(0f));
            Assert.That(Find("BitIconConnectionDash_00"), Is.Not.Null);
        }

        /// <summary>Verifies that connected hides instructions and completes the BIT link.</summary>
        [Test]
        public void ConnectedCompletesLinkAndHidesInstructions()
        {
            _popup.ShowConnected();

            Assert.That(Find("DeviceConnectedStateVisuals").gameObject.activeSelf, Is.True);
            Assert.That(Find("HeadsetInstructionsLabel").gameObject.activeSelf, Is.False);
            Assert.That(Find("BitIconConnectionFill").GetComponent<Image>().fillAmount, Is.EqualTo(1f));
        }

        /// <summary>Verifies that hiding the popup releases its visible state.</summary>
        [Test]
        public void HideMakesPopupInvisible()
        {
            _popup.ShowSearching();
            _popup.Hide();

            Assert.That(_popup.IsVisible(), Is.False);
        }

        private Transform Find(string name)
        {
            Transform result = FindRecursive(_instance.transform, name);
            Assert.That(result, Is.Not.Null, name);
            return result;
        }

        private static Transform FindRecursive(Transform parent, string name)
        {
            if (parent.name == name) { return parent; }
            foreach (Transform child in parent)
            {
                Transform result = FindRecursive(child, name);
                if (result != null) { return result; }
            }
            return null;
        }
    }
}

/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System.Collections;
using System.Reflection;
using Bit.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;

namespace Bit.Tests
{
    /// <summary>Validates the simulated connection lifecycle without BrainLink hardware.</summary>
    public sealed class ConnectionPopupPlayModeTests
    {
        /// <summary>Verifies retry, connection completion, delayed close and unlock events.</summary>
        [UnityTest]
        public IEnumerator SimulatedFlowRetriesAndCompletes()
        {
            GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/UI/ConnectionPopup.prefab");
            GameObject instance = Object.Instantiate(prefab);
            ConnectionPopup popup = instance.GetComponent<ConnectionPopup>();
            ConnectionPopupController controller = instance.GetComponent<ConnectionPopupController>();
            controller.enabled = false;

            bool blockingEnded = false;
            bool connectionCompleted = false;
            controller.OnBlockingEnded += () => blockingEnded = true;
            FieldInfo field = typeof(ConnectionPopupController).GetField("onConnectionCompleted", BindingFlags.Instance | BindingFlags.NonPublic);
            ((UnityEvent)field.GetValue(controller)).AddListener(() => connectionCompleted = true);

            controller.SimulateConnectionFlowForEditor();
            yield return new WaitForSecondsRealtime(0.6f);
            Assert.That(popup.IsVisible(), Is.True);
            yield return new WaitForSecondsRealtime(1.1f);
            Assert.That(popup.IsVisible(), Is.True);
            Assert.That(instance.transform.Find("ConnectionModalPanel/HeadsetInstructionsLabel").gameObject.activeSelf, Is.True);
            yield return new WaitForSecondsRealtime(0.6f);
            Assert.That(instance.transform.Find("ConnectionModalPanel/DeviceConnectedStateVisuals").gameObject.activeSelf, Is.True);
            yield return new WaitForSecondsRealtime(3.1f);

            Assert.That(popup.IsVisible(), Is.False);
            Assert.That(blockingEnded, Is.True);
            Assert.That(connectionCompleted, Is.True);
            Object.Destroy(instance);
        }
    }
}

/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using BciGame.Gameplay.Tutorial;
using BciGame.Input;
using BciGame.Input.Signals;
using BciGame.UI.Tutorial;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BciGame.Tests.Editor
{
    /// <summary>Validates tutorial prefab and scene references.</summary>
    public sealed class TutorialScreenTests
    {
        // Tutorial scene validated by reference tests.
        private const string TutorialScenePath = "Assets/Game/Scenes/Tutorial.unity";

        /// <summary>Verifies that EEG training prefabs contain required feedback references.</summary>
        /// <param name="prefabPath">Asset path of the EEG training screen prefab.</param>
        [TestCase("Assets/Game/Prefabs/UI/Tutorial/RelaxationScreen.prefab")]
        [TestCase("Assets/Game/Prefabs/UI/Tutorial/ConcentrationScreen.prefab")]
        public void EegTrainingScreen_HasAllRequiredFeedbackReferences(string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            TutorialEegTrainingScreen screen = prefab.GetComponent<TutorialEegTrainingScreen>();
            SerializedObject serializedScreen = new SerializedObject(screen);

            Assert.That(serializedScreen.FindProperty("fillImage").objectReferenceValue, Is.Not.Null, $"Missing Fill image in {prefabPath}.");
            Assert.That(serializedScreen.FindProperty("successCheck").objectReferenceValue, Is.Not.Null, $"Missing success check in {prefabPath}.");
            Assert.That(serializedScreen.FindProperty("resultText").objectReferenceValue, Is.Not.Null, $"Missing result text in {prefabPath}.");
        }

        /// <summary>Verifies that the tutorial scene assigns the AR face manager.</summary>
        [Test]
        public void TutorialScene_HeadPoseTracker_HasFaceManagerReference()
        {
            Scene scene = EditorSceneManager.OpenScene(TutorialScenePath, OpenSceneMode.Additive);
            try
            {
                HeadPoseTracker tracker = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    tracker = root.GetComponentInChildren<HeadPoseTracker>(true);
                    if (tracker != null) { break; }
                }

                Assert.That(tracker, Is.Not.Null, "Missing HeadPoseTracker in the tutorial scene.");
                SerializedObject serializedTracker = new SerializedObject(tracker);
                Assert.That(serializedTracker.FindProperty("faceManager").objectReferenceValue, Is.Not.Null, "Missing AR Face Manager reference in HeadPoseTracker.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        /// <summary>Verifies that the tutorial scene uses the BrainLink manager extension.</summary>
        [Test]
        public void TutorialScene_UsesBrainLinkManagerExtension()
        {
            Scene scene = EditorSceneManager.OpenScene(TutorialScenePath, OpenSceneMode.Additive);
            try
            {
                BrainLinkManager manager = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    manager = root.GetComponentInChildren<BrainLinkManager>(true);
                    if (manager != null) { break; }
                }

                Assert.That(manager, Is.Not.Null, "Missing BrainLinkManager extension in the tutorial scene.");
                Assert.That(manager.gameObject.name, Is.EqualTo("ThinkGearManager"), "The SDK listener GameObject name must remain unchanged.");

                BrainLinkConnection connection = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    connection = root.GetComponentInChildren<BrainLinkConnection>(true);
                    if (connection != null) { break; }
                }

                Assert.That(connection, Is.Not.Null, "Missing BrainLinkConnection in the tutorial scene.");
                SerializedObject serializedConnection = new SerializedObject(connection);
                Assert.That(serializedConnection.FindProperty("thinkGearManager").objectReferenceValue, Is.EqualTo(manager), "BrainLinkConnection must reference the BrainLinkManager extension.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}

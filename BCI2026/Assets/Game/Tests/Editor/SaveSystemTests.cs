/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using Bit.Core;
using Bit.Services;
using NUnit.Framework;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Bit.Tests.Editor
{
    /// <summary>Validates local level progression and saved-game behavior.</summary>
    public sealed class SaveSystemTests
    {
        // Temporary object hosting the service under test.
        private GameObject _gameObject;
        // Save service instance used by each test.
        private SaveSystem _service;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("SaveSystemTests");
            _service = _gameObject.AddComponent<SaveSystem>();
            _service.DeleteSave();
        }

        [TearDown]
        public void TearDown()
        {
            _service.DeleteSave();
            Object.DestroyImmediate(_gameObject);
        }

        /// <summary>Verifies that an absent save disables continuation and resolves to the first level.</summary>
        [Test]
        public void MissingSave_DisablesContinueAndResolvesFirstLevel()
        {
            Assert.That(_service.HasSavedGame(), Is.False);
            Assert.That(_service.GetLastUnlockedLevel(), Is.EqualTo(0));
            Assert.That(_service.GetContinueSceneName(), Is.EqualTo("Level01"));
        }

        /// <summary>Verifies that starting a new game stores the first level.</summary>
        [Test]
        public void StartNewGame_StoresFirstLevel()
        {
            _service.StartNewGame();

            Assert.That(_service.HasSavedGame(), Is.True);
            Assert.That(_service.GetLastUnlockedLevel(), Is.EqualTo(1));
            Assert.That(_service.GetContinueSceneName(), Is.EqualTo("Level01"));
        }

        /// <summary>Verifies that a completed level unlocks the following configured scene.</summary>
        [Test]
        public void CompleteCurrentLevel_UnlocksNextLevel()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Game/Scenes/Levels/Level01.unity", OpenSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                _service.StartNewGame();

                string nextScene = _service.CompleteCurrentLevel();

                Assert.That(nextScene, Is.EqualTo("Level02"));
                Assert.That(_service.GetLastUnlockedLevel(), Is.EqualTo(2));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        /// <summary>Verifies that new-game progress replaces a previously unlocked level.</summary>
        [Test]
        public void StartNewGame_ReplacesExistingProgress()
        {
            PlayerPrefs.SetInt("Bit.LastUnlockedLevel", 5);
            PlayerPrefs.Save();

            _service.StartNewGame();

            Assert.That(_service.GetLastUnlockedLevel(), Is.EqualTo(1));
        }
    }
}

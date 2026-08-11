using BciGame.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace BciGame.Tests.Editor
{
    public sealed class TutorialScreenTests
    {
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
    }
}

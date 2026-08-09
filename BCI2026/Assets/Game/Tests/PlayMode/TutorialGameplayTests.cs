using System.Collections;
using BciGame.Gameplay;
using BciGame.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace BciGame.Tests.PlayMode
{
    public sealed class TutorialGameplayTests
    {
        private const string MovementScreenPrefabPath = "Assets/Game/Prefabs/UI/Tutorial/MovementScreen.prefab";

        [UnityTest]
        public IEnumerator Activate_CreatesBallAndGoal()
        {
            TutorialMovementScreen prefab = AssetDatabase.LoadAssetAtPath<TutorialMovementScreen>(MovementScreenPrefabPath);
            Assert.That(prefab, Is.Not.Null);

            TutorialMovementScreen screen = Object.Instantiate(prefab);
            screen.Activate();
            yield return null;

            Assert.That(screen.GetComponentsInChildren<TutorialBall>(true), Has.Length.EqualTo(1));
            Assert.That(screen.GetComponentsInChildren<TutorialGoal>(true), Has.Length.EqualTo(1));

            screen.Deactivate();
            Object.Destroy(screen.gameObject);
        }
    }
}

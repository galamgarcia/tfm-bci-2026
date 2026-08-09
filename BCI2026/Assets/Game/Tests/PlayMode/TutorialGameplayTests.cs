using System;
using System.Collections;
using BciGame.Input;
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
        private const float ExtremeHorizontalInput = 1000000f;

        [UnityTest]
        public IEnumerator Activate_CreatesBallAndGoal()
        {
            TutorialMovementScreen prefab = AssetDatabase.LoadAssetAtPath<TutorialMovementScreen>(MovementScreenPrefabPath);
            Assert.That(prefab, Is.Not.Null);

            TutorialMovementScreen screen = UnityEngine.Object.Instantiate(prefab);
            screen.Activate();
            yield return null;

            Assert.That(screen.GetComponentsInChildren<TutorialBall>(true), Has.Length.EqualTo(1));
            Assert.That(screen.GetComponentsInChildren<TutorialGoal>(true), Has.Length.EqualTo(1));

            screen.Deactivate();
            UnityEngine.Object.Destroy(screen.gameObject);
        }

        [UnityTest]
        public IEnumerator Activate_SetsInitialBallAndGoalPositions()
        {
            TutorialMovementScreen prefab = AssetDatabase.LoadAssetAtPath<TutorialMovementScreen>(MovementScreenPrefabPath);
            Assert.That(prefab, Is.Not.Null);

            TutorialMovementScreen screen = UnityEngine.Object.Instantiate(prefab);
            screen.Activate();
            yield return null;

            TutorialBall ball = screen.GetComponentInChildren<TutorialBall>(true);
            TutorialGoal goal = screen.GetComponentInChildren<TutorialGoal>(true);
            Assert.That(ball.Position, Is.EqualTo(new Vector2(-260f, 0f)));
            Assert.That(goal.Position, Is.EqualTo(new Vector2(260f, 0f)));

            screen.Deactivate();
            UnityEngine.Object.Destroy(screen.gameObject);
        }

        [UnityTest]
        public IEnumerator Activate_LimitsBallMovementToBounds()
        {
            TutorialMovementScreen prefab = AssetDatabase.LoadAssetAtPath<TutorialMovementScreen>(MovementScreenPrefabPath);
            Assert.That(prefab, Is.Not.Null);

            TutorialMovementScreen screen = UnityEngine.Object.Instantiate(prefab);
            screen.Activate();
            yield return null;

            TutorialBall ball = screen.GetComponentInChildren<TutorialBall>(true);
            TestHeadInputSource headInputSource = new TestHeadInputSource();
            ball.GetComponent<InputComponent>().ConfigureSources(headInputSource, null);

            // Force the clamp regardless of the variable delta time used by the test runner.
            headInputSource.HorizontalInput = ExtremeHorizontalInput;
            yield return null;
            Assert.That(ball.Position.x, Is.EqualTo(290f));

            headInputSource.HorizontalInput = -ExtremeHorizontalInput;
            yield return null;
            Assert.That(ball.Position.x, Is.EqualTo(-290f));

            screen.Deactivate();
            UnityEngine.Object.Destroy(screen.gameObject);
        }

        [UnityTest]
        public IEnumerator CompleteMovementPhase_RequiresFocusAndDefocusOnOppositeSides()
        {
            TutorialMovementScreen prefab = AssetDatabase.LoadAssetAtPath<TutorialMovementScreen>(MovementScreenPrefabPath);
            Assert.That(prefab, Is.Not.Null);

            TutorialMovementScreen screen = UnityEngine.Object.Instantiate(prefab);
            bool isComplete = false;
            screen.OnComplete += () => isComplete = true;
            screen.Activate();
            yield return null;

            TutorialBall ball = screen.GetComponentInChildren<TutorialBall>(true);
            TutorialGoal goal = screen.GetComponentInChildren<TutorialGoal>(true);
            TestHeadInputSource headInputSource = new TestHeadInputSource();
            TestMentalInputSource mentalInputSource = new TestMentalInputSource();
            screen.ConfigureInputSources(headInputSource, mentalInputSource);

            headInputSource.HorizontalInput = ExtremeHorizontalInput;
            yield return null;
            Assert.That(goal.Position.x, Is.EqualTo(-260f));

            mentalInputSource.HasValidSignal = true;
            mentalInputSource.Concentration = 0.1f;
            headInputSource.HorizontalInput = -ExtremeHorizontalInput;
            yield return null;
            Assert.That(goal.Position.x, Is.EqualTo(-260f));

            mentalInputSource.Concentration = 0.5f;
            yield return null;
            Assert.That(goal.Position.x, Is.EqualTo(260f));

            headInputSource.HorizontalInput = ExtremeHorizontalInput;
            yield return null;
            Assert.That(isComplete, Is.False);

            mentalInputSource.Concentration = 0.1f;
            yield return null;
            Assert.That(isComplete, Is.True);

            screen.Deactivate();
            UnityEngine.Object.Destroy(screen.gameObject);
        }

        private sealed class TestHeadInputSource : IHeadInputSource
        {
            public bool HasFace => true;
            public float HorizontalInput { get; set; }

            public event Action NodDetected
            {
                add { }
                remove { }
            }
        }

        private sealed class TestMentalInputSource : IMentalInputSource
        {
            public bool HasValidSignal { get; set; }
            public float Relaxation { get; set; }
            public float Concentration { get; set; }
        }
    }
}

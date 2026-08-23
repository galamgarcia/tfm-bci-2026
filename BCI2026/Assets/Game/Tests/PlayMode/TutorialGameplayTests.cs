using System;
using System.Collections;
using BciGame.Gameplay;
using BciGame.Gameplay.Tutorial;
using BciGame.Input;
using BciGame.Input.Signals;
using BciGame.UI.Tutorial;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace BciGame.Tests.PlayMode
{
    /// <summary>Validates tutorial movement gameplay in play mode.</summary>
    public sealed class TutorialGameplayTests
    {
        // Asset path of the movement screen prefab under test.
        private const string MovementScreenPrefabPath = "Assets/Game/Prefabs/UI/Tutorial/MovementScreen.prefab";
        // Input magnitude used to reach either movement bound in one frame.
        private const float ExtremeHorizontalInput = 1000000f;

        /// <summary>Verifies that activating the screen creates one ball and one goal.</summary>
        /// <returns>Coroutine that completes the play-mode assertion.</returns>
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

        /// <summary>Verifies that activating the screen applies its configured positions.</summary>
        /// <returns>Coroutine that completes the play-mode assertion.</returns>
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
            Assert.That(ball.GetPosition(), Is.EqualTo(new Vector2(-260f, 0f)));
            Assert.That(goal.GetPosition(), Is.EqualTo(new Vector2(260f, 0f)));

            screen.Deactivate();
            UnityEngine.Object.Destroy(screen.gameObject);
        }

        /// <summary>Verifies that horizontal ball movement is constrained to its bounds.</summary>
        /// <returns>Coroutine that completes the play-mode assertion.</returns>
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
            ball.GetComponent<InputController>().ConfigureSources(headInputSource, null);

            // Force the clamp regardless of the variable delta time used by the test runner.
            headInputSource.HorizontalInput = ExtremeHorizontalInput;
            yield return null;
            Assert.That(ball.GetPosition().x, Is.EqualTo(290f));

            headInputSource.HorizontalInput = -ExtremeHorizontalInput;
            yield return null;
            Assert.That(ball.GetPosition().x, Is.EqualTo(-290f));

            screen.Deactivate();
            UnityEngine.Object.Destroy(screen.gameObject);
        }

        /// <summary>Verifies that movement completion requires opposite focus and defocus rounds.</summary>
        /// <returns>Coroutine that completes the play-mode assertion.</returns>
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
            screen.ConfigureInput(headInputSource, mentalInputSource);

            headInputSource.HorizontalInput = ExtremeHorizontalInput;
            yield return null;
            Assert.That(goal.GetPosition().x, Is.EqualTo(-260f));

            mentalInputSource.HasValidSignal = true;
            mentalInputSource.Concentration = 0.1f;
            headInputSource.HorizontalInput = -ExtremeHorizontalInput;
            yield return null;
            Assert.That(goal.GetPosition().x, Is.EqualTo(-260f));

            mentalInputSource.Concentration = 0.5f;
            yield return null;
            Assert.That(goal.GetPosition().x, Is.EqualTo(260f));

            headInputSource.HorizontalInput = ExtremeHorizontalInput;
            yield return null;
            Assert.That(isComplete, Is.False);

            mentalInputSource.Concentration = 0.1f;
            yield return null;
            Assert.That(isComplete, Is.True);

            screen.Deactivate();
            UnityEngine.Object.Destroy(screen.gameObject);
        }

        /// <summary>Provides controllable head input for movement tests.</summary>
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

        /// <summary>Provides controllable mental input for movement tests.</summary>
        private sealed class TestMentalInputSource : IMentalInputSource
        {
            public bool HasValidSignal { get; set; }
            public float Relaxation { get; set; }
            public float Concentration { get; set; }
        }
    }
}

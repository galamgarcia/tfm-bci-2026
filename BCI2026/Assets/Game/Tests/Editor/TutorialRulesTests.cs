/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using BciGame.Gameplay;
using NUnit.Framework;

namespace BciGame.Tests.Editor
{
    /// <summary>Validates pure rules used by the tutorial flow.</summary>
    public sealed class TutorialRulesTests
    {
        /// <summary>Verifies that a neutral goal requirement is always satisfied.</summary>
        [Test]
        public void AreGoalRequirementsMet_ReturnsTrueForNeutralRequirement()
        {
            bool result = TutorialRules.AreGoalRequirementsMet(MentalStateLevel.None, MentalStateLevel.None, false);
            Assert.That(result, Is.True);
        }

        /// <summary>Verifies that a low goal accepts only low concentration.</summary>
        [Test]
        public void AreGoalRequirementsMet_ReturnsTrueOnlyForLowConcentration()
        {
            Assert.That(TutorialRules.AreGoalRequirementsMet(MentalStateLevel.Low, MentalStateLevel.Low, true), Is.True);
            Assert.That(TutorialRules.AreGoalRequirementsMet(MentalStateLevel.Low, MentalStateLevel.Medium, true), Is.False);
            Assert.That(TutorialRules.AreGoalRequirementsMet(MentalStateLevel.Low, MentalStateLevel.High, true), Is.False);
        }

        /// <summary>Verifies that medium goals accept medium and high concentration.</summary>
        /// <param name="level">Current concentration level.</param>
        [TestCase(MentalStateLevel.Medium)]
        [TestCase(MentalStateLevel.High)]
        public void AreGoalRequirementsMet_ReturnsTrueForMediumOrHighConcentration(MentalStateLevel level)
        {
            bool result = TutorialRules.AreGoalRequirementsMet(MentalStateLevel.Medium, level, true);
            Assert.That(result, Is.True);
        }

        /// <summary>Verifies that non-neutral goals reject invalid signals.</summary>
        /// <param name="required">Goal concentration requirement.</param>
        [TestCase(MentalStateLevel.Low)]
        [TestCase(MentalStateLevel.Medium)]
        public void AreGoalRequirementsMet_ReturnsFalseWhenSignalIsInvalid(MentalStateLevel required)
        {
            bool result = TutorialRules.AreGoalRequirementsMet(required, required, false);
            Assert.That(result, Is.False);
        }

        /// <summary>Verifies the first mental round requirement for unfocused players.</summary>
        /// <param name="level">Current concentration level.</param>
        [TestCase(MentalStateLevel.None)]
        [TestCase(MentalStateLevel.Low)]
        public void GetFirstMentalRoundRequirement_ReturnsMediumForUnfocusedPlayer(MentalStateLevel level)
        {
            Assert.That(TutorialRules.GetFirstMentalMovementRoundRequirement(level), Is.EqualTo(MentalStateLevel.Medium));
        }

        /// <summary>Verifies the first mental round requirement for focused players.</summary>
        /// <param name="level">Current concentration level.</param>
        [TestCase(MentalStateLevel.Medium)]
        [TestCase(MentalStateLevel.High)]
        public void GetFirstMentalMovementRoundRequirement_ReturnsLowForFocusedConcentration(MentalStateLevel level)
        {
            Assert.That(TutorialRules.GetFirstMentalMovementRoundRequirement(level), Is.EqualTo(MentalStateLevel.Low));
        }

        /// <summary>Verifies that the next mental round uses the opposite requirement.</summary>
        /// <param name="previous">Previous mental-state requirement.</param>
        /// <param name="expected">Expected opposite requirement.</param>
        [TestCase(MentalStateLevel.Low, MentalStateLevel.Medium)]
        [TestCase(MentalStateLevel.Medium, MentalStateLevel.Low)]
        public void GetOppositeMentalMovementRoundRequirement_ReturnsOppositeRequirement(MentalStateLevel previous, MentalStateLevel expected)
        {
            Assert.That(TutorialRules.GetOppositeMentalMovementRoundRequirement(previous), Is.EqualTo(expected));
        }

        /// <summary>Verifies that horizontal movement remains within its configured bounds.</summary>
        /// <param name="current">Current horizontal position.</param>
        /// <param name="delta">Horizontal displacement.</param>
        /// <param name="min">Minimum allowed position.</param>
        /// <param name="max">Maximum allowed position.</param>
        /// <param name="expectedPosition">Expected bounded position.</param>
        [TestCase(-260f, -100f, -290f, 290f, -290f)]
        [TestCase(260f, 100f, -290f, 290f, 290f)]
        [TestCase(0f, 50f, -290f, 290f, 50f)]
        public void GetBoundedHorizontal_ReturnsPositionWithinBounds(float current, float delta, float min, float max, float expectedPosition)
        {
            float position = TutorialRules.GetBoundedHorizontal(current, delta, min, max);
            Assert.That(position, Is.EqualTo(expectedPosition));
        }
    }
}

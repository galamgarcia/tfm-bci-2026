using BciGame.Gameplay;
using Game.Scripts.Gameplay;
using NUnit.Framework;

namespace BciGame.Tests.Editor
{
    public sealed class TutorialRulesTests
    {
        [Test]
        public void AreGoalRequirementsMet_ReturnsTrueForNeutralRequirement()
        {
            bool result = TutorialRules.AreGoalRequirementsMet(MentalStateLevel.None, MentalStateLevel.None, false);
            Assert.That(result, Is.True);
        }

        [Test]
        public void AreGoalRequirementsMet_ReturnsTrueOnlyForLowConcentration()
        {
            Assert.That(TutorialRules.AreGoalRequirementsMet(MentalStateLevel.Low, MentalStateLevel.Low, true), Is.True);
            Assert.That(TutorialRules.AreGoalRequirementsMet(MentalStateLevel.Low, MentalStateLevel.Medium, true), Is.False);
            Assert.That(TutorialRules.AreGoalRequirementsMet(MentalStateLevel.Low, MentalStateLevel.High, true), Is.False);
        }

        [TestCase(MentalStateLevel.Medium)]
        [TestCase(MentalStateLevel.High)]
        public void AreGoalRequirementsMet_ReturnsTrueForMediumOrHighConcentration(MentalStateLevel level)
        {
            bool result = TutorialRules.AreGoalRequirementsMet(MentalStateLevel.Medium, level, true);
            Assert.That(result, Is.True);
        }

        [TestCase(MentalStateLevel.Low)]
        [TestCase(MentalStateLevel.Medium)]
        public void AreGoalRequirementsMet_ReturnsFalseWhenSignalIsInvalid(MentalStateLevel required)
        {
            bool result = TutorialRules.AreGoalRequirementsMet(required, required, false);
            Assert.That(result, Is.False);
        }

        [TestCase(MentalStateLevel.None)]
        [TestCase(MentalStateLevel.Low)]
        public void GetFirstMentalRoundRequirement_ReturnsMediumForUnfocusedPlayer(MentalStateLevel level)
        {
            Assert.That(TutorialRules.GetFirstMentalMovementRoundRequirement(level), Is.EqualTo(MentalStateLevel.Medium));
        }

        [TestCase(MentalStateLevel.Medium)]
        [TestCase(MentalStateLevel.High)]
        public void GetFirstMentalMovementRoundRequirement_ReturnsLowForFocusedConcentration(MentalStateLevel level)
        {
            Assert.That(TutorialRules.GetFirstMentalMovementRoundRequirement(level), Is.EqualTo(MentalStateLevel.Low));
        }

        [TestCase(MentalStateLevel.Low, MentalStateLevel.Medium)]
        [TestCase(MentalStateLevel.Medium, MentalStateLevel.Low)]
        public void GetOppositeMentalMovementRoundRequirement_ReturnsOppositeRequirement(MentalStateLevel previous, MentalStateLevel expected)
        {
            Assert.That(TutorialRules.GetOppositeMentalMovementRoundRequirement(previous), Is.EqualTo(expected));
        }

        [TestCase(-260f, -100f, -290f, 290f, -290f)]
        [TestCase(260f, 100f, -290f, 290f, 290f)]
        [TestCase(0f, 50f, -290f, 290f, 50f)]
        public void GetBoundedHorizontalPosition_ReturnsPositionWithinBounds(float current, float delta, float min, float max, float expectedPosition)
        {
            float position = TutorialRules.GetBoundedHorizontalPosition(current, delta, min, max);
            Assert.That(position, Is.EqualTo(expectedPosition));
        }
    }
}

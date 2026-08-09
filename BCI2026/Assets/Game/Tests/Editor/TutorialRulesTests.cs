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
        public void AreGoalRequirementsMet_ReturnsTrueForMediumOrHighConcentration(MentalStateLevel concentrationLevel)
        {
            bool result = TutorialRules.AreGoalRequirementsMet(MentalStateLevel.Medium, concentrationLevel, true);
            Assert.That(result, Is.True);
        }

        [TestCase(MentalStateLevel.Low)]
        [TestCase(MentalStateLevel.Medium)]
        public void AreGoalRequirementsMet_ReturnsFalseForNonNeutralRequirementAndInvalidSignal(MentalStateLevel requiredLevel)
        {
            bool result = TutorialRules.AreGoalRequirementsMet(requiredLevel, requiredLevel, false);
            Assert.That(result, Is.False);
        }

        [TestCase(MentalStateLevel.None)]
        [TestCase(MentalStateLevel.Low)]
        public void GetFirstMentalRoundRequirement_ReturnsMediumForNonFocusedConcentration(MentalStateLevel concentrationLevel)
        {
            Assert.That(TutorialRules.GetFirstMentalMovementRoundRequirement(concentrationLevel), Is.EqualTo(MentalStateLevel.Medium));
        }

        [TestCase(MentalStateLevel.Medium)]
        [TestCase(MentalStateLevel.High)]
        public void GetFirstMentalRoundRequirement_ReturnsLowForFocusedConcentration(MentalStateLevel concentrationLevel)
        {
            Assert.That(TutorialRules.GetFirstMentalMovementRoundRequirement(concentrationLevel), Is.EqualTo(MentalStateLevel.Low));
        }

        [TestCase(MentalStateLevel.Low, MentalStateLevel.Medium)]
        [TestCase(MentalStateLevel.Medium, MentalStateLevel.Low)]
        public void GetNextMentalRoundRequirement_ReturnsOppositeRequirement(MentalStateLevel previousRequirement, MentalStateLevel expectedRequirement)
        {
            Assert.That(TutorialRules.GetOppositeMentalMovementRoundRequirement(previousRequirement), Is.EqualTo(expectedRequirement));
        }
    }
}

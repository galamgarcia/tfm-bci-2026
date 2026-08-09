using Game.Scripts.Gameplay;

namespace BciGame.Gameplay
{
    /// <summary>
    /// Provides pure gameplay rules used by the tutorial flow.
    /// </summary>
    public static class TutorialRules
    {
        /// <summary>Determines whether a concentration level satisfies a goal requirement.</summary>
        /// <param name="requiredConcentration">Concentration level required by the goal.</param>
        /// <param name="currentConcentration">Current concentration level reported by the input component.</param>
        /// <param name="hasValidEegSignal">Whether the EEG signal is valid for non-neutral goals.</param>
        /// <returns>Whether the goal's concentration requirement is satisfied.</returns>
        public static bool AreGoalRequirementsMet(MentalStateLevel requiredConcentration, MentalStateLevel currentConcentration, bool hasValidEegSignal)
        {
            if (requiredConcentration == MentalStateLevel.None) { return true; }
            if (!hasValidEegSignal) { return false; }
            return requiredConcentration == MentalStateLevel.Low ? currentConcentration == MentalStateLevel.Low : currentConcentration >= requiredConcentration;
        }

        /// <summary>Gets the first mental-state requirement after the neutral movement round.</summary>
        /// <param name="currentConcentration">Concentration level measured during the neutral round.</param>
        /// <returns>Low when already concentrated; otherwise Medium.</returns>
        public static MentalStateLevel GetFirstMentalMovementRoundRequirement(MentalStateLevel currentConcentration)
        {
            return currentConcentration >= MentalStateLevel.Medium ? MentalStateLevel.Low : MentalStateLevel.Medium;
        }

        /// <summary>Gets the mental-state requirement opposite to the previous round.</summary>
        /// <param name="previousRequirement">Requirement used in the previous mental round.</param>
        /// <returns>Medium after Low; otherwise Low.</returns>
        public static MentalStateLevel GetOppositeMentalMovementRoundRequirement(MentalStateLevel previousRequirement)
        {
            return previousRequirement == MentalStateLevel.Medium ? MentalStateLevel.Low : MentalStateLevel.Medium;
        }
    }
}

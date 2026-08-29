/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using BciGame.Gameplay;
using UnityEngine;

namespace BciGame.Gameplay
{
    /// <summary>Provides pure gameplay rules used by the tutorial flow.</summary>
    public static class TutorialRules
    {
        /// <summary>Determines if a concentration level satisfies a goal requirement.</summary>
        /// <param name="required">Concentration level required by the goal.</param>
        /// <param name="current">Current concentration level reported by the input component.</param>
        /// <param name="isValid">Indicates if the EEG signal is valid for non-neutral goals.</param>
        /// <returns>True, the goal's concentration requirement is satisfied.</returns>
        public static bool AreGoalRequirementsMet(MentalStateLevel required, MentalStateLevel current, bool isValid)
        {
            if (required == MentalStateLevel.None) { return true; }
            if (!isValid) { return false; }
            return required == MentalStateLevel.Low ? current == MentalStateLevel.Low : current >= required;
        }

        /// <summary>Determines if the ball can trigger a goal from its position and EEG state.</summary>
        /// <param name="ballPosition">Current ball position in canvas space.</param>
        /// <param name="goalPosition">Goal position in canvas space.</param>
        /// <param name="radius">Maximum distance allowed between ball and goal.</param>
        /// <param name="required">Concentration level required by the goal.</param>
        /// <param name="current">Current concentration level reported by the input component.</param>
        /// <param name="isValid">If the EEG signal is valid for non-neutral goals.</param>
        /// <returns>If the ball satisfies the goal position and concentration requirements.</returns>
        public static bool CanTriggerGoal(Vector2 ballPosition, Vector2 goalPosition, float radius, MentalStateLevel required, MentalStateLevel current, bool isValid)
        {
            return Vector2.Distance(ballPosition, goalPosition) <= radius && AreGoalRequirementsMet(required, current, isValid);
        }

        /// <summary>Gets the first mental-state requirement after the neutral movement round.</summary>
        /// <param name="current">Concentration level measured during the neutral round.</param>
        /// <returns>Low when already concentrated; otherwise Medium.</returns>
        public static MentalStateLevel GetFirstMentalMovementRoundRequirement(MentalStateLevel current)
        {
            return current >= MentalStateLevel.Medium ? MentalStateLevel.Low : MentalStateLevel.Medium;
        }

        /// <summary>Gets the mental-state requirement for the next round.</summary>
        /// <param name="current">Requirement used in the previous mental round.</param>
        /// <returns>Medium after Low; otherwise Low.</returns>
        public static MentalStateLevel GetOppositeMentalMovementRoundRequirement(MentalStateLevel current)
        {
            return current == MentalStateLevel.Medium ? MentalStateLevel.Low : MentalStateLevel.Medium;
        }

        /// <summary>Gets a horizontal position constrained to the configured movement bounds.</summary>
        /// <param name="position">Current horizontal canvas position.</param>
        /// <param name="delta">Horizontal displacement to apply.</param>
        /// <param name="min">Minimum permitted horizontal position.</param>
        /// <param name="max">Maximum permitted horizontal position.</param>
        /// <returns>The resulting horizontal position within the configured bounds.</returns>
        public static float GetBoundedHorizontal(float position, float delta, float min, float max)
        {
            return Mathf.Clamp(position + delta, min, max);
        }
    }
}

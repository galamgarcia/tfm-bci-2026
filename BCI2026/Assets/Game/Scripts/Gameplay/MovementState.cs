/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

namespace BciGame.UI
{
    /// <summary>
    /// Defines the input source that drives a tutorial ball.
    /// </summary>
    public enum MovementState
    {
        /// <summary>Disables automatic movement.</summary>
        None,
        /// <summary>Moves horizontally from tracked head yaw.</summary>
        HeadYaw,
        /// <summary>Moves upward from the relaxation EEG value.</summary>
        RelaxationUp,
        /// <summary>Moves downward from the concentration EEG value.</summary>
        ConcentrationDown
    }
}

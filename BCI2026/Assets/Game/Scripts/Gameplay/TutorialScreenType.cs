/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

namespace BciGame.Gameplay
{
    /// <summary>
    /// Identifies the behavior associated with a tutorial screen independently of its navigation order.
    /// </summary>
    public enum TutorialScreenType
    {
        Welcome,
        HeadsetConfirmation,
        Connection,
        EegSignal,
        PracticeIntro,
        Relaxation,
        Concentration,
        Movement,
        Complete = 8
    }
}

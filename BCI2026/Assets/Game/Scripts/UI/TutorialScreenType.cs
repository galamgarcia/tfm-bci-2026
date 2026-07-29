namespace BciGame.UI
{
    /// <summary>
    /// Identifies the behavior associated with a tutorial screen independently of its navigation order.
    /// </summary>
    public enum TutorialScreenType
    {
        /// <summary>Initial welcome screen.</summary>
        Welcome,
        /// <summary>Headset confirmation screen completed by a nod.</summary>
        HeadsetConfirmation,
        /// <summary>BrainLink connection progress screen.</summary>
        Connection,
        /// <summary>EEG signal quality explanation screen.</summary>
        EegSignal,
        /// <summary>Practice sequence introduction screen.</summary>
        PracticeIntro,
        /// <summary>Relaxation EEG training screen.</summary>
        Relaxation,
        /// <summary>Concentration EEG training screen.</summary>
        Concentration,
        /// <summary>Head-controlled horizontal movement screen.</summary>
        HeadMovement,
        /// <summary>Relaxation-controlled vertical movement screen.</summary>
        RelaxationMovement,
        /// <summary>Concentration-controlled vertical movement screen.</summary>
        ConcentrationMovement,
        /// <summary>Final tutorial completion screen.</summary>
        Complete
    }
}

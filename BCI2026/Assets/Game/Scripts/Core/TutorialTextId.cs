/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

namespace Bit.Core
{
    /// <summary>IDs for all UI tutorial texts.</summary>
    public enum TutorialTextId
    {
        None = -1,
        WelcomeHeader,
        WelcomeBody,
        WelcomeContinue,
        HeadsetHeader,
        HeadsetInstruction,
        HeadsetSuccess,
        ConnectionConnecting,
        ConnectionSearching,
        ConnectionConnected,
        EegSignalHeader,
        EegSignalBody,
        EegSignalContinue,
        PracticeHeader,
        PracticeBody,
        PracticeStart,
        RelaxationHeader,
        RelaxationInstruction,
        RelaxationActivated,
        RelaxationRelaxed,
        RelaxationSuccess,
        ConcentrationHeader,
        ConcentrationInstruction,
        ConcentrationDistracted,
        ConcentrationFocused,
        ConcentrationSuccess,
        MovementHeader,
        MovementInitialInstruction,
        MovementFocusInstruction,
        MovementDefocusInstruction,
        RelaxationMovementHeader,
        RelaxationMovementInstruction,
        ConcentrationMovementHeader,
        ConcentrationMovementInstruction,
        CompleteHeader,
        CompleteBody,
        CompleteStart
    }
}

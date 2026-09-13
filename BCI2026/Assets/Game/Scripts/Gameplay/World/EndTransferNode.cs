/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

using System.Collections;
using Bit.Services;
using Bit.UI;

namespace Bit.Gameplay
{
    /// <summary>Completes the final level without loading another gameplay scene.</summary>
    public sealed class EndTransferNode : TransferNode
    {
        [UnityEngine.Header("Final Completion")]
        [UnityEngine.Tooltip("Modal opened after the final transfer completes.")]
        [UnityEngine.SerializeField] private CongratsMenuController congratsMenu;

        /// <summary>Shows the final modal after saving the completed level.</summary>
        /// <returns>Coroutine that completes the final transfer.</returns>
        protected override IEnumerator FinishTransfer()
        {
            SetTransferring();
            GetPlayer()?.GetComponent<BitTransferEffect>()?.PlayTransferOut();
            yield return new UnityEngine.WaitForSecondsRealtime(GetTransferDelay());
            SaveSystem.Instance?.CompleteCurrentLevel();
            congratsMenu?.Open();
        }
    }
}

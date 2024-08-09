using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;

namespace Dalamud.Tomestone.Features
{
    internal static class TripleTriad
    {
        internal static unsafe List<uint> GetTripleTriadCards(UIState* uiState)
        {
            var triadCards = new List<uint>();
            // Ensure UIState is initialized
            if (uiState == null)
            {
                throw new Exception("UIState is null.");
            }

            var triadSheet = Service.DataManager.GetExcelSheet<TripleTriadCard>();
            if (triadSheet == null)
            {
                throw new Exception("Triple Triad sheet is null.");
            }

            foreach (var row in triadSheet)
            {
                if (uiState->IsTripleTriadCardUnlocked((ushort)row.RowId))
                {
                    triadCards.Add((uint)row.RowId);
                }
            }

            return triadCards;
        }
    }
}

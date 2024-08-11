using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.GeneratedSheets2;
using System;
using System.Collections.Generic;

namespace Dalamud.Tomestone.Features
{
    internal static class Orchestrion
    {
        internal static unsafe List<uint> GetOrchestrionRolls(PlayerState* pState)
        {
            var orchestrionRolls = new List<uint>();
            // Ensure UIState is initialized
            if (pState == null)
            {
                throw new Exception("PlayerState is null.");
            }

            var orchestrionSheet = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets2.Orchestrion>();
            if (orchestrionSheet == null)
            {
                throw new Exception("Orchestrion sheet is null.");
            }

            foreach (var row in orchestrionSheet)
            {
                if (pState->IsOrchestrionRollUnlocked((ushort)row.RowId))
                {
                    orchestrionRolls.Add((uint)row.RowId);
                }
            }

            return orchestrionRolls;
        }
    }
}

using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dalamud.Tomestone.Features
{
    internal static class Minions
    {
        internal static unsafe List<uint> GetUnlockedMinions(UIState* uiState)
        {
            List<uint> unlockedMinions = new List<uint>();
            // Ensure UIState is initialized
            if (uiState == null)
            {
                return unlockedMinions;
            }

            Lumina.Excel.ExcelSheet<Companion>? minionSheet = Service.DataManager.GetExcelSheet<Companion>();
            if (minionSheet == null)
            {
                return unlockedMinions;
            }
            foreach (var row in minionSheet)
            {
                if (uiState->IsCompanionUnlocked(row.RowId))
                {
                    unlockedMinions.Add(row.RowId);
                }
            }

            return unlockedMinions;
        }
    }
}

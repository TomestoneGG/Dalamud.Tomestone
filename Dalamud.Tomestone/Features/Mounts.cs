using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dalamud.Tomestone.Features
{
    internal static class Mounts
    {
        internal static unsafe List<uint> GetUnlockedMounts(PlayerState* playerState)
        {
            List<uint> unlockedMounts = new List<uint>();

            // Ensure PlayerState is initialized
            if (playerState == null)
            {
                return unlockedMounts;
            }

            Lumina.Excel.ExcelSheet<Mount>? mountSheet = Service.DataManager.GetExcelSheet<Mount>();
            if (mountSheet == null)
            {
                return unlockedMounts;
            }
            foreach (var row in mountSheet)
            {
                if (playerState->IsMountUnlocked(row.RowId))
                {
                    unlockedMounts.Add(row.RowId);
                }
            }

            return unlockedMounts;
        }
    }
}

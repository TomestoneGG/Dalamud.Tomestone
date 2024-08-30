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
        internal static unsafe List<uint> GetUnlockedMinions(UIState* uiState, List<uint> cache)
        {
            List<uint> unlockedMinions = new List<uint>();

            foreach (var minionId in cache)
            {
                if (uiState->IsCompanionUnlocked(minionId))
                {
                    unlockedMinions.Add(minionId);
                }
            }

            return unlockedMinions;
        }
    }
}

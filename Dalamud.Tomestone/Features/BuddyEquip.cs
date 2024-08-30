using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.GeneratedSheets;
using System.Collections.Generic;

namespace Dalamud.Tomestone.Features
{
    internal static class BuddyEquip
    {
        internal static unsafe List<uint> GetChocoboBardings(UIState* UI, List<uint> cache)
        {
            var unlockedBuddyEquip = new List<uint>();

            foreach (var buddyEquipId in cache)
            {
                if (UI->Buddy.CompanionInfo.IsBuddyEquipUnlocked(buddyEquipId))
                {
                    unlockedBuddyEquip.Add(buddyEquipId);
                }
            }

            return unlockedBuddyEquip;
        }
    }
}

using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Tomestone.Models;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System;
using System.Linq;

namespace Dalamud.Tomestone.Features
{
    internal static class Gear
    {
        internal static unsafe Gearset GetGear(IPlayerCharacter localPlayer)
        {
            Gearset gear = new Gearset();
            try
            {
                // Grab the player's current job
                var currentJob = localPlayer.ClassJob.GetWithLanguage(Game.ClientLanguage.English);
                if (currentJob == null)
                {
                    throw new Exception("Failed to get current job name.");
                }
                gear.jobId = (uint)currentJob.RowId;

                // Grab the player's current gear (in a loop going through the ItemSlot enum)
                var lastSlot = Enum.GetValues(typeof(ItemSlot)).Cast<ItemSlot>().Last();
                for (int i = 0; i < (int)lastSlot; i++)
                {
                    try
                    {
                        var item = DalamudUtils.GetEquippedItem(i);
                        if (item == null)
                        {
                            continue;
                        }

                        // Add the item to the gearset
                        gear.items.Add(item);
                    }
                    catch
                    {
                        // Its possible that there is no item equipped in a slot
                        Service.Log.Verbose($"Failed to get item for slot {i}");
                    }
                }
            }
            catch
            {
                throw new Exception("Failed to get gear");
            }
            return gear;
        }
    }
}

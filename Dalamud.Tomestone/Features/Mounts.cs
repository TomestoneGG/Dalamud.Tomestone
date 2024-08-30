using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System.Collections.Generic;

namespace Dalamud.Tomestone.Features
{
    internal static class Mounts
    {
        internal static unsafe List<uint> GetUnlockedMounts(PlayerState* playerState, List<uint> cache)
        {
            List<uint> unlockedMounts = new List<uint>();
            foreach (var mountId in cache)
            {
                if (playerState->IsMountUnlocked(mountId))
                {
                    unlockedMounts.Add(mountId);
                }
            }

            return unlockedMounts;
        }
    }
}

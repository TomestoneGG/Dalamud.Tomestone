using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System.Collections.Generic;

namespace Dalamud.Tomestone.Features
{
    internal static class Orchestrion
    {
        internal static unsafe List<uint> GetOrchestrionRolls(PlayerState* pState, List<uint> cache)
        {
            var orchestrionRolls = new List<uint>();
            foreach (var orchestrionRollId in cache)
            {
                if (pState->IsOrchestrionRollUnlocked(orchestrionRollId))
                {
                    orchestrionRolls.Add(orchestrionRollId);
                }
            }

            return orchestrionRolls;
        }
    }
}

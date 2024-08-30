using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System.Collections.Generic;

namespace Dalamud.Tomestone.Features
{
    internal static class BlueMage
    {
        private unsafe static bool SpellUnlocked(uint unlockLink) => UIState.Instance()->IsUnlockLinkUnlockedOrQuestCompleted(unlockLink);

        public static List<uint> CheckLearnedSpells(List<AozCacheItem> cache)
        {
            var unlockedSpells = new List<uint>();

            foreach (var action in cache)
                if (SpellUnlocked(action.unlockLink))
                    unlockedSpells.Add(action.number);

            return unlockedSpells;
        }
    }
}

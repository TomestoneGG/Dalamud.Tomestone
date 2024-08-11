using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Dalamud.Tomestone.Features
{
    internal static class BlueMage
    {
        private unsafe static bool SpellUnlocked(uint unlockLink) => UIState.Instance()->IsUnlockLinkUnlockedOrQuestCompleted(unlockLink);

        public static List<uint> CheckLearnedSpells()
        {
            List<uint> unlockedSpells = new();

            List<AozAction> AozActionsCache = Service.DataManager.GetExcelSheet<AozAction>()!.Where(a => a.Rank != 0).ToList();
            List<AozActionTransient> AozTransientCache = Service.DataManager.GetExcelSheet<AozActionTransient>()!.Where(a => a.Number != 0).ToList();

            foreach (var (transient, action) in AozTransientCache.Zip(AozActionsCache).OrderBy(pair => pair.First.Number))
                if (SpellUnlocked(action.Action.Value!.UnlockLink))
                    unlockedSpells.Add(transient.Number);

            return unlockedSpells;
        }
    }
}

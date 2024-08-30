using Lumina.Excel.GeneratedSheets;
using System.Collections.Generic;
using System.Linq;

namespace Dalamud.Tomestone.Features
{
    internal class AozCacheItem
    { public uint unlockLink, number; }

    internal class CollectionCache
    {
        private static readonly uint[] BlockedItemIds = {
            24225 // Unlock book for Tomestone emote, unused.
        };

        private readonly List<Item> unlockLinkItems = new List<Item>();

        public readonly List<uint> BuddyEquip = new List<uint>();
        public readonly List<uint> Achievement = new List<uint>();
        public readonly List<uint> Minion = new List<uint>();
        public readonly List<uint> Mount = new List<uint>();
        public readonly List<uint> OrchestrionRoll = new List<uint>();
        public readonly List<uint> TripleTriadCard = new List<uint>();
        public readonly List<uint> Hairstyle = new List<uint>();    
        public readonly List<AozCacheItem> AozAction = new List<AozCacheItem>();

        public CollectionCache()
        {}

        public void LoadCache(bool force = false)
        {
            LoadBuddyEquip(force);
            LoadAchievements(force);
            LoadMinions(force);
            LoadMounts(force);
            LoadOrchestrionRolls(force);
            LoadTripleTriadCards(force);
            LoadHairstyles(force);
            LoadAozActions(force);

            LoadUnlockItems(force);
        }

        private void LoadBuddyEquip(bool force = false)
        {
            if (this.BuddyEquip.Count != 0 && !force) return;

            var buddyEquipSheet = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.BuddyEquip>();
            if (buddyEquipSheet == null) return;

            foreach (var row in buddyEquipSheet)
            {
                this.BuddyEquip.Add(row.RowId);
            }
        }

        private void LoadAchievements(bool force = false)
        {
            if (this.Achievement.Count != 0 && !force) return;

            var achievementSheet = Service.DataManager.GetExcelSheet<Achievement>();
            if (achievementSheet == null) return;

            foreach (var row in achievementSheet)
            {
                this.Achievement.Add(row.RowId);
            }
        }

        public void LoadMinions(bool force = false)
        {
            if (this.Minion.Count != 0 && !force) return;

            var minionSheet = Service.DataManager.GetExcelSheet<Companion>();
            if (minionSheet == null) return;

            foreach (var row in minionSheet)
            {
                this.Minion.Add(row.RowId);
            }
        }

        public void LoadMounts(bool force = false)
        {
            if (this.Mount.Count != 0 && !force) return;

            var mountSheet = Service.DataManager.GetExcelSheet<Mount>();
            if (mountSheet == null) return;

            foreach (var row in mountSheet)
            {
                this.Mount.Add(row.RowId);
            }
        }

        public void LoadOrchestrionRolls(bool force = false)
        {
            if (this.OrchestrionRoll.Count != 0 && !force) return;

            var orchestrionSheet = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets2.Orchestrion>();
            if (orchestrionSheet == null) return;

            foreach (var row in orchestrionSheet)
            {
                this.OrchestrionRoll.Add(row.RowId);
            }
        }

        public void LoadTripleTriadCards(bool force = false)
        {
            if (this.TripleTriadCard.Count != 0 && !force) return;

            var tripleTriadCardSheet = Service.DataManager.GetExcelSheet<TripleTriadCard>();
            if (tripleTriadCardSheet == null) return;

            foreach (var row in tripleTriadCardSheet)
            {
                this.TripleTriadCard.Add(row.RowId);
            }
        }

        private void LoadHairstyles(bool force = false)
        {
            if (this.Hairstyle.Count != 0 && !force) return;

            var charaMakeCustomizeSheet = Service.DataManager.GetExcelSheet<CharaMakeCustomize>();
            if (charaMakeCustomizeSheet == null) return;

            foreach (var charaMakeCustomize in charaMakeCustomizeSheet)
            {
                Hairstyle.Add(charaMakeCustomize.Data);
            }
        }

        public void LoadUnlockItems(bool force = false)
        {
            if (this.unlockLinkItems.Count != 0 && !force) return;

            // We need the ItemSheet for Hairstyles, since there seems to be no easy way to get them.
            var itemSheet = Service.DataManager.GetExcelSheet<Item>();
            if (itemSheet == null) return;

            foreach (var item in itemSheet)
            {
                var itemAction = item.ItemAction.Value;
                if (itemAction == null) continue;

                switch (itemAction.Type)
                {
                    case 0xA49:
                        // TODO: Check wether this is hairstyle or emote.
                        //this.Hairstyle.Add(itemAction.Data[0]);
                        this.unlockLinkItems.Add(item);
                        break;

                    default:
                        continue;
                }
            }
        }

        private void LoadAozActions(bool force = false)
        {
            if (this.AozAction.Count != 0 && !force) return;

            List<AozAction> AozActionsCache = Service.DataManager.GetExcelSheet<AozAction>()!.Where(a => a.Rank != 0).ToList();
            List<AozActionTransient> AozTransientCache = Service.DataManager.GetExcelSheet<AozActionTransient>()!.Where(a => a.Number != 0).ToList();

            foreach (var (transient, action) in AozTransientCache.Zip(AozActionsCache).OrderBy(pair => pair.First.Number))
            {
                AozAction.Add(new AozCacheItem { unlockLink = action.Action.Value!.UnlockLink, number = transient.Number });
            }
        }

        public Item? GetItemForUnlockLink(uint unlockLink)
        {
            var item = this.unlockLinkItems.Find(item => item.ItemAction.Value!.Data[0] == unlockLink);
            return item;
        }
    }
}

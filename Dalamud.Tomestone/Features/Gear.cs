using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Tomestone.Models;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dalamud.Tomestone.Features
{
    internal static class Gear
    {
        private static unsafe List<Models.Attribute> GetAttributes(Gearset gear, PlayerState* playerState)
        {
            var attributes = new List<Models.Attribute>();

            var playerAttributes = playerState->Attributes;

            // TODO: Check if we can clean up bufffood

            var crafterIds = new List<uint> { 8, 9, 10, 11, 12, 13, 14, 15 };
            var gathererIds = new List<uint> { 16, 17, 18 };
            // If the jobId is a crafter, we only need to collect craftsmanship, control, and CP
            if (crafterIds.Contains(gear.jobId))
            {
                
                attributes.Add(new Models.Attribute { name = "Craftsmanship", value = playerAttributes[70] });
                attributes.Add(new Models.Attribute { name = "Control", value = playerAttributes[71] });
                attributes.Add(new Models.Attribute { name = "CP", value = playerAttributes[11] });
            }
            else if (gathererIds.Contains(gear.jobId))
            {
                attributes.Add(new Models.Attribute { name = "Gathering", value = playerAttributes[72] });
                attributes.Add(new Models.Attribute { name = "Perception", value = playerAttributes[73] });
                attributes.Add(new Models.Attribute { name = "GP", value = playerAttributes[10] });
            }
            else
            { // If the jobId is a combat job
                // Get the player's base attributes
                attributes.Add(new Models.Attribute { name = "Strength", value = playerAttributes[1] });
                attributes.Add(new Models.Attribute { name = "Dexterity", value = playerAttributes[2] });
                attributes.Add(new Models.Attribute { name = "Vitality", value = playerAttributes[3] });
                attributes.Add(new Models.Attribute { name = "Intelligence", value = playerAttributes[4] });
                attributes.Add(new Models.Attribute { name = "Mind", value = playerAttributes[5] });
                attributes.Add(new Models.Attribute { name = "Piety", value = playerAttributes[6] });

                // Get the player's secondary attributes
                attributes.Add(new Models.Attribute { name = "Critical Hit Rate", value = playerAttributes[27] });
                attributes.Add(new Models.Attribute { name = "Determination", value = playerAttributes[44] });
                attributes.Add(new Models.Attribute { name = "Direct Hit Rate", value = playerAttributes[22] });
                attributes.Add(new Models.Attribute { name = "Skill Speed", value = playerAttributes[45] });
                attributes.Add(new Models.Attribute { name = "Spell Speed", value = playerAttributes[46] });
                attributes.Add(new Models.Attribute { name = "Tenacity", value = playerAttributes[19] });

                attributes.Add(new Models.Attribute { name = "HP", value = playerAttributes[7] });
                attributes.Add(new Models.Attribute { name = "MP", value = playerAttributes[8] });
                attributes.Add(new Models.Attribute { name = "Attack Power", value = playerAttributes[20] });
                attributes.Add(new Models.Attribute { name = "Attack Magic Potency", value = playerAttributes[33] });
                attributes.Add(new Models.Attribute { name = "Healing Magic Potency", value = playerAttributes[34] });
                attributes.Add(new Models.Attribute { name = "Defense", value = playerAttributes[21] });
                attributes.Add(new Models.Attribute { name = "Magic Defense", value = playerAttributes[24] });
            }

            return attributes;
        }            

        // TODO: Fix this up a bit so it grabs some data from our own structs
        internal static unsafe Gearset GetGear( IPlayerCharacter dalamudPlayer, PlayerState* playerState)
        {
            Gearset gear = new Gearset();
            try
            {
                // Grab the player's current job and level
                var currentJob = dalamudPlayer.ClassJob.GetWithLanguage(Game.ClientLanguage.English);
                if (currentJob == null)
                {
                    throw new Exception("Failed to get current job name.");
                }
                gear.jobId = (uint)currentJob.RowId;
                gear.jobLevel = (uint)dalamudPlayer.Level;

                // Grab the player's current gear (in a loop going through the ItemSlot enum)
                var lastSlot = Enum.GetValues(typeof(ItemSlot)).Cast<ItemSlot>().Last();
                bool hasShield = false;
                for (int i = 0; i <= (int)lastSlot; i++)
                {
                    try
                    {
                        var item = DalamudUtils.GetEquippedItem(i);
                        if (item == null)
                        {
                            continue;
                        }

                        if (item.slot == ItemSlot.OffHand)
                        {
                            hasShield = true;
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

                // If we have a shield, ensure it's added in the correct place (after feet)
                if (hasShield)
                {
                    var tempItems = new List<Item>();
                    var correctSlots = new List<ItemSlot> { ItemSlot.MainHand, ItemSlot.Head, ItemSlot.Body, ItemSlot.Hands, ItemSlot.Legs, ItemSlot.Feet, ItemSlot.OffHand, ItemSlot.Ears, ItemSlot.Neck, ItemSlot.Wrists, ItemSlot.LeftRing, ItemSlot.RightRing, ItemSlot.Crystals };
                    // Add the items in the correct order
                    foreach ( var slot in correctSlots)
                    {
                        var item = gear.items.Find(x => x.slot == slot);
                        if (item != null)
                        {
                            tempItems.Add(item);
                        }
                    }

                    gear.items = tempItems;
                }

                // Calculate the average item level
                gear.itemLevel = CalculateItemLevel(gear);  
                
                // Get the player's attributes
                gear.attributes = GetAttributes(gear, playerState);
            }
            catch
            {
                throw new Exception("Failed to get gear");
            }
            return gear;
        }

        private static uint CalculateItemLevel(Gearset gearset)
        {
            List<uint> iLvls = new List<uint>();
            bool hasOffhand = false;
            
            for (int i = 0; i < gearset.items.Count; i++)
            {
                if (gearset.items[i].slot == ItemSlot.OffHand)
                {
                    hasOffhand = true;
                }
            }

            // If the gearset has no offhand, we need to count the main hand twice
            if (!hasOffhand)
            {
                for (int i = 0; i < gearset.items.Count; i++)
                {
                    if (gearset.items[i].slot == ItemSlot.MainHand)
                    {
                        iLvls.Add(gearset.items[i].itemLevel);
                        iLvls.Add(gearset.items[i].itemLevel);
                    }
                    else
                    {
                        if (gearset.items[i].slot != ItemSlot.Crystals)
                            iLvls.Add(gearset.items[i].itemLevel);
                    }
                }
            }
            else
            {
                for (int i = 0; i < gearset.items.Count; i++)
                {
                    if (gearset.items[i].slot != ItemSlot.Crystals)
                        iLvls.Add(gearset.items[i].itemLevel);
                }
            }

            uint sum = 0;
            foreach (uint i in iLvls)
            {
                sum += i;
            }
            double itemLevel = sum / iLvls.Count;

            return (uint)Math.Round(itemLevel);
        }
    }
}

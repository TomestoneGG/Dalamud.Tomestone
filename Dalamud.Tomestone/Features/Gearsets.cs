using Dalamud.Tomestone.Models;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dalamud.Tomestone.Features
{
    internal static class Gearsets
    {
        internal static unsafe List<Gearset> GetGearsets()
        {
            List<Gearset> gearsets = new List<Gearset>();
            try
            {
                var gearsetModule = RaptureGearsetModule.Instance();
                if (gearsetModule == null)
                {
                    return gearsets;
                }

                int gsNum = gearsetModule->NumGearsets;

                // Iterate over all gearsets (0-99)
                for (int i = 0; i < gsNum; i++)
                {
                    var gearset = gearsetModule->GetGearset(i);

                    if (gearset == null)
                    {
                        return gearsets;
                    }

                    var jobId = gearset->ClassJob;
                    var itemLevel = (uint)gearset->ItemLevel;

                    // Check if the gearset is empty
                    if (jobId == 0 || itemLevel == 0 || gearset->Items == null || gearset->Items.Length == 0)
                    {
                        continue;
                    }

                    // Check if we already have a gearset for this job
                    if (gearsets.Any(g => g.jobId == jobId))
                    {
                        // Compare if the item level is higher
                        var existingGearset = gearsets.First(g => g.jobId == jobId);
                        if (existingGearset.itemLevel > itemLevel)
                        {
                            continue;
                        }

                        // Remove the existing gearset
                        gearsets.Remove(existingGearset);
                    }

                    // Create a new Gearset object
                    var gearsetObject = new Gearset
                    {
                        jobId = gearset->ClassJob,
                        itemLevel = (uint)gearset->ItemLevel,
                    };

                    // Get the items in the gearset
                    for (int j = 0; j < gearset->Items.Length; j++)
                    {
                        var itemData = gearset->Items[j];

                        // Skip empty items (for example, the offhand slot)
                        if (itemData.ItemId == 0)
                        {
                            continue;
                        }

                        // Clean up the item ID if the item is HQ (starts with 10, for examplke 10500 or 1042890)
                        var itemId = itemData.ItemId;
                        var itemIdString = itemData.ItemId.ToString();
                        var hq = false;         
                        if (itemIdString.StartsWith("10"))
                        {
                            hq = true;
                            itemIdString = itemIdString.Substring(2);
                            itemId = uint.Parse(itemIdString);
                        }

                        // Create a new Gear object
                        var item = new Models.Item
                        {
                            itemId = itemId,
                            hq = hq,
                            slot = (Models.ItemSlot)j, // GearsetModule uses this order, so we can use the index as the slot
                        };

                        // Check if the item has a glamour
                        if (itemData.GlamourId != 0)
                        {
                            // Check if the glamour is HQ
                            var glamourId = itemData.GlamourId;
                            var glamourIdString = itemData.GlamourId.ToString();
                            var glamourHq = false;
                            if (glamourIdString.StartsWith("10"))
                            {
                                glamourHq = true;
                                glamourIdString = glamourIdString.Substring(2);
                                glamourId = uint.Parse(glamourIdString);
                            }

                            // Create a new Glamour object
                            item.glamour = new Models.Glamour
                            {
                                itemId = glamourId,
                                hq = glamourHq,
                            };
                        }

                        // Check if the item has dyes
                        item.dye1 = itemData.Stain0Id;
                        item.dye2 = itemData.Stain1Id;

                        // Get the materia in the gear
                        var materiaArray = itemData.Materia;
                        var materiaGradeArray = itemData.MateriaGrades;
                        // Materia is a 5 element array ushort*
                        for (int k = 0; k < 5; k++)
                        {
                            // Get the materia
                            var materia = materiaArray[k];
                            if (materia == 0)
                            {
                                continue;
                            }

                            // If the item has no materia list, create a new list
                            // This way the materia entry will sent as "null" if there is no materia
                            if (item.materia == null)
                            {
                                item.materia = new List<Models.Materia>();
                            }

                            var materiaGrade = materiaGradeArray[k];

                            // Create a new Materia object
                            var materiaObject = new Models.Materia
                            {
                                materiaType = materia,
                                type = (Models.MateriaType)materia,
                                grade = (uint)materiaGrade + 1,
                                slot = (ushort)k,
                            };

                            // Add the materia to the gear
                            item.materia.Add(materiaObject);
                        }

                        // Add the gear to the gearset
                        gearsetObject.items.Add(item);
                    }

                    // Add the gearset to the list
                    gearsets.Add(gearsetObject);

                    // Debug
                    if (jobId == 42) { 
                    var gsJson = Newtonsoft.Json.JsonConvert.SerializeObject(gearsetObject, Newtonsoft.Json.Formatting.None);
                    Service.Log.Info($"Gearset: {gsJson}");
                    }
                }
            }
            catch (Exception ex)
            {
                Service.Log.Error(ex, "Failed to get gearsets.");
            }

            return gearsets;
        }
    }
}

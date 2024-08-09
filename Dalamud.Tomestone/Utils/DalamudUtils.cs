using Dalamud.Tomestone.Models;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;

namespace Dalamud.Tomestone
{
    internal static class DalamudUtils
    {
        // Get the equipped item in a specific slot
        internal static unsafe Item? GetEquippedItem(int index)
        {
            var im = InventoryManager.Instance();
            if (im == null)
                throw new Exception("InventoryManager was null");

            var equipped = im->GetInventoryContainer(InventoryType.EquippedItems);
            if (equipped == null)
                throw new Exception("EquippedItems was null");

            var slot = equipped->GetInventorySlot(index);
            if (slot == null)
                throw new Exception($"InventorySlot{index} was null");

            if (slot->ItemId == 0)
                return null;

            // Clean up the item ID if the item is HQ (starts with 10, for examplke 10500 or 1042890)
            var itemId = slot->ItemId;
            bool hq = slot->Flags == InventoryItem.ItemFlags.HighQuality;

            var item = new Item
            {
                itemId = itemId,
                hq = hq,
                slot = (ItemSlot)index,
            };

            // Check if the item has a glamour
            if (slot->GlamourId != 0)
            {
                // Check if the glamour is HQ
                var glamourId = slot->GlamourId;
                var glamourIdString = slot->GlamourId.ToString();
                var glamourHq = false;
                if (glamourIdString.StartsWith("10"))
                {
                    glamourHq = true;
                    glamourIdString = glamourIdString.Substring(2);
                    glamourId = uint.Parse(glamourIdString);
                }

                // Create a new Glamour object
                item.glamour = new Glamour
                {
                    itemId = glamourId,
                    hq = glamourHq,
                };
            }

            // Check if the item has dyes
            var stains = slot->Stains;
            if (stains[0] != 0 || stains[1] != 0)
            {
                item.dye1 = DalamudUtils.GetDyeName(stains[0]);
                item.dye2 = DalamudUtils.GetDyeName(stains[1]);
            }

            // Get the materia in the gear
            var materiaArray = slot->Materia;
            var materiaGradeArray = slot->MateriaGrades;
            // Materia is a 5 element array ushort*
            for (int j = 0; j < materiaArray.Length; j++) {

                

                // Get the materia
                var materia = materiaArray[j];
                if (materia == 0)
                {
                    continue;
                }

                var materiaGrade = materiaGradeArray[j];

                item.materia ??= new List<Materia>();

                item.materia.Add(new Materia
                {
                    id = GetMateriaID(materia, materiaGrade),
                    slot = (ushort)j,
                });
            }

            return item;
        }

        /// <summary>
        /// Get the materia ID from the materia type and grade using Lumina
        /// </summary>
        /// <param name="materiaType"></param>
        /// <param name="grade"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal static uint GetMateriaID(uint materiaType, uint grade)
        {
            var materiaSheet = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Materia>();
            if (materiaSheet == null)
            {
                throw new Exception("Failed to get materia sheet");
            }
            var materiaTypeRow = materiaSheet.GetRow(materiaType);
            if (materiaTypeRow == null)
            {
                throw new Exception("Failed to get materia type row");
            }
            var materiaGradeItem = materiaTypeRow?.Item[grade];
            if (materiaGradeItem == null)
            {
                throw new Exception("Failed to get materia grade item");
            }
            var materiaID = materiaGradeItem.Value;
            if (materiaID == null)
            {
                throw new Exception("Failed to get materia ID");
            }
            return materiaID.RowId;
        }

        /// <summary>
        /// Get the dye name from the dye type in english. Returns an empty string if the dye type is not found.
        /// </summary>
        /// <param name="dyeType"></param>
        /// <returns></returns>
        internal static string GetDyeName(uint dyeType)
        {
            var dyeSheet = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Stain>(Game.ClientLanguage.English);
            if (dyeSheet == null)
            {
                return string.Empty;
            }
            var dyeRow = dyeSheet.GetRow(dyeType);
            if (dyeRow == null)
            {
                return string.Empty;
            }

            // Check if the dye name is "No Color", if so, return an empty string
            var result = dyeRow.Name == "No Color" ? string.Empty : dyeRow.Name;

            return result;
        }
    }
}

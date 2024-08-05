using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dalamud.Tomestone.Features
{
    internal static class Achievements
    {
        internal static unsafe List<Models.Achievement> GetAchievements()
        {
            List<Models.Achievement> achievements = new List<Models.Achievement>();
            Lumina.Excel.ExcelSheet<Lumina.Excel.GeneratedSheets.Achievement>? achievementSheet = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Achievement>();
            if (achievementSheet == null)
            {
                return achievements;
            }

            var achievementState = FFXIVClientStructs.FFXIV.Client.Game.UI.Achievement.Instance();

            if (achievementState == null)
            {
                return achievements;
            }

            // Check if the achievement data is loaded
            bool loaded = achievementState->IsLoaded();
            if (!loaded)
            {
                Service.Log.Info("Achievement data is not loaded.");
                return achievements;
            }

            foreach (var row in achievementSheet)
            {

                if (achievementState->IsComplete((int)row.RowId))
                {
                    achievements.Add(new Models.Achievement
                    {
                        id = (uint)row.RowId,
                    });
                    continue;
                }
            }

            return achievements;
        }
    }
}

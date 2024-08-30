using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dalamud.Tomestone.Features
{
    internal static class Achievements
    {
        internal static unsafe List<Models.Achievement> GetAchievements(List<uint> cache)
        {
            List<Models.Achievement> achievements = new List<Models.Achievement>();

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

            foreach (var achievementId in cache)
            {

                if (achievementState->IsComplete((int)achievementId))
                {
                    achievements.Add(new Models.Achievement
                    {
                        id = (uint)achievementId,
                    });
                    continue;
                }
            }

            return achievements;
        }
    }
}

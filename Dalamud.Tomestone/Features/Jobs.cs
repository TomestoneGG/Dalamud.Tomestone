using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dalamud.Tomestone.Features
{
    internal static class Jobs
    {
        internal unsafe static List<Models.Job> GetJobs(PlayerState* playerState)
        {
            List<Models.Job> jobs = new List<Models.Job>();
            // Ensure PlayerState is initialized
            if (playerState == null)
            {
                throw new Exception("PlayerState is not initialized.");
            }

            // Iterate over the CLassJob Excel sheet 
            var classSheet = Service.DataManager.GetExcelSheet<ClassJob>();

            if (classSheet != null)
            {
                for (int i = 0; i < classSheet.RowCount; i++)
                {
                    var job = classSheet.GetRow(Convert.ToUInt32(i));
                    if (job == null || job.ExpArrayIndex < 0)
                    {
                        continue;
                    }

                    short value = playerState->ClassJobLevels[job.ExpArrayIndex];

                    if (value > 0)
                    {
                        jobs.Add(new Models.Job
                        {
                            id = (uint)i,
                            unlocked = value > 0,
                            level = value
                        });
                    }
                }
            }

            return jobs;
        }
    }
}

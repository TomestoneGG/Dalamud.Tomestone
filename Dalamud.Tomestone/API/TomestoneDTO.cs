using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dalamud.Tomestone.API
{
    internal class ActivityDTO
    {
        public uint jobId { get; set; } = 0;
        public uint jobLevel { get; set; } = 0;
        public uint territoryId { get; set; } = 0;
        public string currentWorld { get; set; } = string.Empty;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dalamud.Tomestone.Models
{
    internal class Job
    {
        public uint id { get; set; } = 0;
        // Defines if the job is unlocked
        public bool unlocked { get; set; } = false;
        // Can be a value from 0 to 100 (7.0 level cap)
        public short level { get; set; } = 0;
    }
}

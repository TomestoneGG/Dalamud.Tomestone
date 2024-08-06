using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dalamud.Tomestone.Models
{
    internal class Achievement
    {
        public uint id { get; set; } = 0;
        public DateTime timestamp { get; set; } = DateTime.MinValue;
    }
}

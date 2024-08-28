using System.Collections.Generic;

namespace Dalamud.Tomestone.API
{
    internal class ActivityDTO
    {
        public uint jobId { get; set; } = 0;
        public uint jobLevel { get; set; } = 0;
        public uint territoryId { get; set; } = 0;
        public uint placeNameId { get; set; } = 0;
        public uint subPlaceNameId { get; set; } = 0;
        public string currentWorld { get; set; } = string.Empty;
    }

    internal class TriadCardsDTO
    {
        public List<uint> cards { get; set; } = new List<uint>();
    }

    internal class OrchestrionDTO
    {
        public List<uint> rolls { get; set; } = new List<uint>();
    }

    internal class BlueMageDTO
    {
        public List<uint> spells { get; set; } = new List<uint>();
    }
}

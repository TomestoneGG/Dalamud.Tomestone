using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dalamud.Tomestone.Models
{
    internal class Player
    {
        public uint lodestoneId { get; set; } = 0;
        public string name { get; set; } = string.Empty;
        public string world { get; set; } = string.Empty;
        public uint currentJobId { get; set; } = 0;
        public uint currentJobLevel { get; set; } = 0;
        public uint currentZoneId { get; set; } = 0;
        public List<Job> jobs { get; set; } = new List<Job>();
        // List of unlocked minions
        public List<uint> minions { get; set; } = new List<uint>();
        // List of unlocked mounts
        public List<uint> mounts { get; set; } = new List<uint>();
        // List of unlocked achievements
        public List<Achievement> achievements { get; set; } = new List<Achievement>();
        // List of caught fish
        public List<uint> fish { get; set; } = new List<uint>();
        // TODO: List of unlocked orchestrion rolls
        // TODO: List of unlocked Blue Mage spells
        // TODO: List of unlocked Hairstyles
        // TODO: List of unlocked Armoire items
        // TODO: List of unlocked Fashion accessories
        // TODO: List of unlocked Emotes
        // TODO: List of unlocked Framers kits
        // TODO: List of obtained Triple Triad cards
        // TODO: List of completed Triple Triad NPCs
        // TODO: List of obtained Chocobo Bardings
        // TODO: Relic weapons
        // List of gearsets
        public List<Gearset> gearsets { get; set; } = new List<Gearset>();
    }

    // StreamData represents simple character data used for the tomestone.gg/streams page
    // This data will be sent every zone/job change
    internal class StreamData
    {
        public uint jobId { get; set; } = 0;
        public uint jobLevel { get; set; } = 0;
        public uint territoryId { get; set; } = 0;
        public string currentWorld { get; set; } = string.Empty;
    }

    internal class Job
    {
        public uint id { get; set; } = 0;
        // Defines if the job is unlocked
        public bool unlocked { get; set; } = false;
        // Can be a value from 0 to 100 (7.0 level cap)
        public short level { get; set; } = 0;
    }

    internal class Gearset
    {
        public uint jobId { get; set; } = 0;
        public uint itemLevel { get; set; } = 0;
        public List<Item> items { get; set; } = new List<Item>();
    }

    internal class Item
    {
        public uint itemId { get; set; } = 0;
        public List<Materia> materia { get; set; } = new List<Materia>();
    }

    internal class Materia
    {
        public uint type { get; set; } = 0;
        public uint grade { get; set; } = 0;
        public ushort slot { get; set; } = 0;
    }

    internal class Achievement
    {
        public uint id { get; set; } = 0;
        public DateTime timestamp { get; set; } = DateTime.MinValue;
    }
}

using System.Collections.Generic;

namespace Dalamud.Tomestone.API
{
    /// <summary>
    /// Represents a remote configuration retrieved from the server.
    /// These will disable certain features if they are set to false.
    /// </summary>
    internal class RemoteConfigDTO
    {
        // Data settings
        public bool enabled { get; set; } = true; // Whether sending data is enabled.
        public bool sendActivity { get; set; } = true; // Whether sending activity data is enabled.
        public bool sendGearSets { get; set; } = true; // Whether sending gear data is enabled.
        public bool sendTripleTriad { get; set; } = true; // Whether sending Triple Triad data is enabled.
        public bool sendOrchestrionRolls { get; set; } = true; // Whether sending Orchestrion data is enabled.
        public bool sendBlueMageSpells { get; set; } = true; // Whether sending Blue Mage data is enabled.
        public bool sendChocoboBardings { get; set; } = false; // Whether sending Chocobo Barding data is enabled.
        public int updateFrameworkInterval { get; set; } = 5; // How often to send framework updates in seconds.
        public int updateFullInterval { get; set; } = 1800; // How often to send a full update in seconds.
        // Plugin settings
        public bool uiModify { get; set; } = true; // Whether modifying the UI is enabled.
    }

    /// <summary>
    /// Represents a players activity sent to the server.
    /// </summary>
    internal class ActivityDTO
    {
        public uint jobId { get; set; } = 0;
        public uint jobLevel { get; set; } = 0;
        public uint territoryId { get; set; } = 0;
        public uint placeNameId { get; set; } = 0;
        public uint subPlaceNameId { get; set; } = 0;
        public string currentWorld { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a players owned Triple Triad Cards sent to the server.
    /// </summary>
    internal class TriadCardsDTO
    {
        public List<uint> cards { get; set; } = new List<uint>();
    }

    /// <summary>
    /// Represents a players owned Orchestrion Rolls sent to the server.
    /// </summary>
    internal class OrchestrionDTO
    {
        public List<uint> rolls { get; set; } = new List<uint>();
    }

    /// <summary>
    /// Represents a players unlocked Blue Mage spells sent to the server.
    /// </summary>
    internal class BlueMageDTO
    {
        public List<uint> spells { get; set; } = new List<uint>();
    }

    /// <summary>
    /// Represents a players owned Chocobo Barding sent to the server.
    /// </summary>
    internal class ChocoboBardingDTO
    {
        public List<uint> bardings { get; set; } = new List<uint>();
    }
}

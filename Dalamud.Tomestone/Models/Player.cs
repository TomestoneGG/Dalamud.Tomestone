using System;
using System.Collections.Generic;

/* Organization TODO's */
// TODO: Remove properties that are not needed (e.g. achievements, fish, minions, mounts - those will be fully sent to the server and don't need state tracking)
// TODO: Move gearsets, etc. to a seperate file
// TODO: Move DTOs to a seperate file

/* Feature TODO's */
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

namespace Dalamud.Tomestone.Models
{
    /// <summary>
    /// Represents a player in the game.
    /// We store all the data that needs to be tracked before sending it to the server here.
    /// </summary>
    internal class Player
    {
        public uint lodestoneId { get; set; } = 0;
        public string name { get; set; } = string.Empty; // Character name
        public string world { get; set; } = string.Empty; // Home world
        public uint currentJobId { get; set; } = 0;
        public uint currentJobLevel { get; set; } = 0;
        public uint currentZoneId { get; set; } = 0;  
        public uint areaPlaceNameId { get; set; } = 0; // Sub region ID
        public uint subAreaPlaceNameId { get; set; } = 0; // Sub sub region ID
        public string currentWorldName { get; set; } = string.Empty; // Current world name
    }
}

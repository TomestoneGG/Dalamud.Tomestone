using System;
using System.Collections.Generic;

/* Feature TODO's */
// TODO: List of unlocked Hairstyles
// TODO: List of unlocked Armoire items
// TODO: List of unlocked Fashion accessories
// TODO: List of unlocked Emotes
// TODO: List of unlocked Framers kits
// TODO: List of obtained Triple Triad cards
// TODO: List of completed Triple Triad NPCs
// TODO: List of obtained Chocobo Bardings

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
        public bool isLevelSynced { get; set; } = false; // Whether the player is level synced
        public object Clone()
        {
           return this.MemberwiseClone();
        }
    }
}

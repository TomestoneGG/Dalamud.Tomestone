using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Tomestone.API;
using System;
using System.Threading.Tasks;

namespace Dalamud.Tomestone.Features
{
    internal static class Player
    {
        internal async static Task<IPlayerCharacter?> GetLocalPlayer(Int16 maxWait)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            while (Service.ClientState.LocalPlayer == null && sw.ElapsedMilliseconds < maxWait)
            {
                await Task.Delay(100);
            }

            return Service.ClientState.LocalPlayer;
        }

        // Obtains all character information we can get using Dalamud
        internal unsafe static Models.Player GetCharacterInfo(IPlayerCharacter localPlayer)
        {
            var result = new Models.Player();
            result.name = localPlayer.Name.ToString();
            result.currentJobLevel = (uint)localPlayer.Level;

            var world = localPlayer.HomeWorld.GetWithLanguage(Game.ClientLanguage.English);
            if (world == null)
            {
                throw new Exception("Failed to get world name.");
            }
            result.world = world.Name.ToString();

            // Print out the player's current job and level
            var currentJob = localPlayer.ClassJob.GetWithLanguage(Game.ClientLanguage.English);
            if (currentJob == null)
            {
                throw new Exception("Failed to get current job name.");
            }
            result.currentJobId = (uint)currentJob.RowId;

            // Print out the players current location
            var currentTerritory = Service.ClientState.TerritoryType;
            result.currentZoneId = (uint)currentTerritory;

            string currentWorldName = string.Empty;
            // Check if the player is traveling to another world
            if (localPlayer.CurrentWorld != null)
            {
                var currentWorld = localPlayer.CurrentWorld.GetWithLanguage(Game.ClientLanguage.English);
                if (currentWorld == null)
                {
                    throw new Exception("Failed to get current world name.");
                }

                // Check if the player is traveling to another world, or if they are in the same world
                if (currentWorld.Name != world.Name)
                {
                    result.currentWorldName = currentWorld.Name.ToString().ToLower();
                }
            }

            return result;
        }
    }
}

using Dalamud.Tomestone.Models;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using NetStone;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dalamud.Tomestone
{
    internal class DataHandlerStatus
    {
        // Last update times
        public long basePlayerUpdate = 0;
        public long lodestoneUpdate = 0;
        public long mountUpdate = 0;
        public long minionUpdate = 0;
        public long achievementUpdate = 0;
        public long gearsetUpdate = 0;
        public long fishUpdate = 0;
        public long backendUpdate = 0;
    }

    internal class DataHandler
    {
        private Player player;
        private LodestoneClient? lodestoneClient;

        internal bool UpdateError = false;
        internal DateTime lastUpdate = DateTime.MinValue;
        internal string UpdateMessage = String.Empty;

        internal DataHandlerStatus status = new DataHandlerStatus();

        internal DataHandler()
        {
            // Initialize the player object
            player = new Player();
            // Initialize the lodestone client in the background
            var clientTask = Task.Run(() => InitLodestoneCient());
        }

        public async void Update()
        {

            // Check if the last update was less than 15 minutes ago
            if (DateTime.Now - lastUpdate < TimeSpan.FromMinutes(15))
            {
                Service.Log.Info("Skipping update, last update was less than 15 minutes ago.");
                return;
            }

            Service.Log.Info("Updating character data.");

            // Get the character data
            GetCharacterInfo();

            // Check if we have a lodestone ID, if not, get it
            if (player != null && player.lodestoneId == 0)
            {
                await GetCharacterFromLodestone();

                // TODO: Update the character data in the backend
                await SendCharacterData();
            }
        }

        // Obtains all character information we can get using Dalamud
        private unsafe void GetCharacterInfo()
        {
            // Init a stopwatch to measure the time it takes to get the data
            var stopwatch = new System.Diagnostics.Stopwatch();


            var localPlayer = Service.ClientState.LocalPlayer;

            if (localPlayer != null)
            {
                stopwatch.Start();

                // Get the player's name and world
                player.name = localPlayer.Name.ToString();
                var world = localPlayer.HomeWorld.GetWithLanguage(Game.ClientLanguage.English);
                player.world = world.Name.ToString();

                // Print out the player's name and world
                Service.Log.Debug($"Player: {localPlayer.Name} - {world.Name}");

                // Print out the player's current job and level
                var currentJob = localPlayer.ClassJob.GetWithLanguage(Game.ClientLanguage.English);
                Service.Log.Debug($"Level {localPlayer.Level} {currentJob.Name}");

                // Print out the players current location
                var currentTerritory = Service.ClientState.TerritoryType;
                Service.Log.Debug($"Current Territory: {currentTerritory}");

                // Get the player's jobs using the ClassJobLevelArray
                var playerState = PlayerState.Instance();
                var uiState = UIState.Instance();
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
                            Service.Log.Debug($"Job: {job.Name} Level: {value}");

                            player.jobs.Add(new Job
                            {
                                id = (uint)i,
                                unlocked = value > 0,
                                level = value
                            });
                        }
                    }
                }

                stopwatch.Stop();
                status.basePlayerUpdate = stopwatch.Elapsed.Microseconds;
                stopwatch.Restart();

                player.mounts = GetUnlockedMounts(playerState);
                Service.Log.Debug($"Player has {player.mounts.Count} mounts.");

                stopwatch.Stop();
                status.mountUpdate = stopwatch.Elapsed.Microseconds;
                stopwatch.Restart();

                player.minions = GetUnlockedMinions(uiState);
                Service.Log.Debug($"Player has {player.minions.Count} minions.");

                stopwatch.Stop();
                status.minionUpdate = stopwatch.Elapsed.Microseconds;
                stopwatch.Restart();

                player.achievements = GetAchievements();
                // We check this because the achievement data is not always loaded
                if (player.achievements.Count > 0)
                    Service.Log.Debug($"Player has {player.achievements.Count} achievements.");

                stopwatch.Stop();
                status.achievementUpdate = stopwatch.Elapsed.Microseconds;
                stopwatch.Restart();

                player.gearsets = GetGearsets();
                Service.Log.Debug($"Player has {player.gearsets.Count} gearsets.");

                stopwatch.Stop();
                status.gearsetUpdate = stopwatch.Elapsed.Microseconds;
                stopwatch.Restart();

                player.fish = GetFish(playerState);
                Service.Log.Debug($"Player has catched {player.fish.Count} fish.");

                stopwatch.Stop();
                status.fishUpdate = stopwatch.Elapsed.Microseconds;
            }
        }

        private unsafe List<uint> GetUnlockedMounts(PlayerState* playerState)
        {
            List<uint> unlockedMounts = new List<uint>();
            Lumina.Excel.ExcelSheet<Mount>? mountSheet = Service.DataManager.GetExcelSheet<Mount>();
            if (mountSheet == null)
            {
                return unlockedMounts;
            }
            foreach (var row in mountSheet)
            {
                if (playerState->IsMountUnlocked(row.RowId))
                {
                    unlockedMounts.Add(row.RowId);
                }
            }

            return unlockedMounts;
        }

        private unsafe List<uint> GetUnlockedMinions(UIState* uiState)
        {
            List<uint> unlockedMinions = new List<uint>();
            Lumina.Excel.ExcelSheet<Companion>? minionSheet = Service.DataManager.GetExcelSheet<Companion>();
            if (minionSheet == null)
            {
                return unlockedMinions;
            }
            foreach (var row in minionSheet)
            {
                if (uiState->IsCompanionUnlocked(row.RowId))
                {
                    unlockedMinions.Add(row.RowId);
                }
            }

            return unlockedMinions;
        }

        private unsafe List<uint> GetAchievements()
        {
            List<uint> achievements = new List<uint>();
            Lumina.Excel.ExcelSheet<Lumina.Excel.GeneratedSheets.Achievement>? achievementSheet = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Achievement>();
            var achievementState = FFXIVClientStructs.FFXIV.Client.Game.UI.Achievement.Instance();

            if (achievementState == null)
            {
                return achievements;
            }

            // Check if the achievement data is loaded
            bool loaded = achievementState->IsLoaded();
            if (!loaded)
            {
                Service.Log.Info("Achievement data is not loaded.");
                return achievements;
            }

            foreach (var row in achievementSheet)
            {
                if (achievementState->IsComplete((int)row.RowId))
                {
                    achievements.Add(row.RowId);
                    continue;
                }
            }

            return achievements;
        }

        private byte ReverseBits(byte b)
        {
            b = (byte)((b * 0x0202020202UL & 0x010884422010UL) % 1023);
            return b;
        }

        private unsafe List<uint> GetFish(PlayerState* playerState)
        {
            List<uint> fish = new List<uint>();

            // CaughtFishBitmask has a length of 159 
            var caughtFishBitmaskArray = playerState->CaughtFishBitmask;

            // Calculate the fish ID and add it to the list of fish caught
            for (int i = 0; i < 159; i++)
            {
                // Get the current byte
                byte currentByte = caughtFishBitmaskArray[i];

                // Reverse the bits in the current byte
                byte reversedByte = ReverseBits(currentByte);

                // Iterate over the bits in the byte
                for (int j = 0; j < 8; j++)
                {
                    // Check if the bit is set
                    if ((currentByte & (1 << j)) != 0)
                    {
                        // Calculate the fish ID
                        uint fishId = (uint)(i * 8 + j);

                        // Add the fish ID to the list of fish caught
                        fish.Add(fishId);
                    }
                }
            }

            return fish;
        }

        private unsafe List<Gearset> GetGearsets()
        {
            List<Gearset> gearsets = new List<Gearset>();

            try
            {
                var gearsetModule = RaptureGearsetModule.Instance();
                if (gearsetModule == null)
                {
                    return gearsets;
                }

                // Iterate over all gearsets (0-99)
                for (int i = 0; i < 99; i++)
                {
                    var gearset = gearsetModule->GetGearset(i);

                    if (gearset == null)
                    {
                        return gearsets;
                    }

                    var jobId = gearset->ClassJob;
                    var itemLevel = (uint)gearset->ItemLevel;

                    // Check if we already have a gearset for this job
                    if (gearsets.Any(g => g.jobId == jobId))
                    {
                        // Compare if the item level is higher
                        var existingGearset = gearsets.First(g => g.jobId == jobId);
                        if (existingGearset.itemLevel > itemLevel)
                        {
                            continue;
                        }

                        // Remove the existing gearset
                        gearsets.Remove(existingGearset);
                    }

                    // Create a new Gearset object
                    var gearsetObject = new Gearset
                    {
                        jobId = gearset->ClassJob,
                        itemLevel = (uint)gearset->ItemLevel,
                    };

                    // Get the items in the gearset
                    for (int j = 0; j < gearset->Items.Length; j++)
                    {
                        var itemData = gearset->Items[j];

                        // Skip empty items (for example, the offhand slot)
                        if (itemData.ItemId == 0)
                        {
                            continue;
                        }

                        // Create a new Gear object
                        var item = new Models.Item
                        {
                            itemId = itemData.ItemId,
                        };

                        // Get the materia in the gear
                        var materiaArray = itemData.Materia;
                        var materiaGradeArray = itemData.MateriaGrades;
                        // Materia is a 5 element array ushort*
                        for (int k = 0; k < 5; k++)
                        {
                            // Get the materia
                            var materia = materiaArray[k];
                            if (materia == 0)
                            {
                                continue;
                            }

                            var materiaGrade = materiaGradeArray[k];

                            // Create a new Materia object
                            var materiaObject = new Models.Materia
                            {
                                type = materia,
                                grade = materiaGrade,
                                slot = (ushort)k,
                            };

                            // Add the materia to the gear
                            item.materia.Add(materiaObject);
                        }

                        // Add the gear to the gearset
                        gearsetObject.items.Add(item);
                    }

                    // Add the gearset to the list
                    gearsets.Add(gearsetObject);
                }
            }
            catch (Exception ex)
            {
                Service.Log.Error(ex, "Failed to get gearsets.");
            }

            return gearsets;
        }


        private async void InitLodestoneCient()
        {
            try
            {
                lodestoneClient = await LodestoneClient.GetClientAsync();

                // Update the character data after the client is initialized
                Update();
            }
            catch (HttpRequestException ex)
            {
                lodestoneClient = null;
                Service.Log.Error(ex, "Failed to initialize Lodestone client.");
            }
        }

        private async Task GetCharacterFromLodestone()
        {
            Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            // Make sure the client is initialized
            if (lodestoneClient == null)
            {
                Service.Log.Error("Lodestone client is not yet initialized.");
                return;
            }

            try
            {
                var searchResponse = await lodestoneClient.SearchCharacter(new NetStone.Search.Character.CharacterSearchQuery()
                {
                    CharacterName = player.name,
                    World = player.world,
                });
                var lodestoneCharacter =
                    searchResponse?.Results
                    .FirstOrDefault(entry => entry.Name == player.name);
                // Convert the Lodestone ID to a uint
                player.lodestoneId = uint.Parse(lodestoneCharacter?.Id ?? "0");
            }
            catch (Exception ex)
            {
                // Just don't change the lodestoneId
                Service.Log.Error(ex, "Failed to get character from Lodestone.");
            }

            sw.Stop();
            status.lodestoneUpdate = sw.Elapsed.Microseconds;

        }

        private async Task SendCharacterData()
        {
            Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            // Safeguard against a missing lodestone ID
            if (player.lodestoneId == 0)
            {
                Service.Log.Error("Character data is missing the Lodestone ID.");
                return;
            }

            try
            {
                // TODO: Init a Singleton to handle Http Communication with the backend

                /*
                // Init a new HttpClient
                using var client = new HttpClient();
                // Serialize the player object to JSON
                var json = System.Text.Json.JsonSerializer.Serialize(player);
                // Create a new StringContent with the JSON
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                // Send the data to the server
                var response = await client.PostAsync("http://192.168.178.29:8080/updatePlayer", content);
                // Check if the response was successful
                if (!response.IsSuccessStatusCode)
                {
                    Service.Log.Error("Failed to send character data to the server.");
                    return;
                }
                */

                // Fake await to simulate the request
                await Task.Delay(1000);

                // Set the last update time to now
                UpdateError = false;
                lastUpdate = DateTime.Now;
                UpdateMessage = "Character data sent to the server.";
            }
            catch (Exception ex)
            {
                UpdateError = true;
                UpdateMessage = "Failed to send character data to the server.";
                Service.Log.Error(ex, "Failed to send character data to the server.");
            }

            sw.Stop();
            status.backendUpdate = sw.Elapsed.Microseconds;
        }
    }
}

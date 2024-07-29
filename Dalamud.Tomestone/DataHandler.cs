using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Tomestone.Models;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using NetStone;
using NetStone.Model.Parseables.Character.Achievement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Dalamud.Tomestone
{
    internal class DataHandlerStatus
    {
        // Current status
        public bool updating = false;
        // Last update status
        internal bool UpdateError = false;
        internal DateTime lastUpdate = DateTime.MinValue;
        internal string UpdateMessage = String.Empty;
    }

    internal class DataHandler
    {
        private Player player;
        private LodestoneClient? lodestoneClient;
        private Plugin plugin;

        private DateTime lastHandledFrameworkUpdate = DateTime.MinValue;

        internal unsafe PlayerState* playerState;
        internal unsafe UIState* uiState;

        internal HttpClient httpClient;
        internal DataHandlerStatus status = new DataHandlerStatus();

        internal DataHandler(Plugin _plugin)
        {
            plugin = _plugin;

            // Initialize the HttpClient
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(plugin.Configuration.BaseUrl);

            // Initialize the player object
            player = new Player();
            // Initialize the lodestone client in the background
            var clientTask = Task.Run(() => InitLodestoneCient());
        }

        public async void HandleFrameworkUpdate()
        {
            // Check if the last update was less than 5 seconds ago
            if (DateTime.Now - lastHandledFrameworkUpdate < TimeSpan.FromSeconds(5))
            {
                return;
            }

            lastHandledFrameworkUpdate = DateTime.Now;

            // Check if the player is in a loading screen
            if (Service.ClientState.LocalPlayer == null)
            {
                return;
            }

            // Grab the players current job and level, if it's different from the last update, send the data
            var currentJob = Service.ClientState.LocalPlayer.ClassJob.GetWithLanguage(Game.ClientLanguage.English);
            if (currentJob == null)
            {
                return;
            }

            // Check if the job or level changed
            if (player.currentJobId != (uint)currentJob.RowId || player.currentJobLevel != (uint)Service.ClientState.LocalPlayer.Level)
            {
                GetPlayerState();
                GetUIState();
                GetCharacterInfo(Service.ClientState.LocalPlayer);
            }
        }

        public async void Update()
        {
            status.updating = true;

            // Defer getting the local player in a retrying loop, since it's null in a loading screen
            var localPlayer = await GetLocalPlayer(5000);
            if (localPlayer == null)
            {
                status.updating = false;
                Service.Log.Error("Failed to get local player.");
                return;
            }

            GetPlayerState();
            GetUIState();

            // Grab character name, job, level and territory data to send to the backend
            GetCharacterInfo(localPlayer);

            // Check if the last update was less than 30 minutes ago
            if (DateTime.Now - status.lastUpdate < TimeSpan.FromMinutes(30))
            {
                Service.Log.Info("Skipping update, last update was less than 30 minutes ago.");
                return;
            }

            // Need API Specs for this, commented out for now
            // If yes, we can update the whole character data
            //Service.Log.Info("Updating character data.");

            //GetJobs();

            //player.gearsets = GetGearsets();
            //Service.Log.Debug($"Player has {player.gearsets.Count} gearsets.");

            //player.mounts = GetUnlockedMounts();
            //Service.Log.Debug($"Player has {player.mounts.Count} mounts.");

            //player.minions = GetUnlockedMinions();
            //Service.Log.Debug($"Player has {player.minions.Count} minions.");

            //player.fish = GetFish();
            //Service.Log.Debug($"Player has catched {player.fish.Count} fish.");

            //player.achievements = GetAchievements();
            //// We check this because the achievement data is not always loaded
            //if (player.achievements.Count > 0)
            //    Service.Log.Debug($"Player has {player.achievements.Count} achievements.");

            //// Check if we have a lodestone ID, if not, get it
            //if (player != null && player.lodestoneId == 0)
            //{

            //    await GetCharacterFromLodestone();

            //    // TODO: Update the character data in the backend
            //    await SendCharacterData();
            //}

            status.updating = false;
        }

        // Waits for <maxWait> ms to get the local player
        private async Task<IPlayerCharacter?> GetLocalPlayer(Int16 maxWait)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            while (Service.ClientState.LocalPlayer == null && sw.ElapsedMilliseconds < maxWait)
            {
                await Task.Delay(100);
            }

            // Log if we couldn't get the local player
            if (Service.ClientState.LocalPlayer == null)
            {
                Service.Log.Error("Failed to get local player.");
                return null;
            } else {
                return Service.ClientState.LocalPlayer;
            }
        }

        private unsafe void GetPlayerState()
        {
            this.playerState = PlayerState.Instance();
            if (this.playerState == null)
            {
                // Throw an exception if the player state is null
                throw new Exception("Failed to get player state.");
            }
        }

        private unsafe void GetUIState()
        {
            this.uiState = UIState.Instance();
            if (this.uiState == null)
            {
                // Throw an exception if the UI state is null
                throw new Exception("Failed to get UI state.");
            }
        }

        // Obtains all character information we can get using Dalamud
        private unsafe void GetCharacterInfo(IPlayerCharacter localPlayer)
        {
            player.name = localPlayer.Name.ToString();
            player.currentJobLevel = (uint)localPlayer.Level;

            var world = localPlayer.HomeWorld.GetWithLanguage(Game.ClientLanguage.English);
            if (world == null)
            {
                Service.Log.Error("Failed to get world name.");
                return;
            }
            player.world = world.Name.ToString();

            // Print out the player's current job and level
            var currentJob = localPlayer.ClassJob.GetWithLanguage(Game.ClientLanguage.English);
            if (currentJob == null)
            {
                Service.Log.Error("Failed to get current job.");
                return;
            }
            player.currentJobId = (uint)currentJob.RowId;

            // Print out the players current location
            var currentTerritory = Service.ClientState.TerritoryType;
            player.currentZoneId = (uint)currentTerritory;

            // Create a new StreamData object
            var streamData = new StreamData
            {
                jobId = player.currentJobId,
                jobLevel = player.currentJobLevel,
                zoneId = player.currentZoneId,
            };

            // Send the stream data to the server, this is a fire and forget operation so we don't await it
            var streamTask = Task.Run(() => SendStreamData(streamData));
        }

        private unsafe void GetJobs()
        {
            // Ensure PlayerState is initialized
            if (this.playerState == null)
            {
                return;
            }

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

                    short value = this.playerState->ClassJobLevels[job.ExpArrayIndex];

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
        }

        private unsafe List<uint> GetUnlockedMounts()
        {
            List<uint> unlockedMounts = new List<uint>();

            // Ensure PlayerState is initialized
            if (this.playerState == null)
            {
                return unlockedMounts;
            }
      
            Lumina.Excel.ExcelSheet<Mount>? mountSheet = Service.DataManager.GetExcelSheet<Mount>();
            if (mountSheet == null)
            {
                return unlockedMounts;
            }
            foreach (var row in mountSheet)
            {
                if (this.playerState->IsMountUnlocked(row.RowId))
                {
                    unlockedMounts.Add(row.RowId);
                }
            }

            return unlockedMounts;
        }

        private unsafe List<uint> GetUnlockedMinions()
        {
            List<uint> unlockedMinions = new List<uint>();
            // Ensure UIState is initialized
            if (this.uiState == null)
            {
                return unlockedMinions;
            }

            Lumina.Excel.ExcelSheet<Companion>? minionSheet = Service.DataManager.GetExcelSheet<Companion>();
            if (minionSheet == null)
            {
                return unlockedMinions;
            }
            foreach (var row in minionSheet)
            {
                if (this.uiState->IsCompanionUnlocked(row.RowId))
                {
                    unlockedMinions.Add(row.RowId);
                }
            }

            return unlockedMinions;
        }

        private unsafe List<Models.Achievement> GetAchievements()
        {
            List<Models.Achievement> achievements = new List<Models.Achievement>();
            Lumina.Excel.ExcelSheet<Lumina.Excel.GeneratedSheets.Achievement>? achievementSheet = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Achievement>();
            if (achievementSheet == null)
            {
                return achievements;
            }

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
                    achievements.Add(new Models.Achievement
                    {
                        id = (uint)row.RowId,
                    });
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

        private unsafe List<uint> GetFish()
        {
            List<uint> fish = new List<uint>();
            // Ensure PlayerState is initialized
            if (this.playerState == null)
            {
                return fish;
            }

            // CaughtFishBitmask has a length of 159 
            var caughtFishBitmaskArray = this.playerState->CaughtFishBitmask;

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
            }
            catch (HttpRequestException ex)
            {
                lodestoneClient = null;
                Service.Log.Error(ex, "Failed to initialize Lodestone client.");
            }
        }

        private async Task GetCharacterFromLodestone()
        {
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
                var characterSearchEntry =
                    searchResponse?.Results
                    .FirstOrDefault(entry => entry.Name == player.name);

                if (characterSearchEntry == null)
                {
                    Service.Log.Error("Failed to find character on Lodestone.");
                    return;
                };

                // Convert the Lodestone ID to a uint
                player.lodestoneId = uint.Parse(characterSearchEntry.Id ?? "0");

                // Check if the characters achievements are public, and the achievement data is loaded
                var lsCharacter = await characterSearchEntry.GetCharacter();
                if (lsCharacter == null)
                {
                    Service.Log.Error("Character could not be loaded from lodestone.");
                    return;
                }

                var achievementPage = await lsCharacter.GetAchievement();
                if (achievementPage == null)
                {
                    Service.Log.Error("Achievement data could not be loaded from lodestone.");
                    return;
                }

                var achievements = new List<CharacterAchievementEntry>();
                achievements.AddRange(achievementPage.Achievements);
                for (int i = 1; i < achievementPage.NumPages; i++)
                {
                    achievementPage = await achievementPage.GetNextPage();
                    if (achievementPage == null)
                    {
                        // TODO: Check if we could continue here, for now we just break for safety
                        break;
                    }
                    achievements.AddRange(achievementPage.Achievements);
                }

                // Enrich player achievements with the Lodestone data (timestamps, mostly)
                foreach (var achievement in player.achievements)
                {
                    var lodestoneAchievement = achievements.FirstOrDefault(a => a.Id == achievement.id);
                    if (lodestoneAchievement == null)
                    {
                        continue;
                    }

                    achievement.timestamp = lodestoneAchievement.TimeAchieved;
                }
            }
            catch (Exception ex)
            {
                // Just don't change the lodestoneId
                Service.Log.Error(ex, "Failed to get character from Lodestone.");
            }
        }

        private async Task SendStreamData(StreamData data)
        {
            // TODO: Check this way earlier
            // Make sure we have a API key
            if (string.IsNullOrEmpty(plugin.Configuration.ApiKey))
            {
                Service.Log.Error("API key is missing.");
                return;
            }

            // Build the Header
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {plugin.Configuration.ApiKey}");

            // Serialize the player object to JSON
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            // Create a new StringContent with the JSON
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // DEBUG: Log the JSON and Headers
            Service.Log.Debug($"Sending Headers: {httpClient.DefaultRequestHeaders}");
            Service.Log.Debug($"Sending JSON: {json}");           

            // Send the data to the server
            var response = await this.httpClient.PostAsync(plugin.Configuration.StreamPath, content);
            // Check if the response was successful
            if (!response.IsSuccessStatusCode)
            {
                Service.Log.Error("Failed to send stream data to the server.");
                return;
            }
        }

        private async Task SendCharacterData()
        {
            // Safeguard against a missing lodestone ID
            if (player.lodestoneId == 0)
            {
                Service.Log.Error("Character data is missing the Lodestone ID.");
                return;
            }

            try
            {
                // TODO: Send data

                // Fake await to simulate the request
                await Task.Delay(1000);

                // Set the last update time to now
                status.UpdateError = false;
                status.lastUpdate = DateTime.Now;
                status.UpdateMessage = "Character data sent to the server.";
            }
            catch (Exception ex)
            {
                status.UpdateError = true;
                status.UpdateMessage = "Failed to send character data to the server.";
                Service.Log.Error(ex, "Failed to send character data to the server.");
            }
        }
    }
}

using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Tomestone.API;
using Dalamud.Tomestone.Models;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using NetStone;
using NetStone.Model.Parseables.Character.Achievement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

    /// <summary>
    /// DataHandler acts as a control plane for the data collection and sending process.
    /// It manages timers and instructs the collection and sending of data, as well as handling and displaying errors.
    /// This is made so we can split off dalamud/api specific code from error handling and data processing.
    /// </summary>
    internal class DataHandler
    {
        private Player player; // Player object to store character state
        private LodestoneClient? lodestoneClient; // Lodestone client to fetch character data, eventually not needed
        private Tomestone plugin; // Reference to the plugin

        private DateTime lastHandledFrameworkUpdate = DateTime.MinValue;
        private bool updateRequested = false;

        internal unsafe PlayerState* playerState;
        internal unsafe UIState* uiState;

        internal TomestoneAPI api;
        internal DataHandlerStatus status = new DataHandlerStatus();

        internal DataHandler(Tomestone _plugin)
        {
            plugin = _plugin;

            // Initialize the API Client
            api = new TomestoneAPI(plugin.Configuration);

            // Initialize the player object
            player = new Player();
            // Initialize the lodestone client in the background//Hopefully this will be removed in the future
            var clientTask = Task.Run(() => InitLodestoneCient());
        }

        public void ScheduleUpdate()
        {
            updateRequested = true;
        }

        public void HandleFrameworkUpdate(IPlayerCharacter localPlayer)
        {
            // Check if the last update was less than 5 seconds ago
            if (DateTime.Now - lastHandledFrameworkUpdate < TimeSpan.FromSeconds(5))
            {
                return;
            }

            lastHandledFrameworkUpdate = DateTime.Now;

            // Check if the player is in a loading screen
            if (localPlayer == null)
            {
                return;
            }

            // Grab the players current job and level, if it's different from the last update, send the data
            var currentJob = localPlayer.ClassJob.GetWithLanguage(Game.ClientLanguage.English);
            if (currentJob == null)
            {
                return;
            }

            // Check if the job or level changed
            if (player.currentJobId != (uint)currentJob.RowId || player.currentJobLevel != (uint)localPlayer.Level)
            {
                GetPlayerState();
                GetUIState();
                // Grab character name, job, level territory and traveling data to send to the backend
                var playerData = Features.Player.GetCharacterInfo(localPlayer);

                // Check for changes in the player state and send if needed
                HandlePlayerState(playerData);
            }

            // Check if an update was requested
            if (updateRequested)
            {
                updateRequested = false;
                Update(localPlayer);
            }
        }

        public void Update(IPlayerCharacter localPlayer)
        {

            GetPlayerState();
            GetUIState();

            // Grab character name, job, level territory and traveling data to send to the backend
            var playerData = Features.Player.GetCharacterInfo(localPlayer);

            // Check for changes in the player state and send if needed
            HandlePlayerState(playerData);

            // Update all data that might change frequently (this isn't in the 30 minute update check)
            HandleGearState(localPlayer);

            // Check if the last update was less than 30 minutes ago
            if (DateTime.Now - status.lastUpdate < TimeSpan.FromMinutes(30))
            {
                Service.Log.Info("Skipping update, last update was less than 30 minutes ago.");
                return;
            }

            // These aren't in the official Plugin yet
#if DEBUG
            // Need API Specs for this, commented out for now
            // If yes, we can update the whole character data
            //Service.Log.Info("Updating character data.");

            HandleTripleTriadState();

            HandleOrchestrionState();

            HandleBlueMageState();

            HandleJobState();

            HandleMountState();

            HandleMinionState();

            HandleAchievementState();

            HandleFishState();         

            // HandleGearsetState();
#endif
            // Set the last update time to now
            status.lastUpdate = DateTime.Now;
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

        private void DoRequest(Func<Task<APIError?>> request)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    var error = await request();
                    if (error != null)
                    {
                        Service.Log.Error(error.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    Service.Log.Error(ex, "Failed to update data.");
                }
                finally
                {
                    // Clean up
                }
            });
        }

        // Handles base player data and activity data
        private unsafe void HandlePlayerState(Models.Player newPlayer)
        {
            var changed = false;
            // Check if the player object is null, if so, set it and send initial activity data
            if (this.player == null)
            {
                this.player = newPlayer;
                changed = true;
            }
            // Check if the player state (job, level, zone, current world) has changed
            else if (this.player.currentJobId != newPlayer.currentJobId || this.player.currentJobLevel != newPlayer.currentJobLevel || this.player.currentZoneId != newPlayer.currentZoneId || this.player.currentWorldName != newPlayer.currentWorldName)
            {
                this.player = newPlayer;
                changed = true;
            }

            if (changed && this.plugin.Configuration.SendActivity)
            {
                var activity = new ActivityDTO
                {
                    jobId = newPlayer.currentJobId,
                    jobLevel = newPlayer.currentJobLevel,
                    territoryId = newPlayer.currentZoneId,
                    currentWorld = newPlayer.currentWorldName,
                };
                // Send the stream data to the server, this is a fire and forget operation so we don't await it
                DoRequest(() => api.SendActivity(player.name, player.world, activity));
            }
        }

        private unsafe void HandleJobState()
        {
            // Get the jobs from the player state
            var jobs = Features.Jobs.GetJobs(this.playerState);
            if (jobs == null)
            {
                return;
            }

            // Check if we have any jobs
            if (jobs.Count == 0)
            {
                return;
            }

            // TODO: Send job data to the server
            Service.Log.Debug($"Player has {jobs.Count} jobs.");
        }

        private unsafe void HandleTripleTriadState()
        {
            try {                 
                // Get the triple triad cards from the player state
                var cards = Features.TripleTriad.GetTripleTriadCards(this.uiState);

                Service.Log.Debug($"Player has {cards.Count} Triple Triad cards.");
            }
            catch (Exception ex)
            {
                Service.Log.Error(ex, "Failed to get Triple Triad cards.");
            }
        }

        private unsafe void HandleOrchestrionState()
        {
            try {
                // Get the orchestrion rolls from the player state
                var rolls = Features.Orchestrion.GetOrchestrionRolls(this.playerState);

                Service.Log.Debug($"Player has {rolls.Count} Orchestrion rolls.");
            }
            catch (Exception ex)
            {
                Service.Log.Error(ex, "Failed to get Orchestrion rolls.");
            }
        }

        private unsafe void HandleBlueMageState()
        {
            try
            {
                // Get the blue mage spells from the player state
                var spells = Features.BlueMage.CheckLearnedSpells();

                Service.Log.Debug($"Player has {spells.Count} Blue Mage spells.");
            }
            catch (Exception ex)
            {
                Service.Log.Error(ex, "Failed to get Blue Mage spells.");
            }
        }

        private unsafe void HandleMountState()
        {
            // Get the mounts from the player state
            var mounts = Features.Mounts.GetUnlockedMounts(this.playerState);
            if (mounts == null)
            {
                return;
            }

            // Check if we have any mounts
            if (mounts.Count == 0)
            {
                return;
            }

            // TODO: Send mount data to the server
            Service.Log.Debug($"Player has {mounts.Count} mounts.");
        }

        private unsafe void HandleMinionState()
        {
            // Get the minions from the ui state
            var minions = Features.Minions.GetUnlockedMinions(this.uiState);
            if (minions == null)
            {
                return;
            }

            // Check if we have any minions
            if (minions.Count == 0)
            {
                return;
            }

            // TODO: Send minion data to the server
            Service.Log.Debug($"Player has {minions.Count} minions.");
        }

        private unsafe void HandleAchievementState()
        {
            // Get the achievements from the player state
            var achievements = Features.Achievements.GetAchievements();
            if (achievements == null)
            {
                return;
            }

            // Check if we have any achievements
            if (achievements.Count == 0)
            {
                return;
            }

            // TODO: Send achievement data to the server
            Service.Log.Debug($"Player has {achievements.Count} achievements.");
        }

        private unsafe void HandleFishState()
        {
            // Get the fish from the player state
            var fish = Features.Fish.GetFish(this.playerState);
            if (fish == null)
            {
                return;
            }

            // Check if we have any fish
            if (fish.Count == 0)
            {
                return;
            }

            // TODO: Send fish data to the server
            Service.Log.Debug($"Player has catched {fish.Count} fish.");
        }

        private unsafe void HandleGearState(IPlayerCharacter localPlayer)
        {
            if (this.plugin.Configuration.Enabled == false || this.plugin.Configuration.SendGear == false)
            {
                return;
            }

            // Get the gearsets from the player state
            var gear = Features.Gear.GetGear(localPlayer, this.playerState);
            if (gear == null)
            {
                return;
            }

            // Send gear data to the server
            DoRequest(() => api.SendGear(player.name, player.world, gear));
        }

        private unsafe void HandleGearsetState()
        {
            // Get the gearsets from the player state
            var gearsets = Features.Gearsets.GetGearsets();
            if (gearsets == null)
            {
                return;
            }

            // Check if we have any gearsets
            if (gearsets.Count == 0)
            {
                return;
            }

            // TODO: Send gearset data to the server
            Service.Log.Debug($"Player has {gearsets.Count} gearsets.");
        }

        private void GetBluSpells()
        {
            try
            {
                //TODO: Maybe a ref here... https://github.com/DiggityMan420/BluDex/blob/master/BluDex/PluginAddressResolver.cs
            }
            catch (Exception ex)
            {
                Service.Log.Error(ex, "Failed to get Blue Mage spells.");
            }
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
                /* TODO: FIX THIS
                foreach (var achievement in player.achievements)
                {
                    var lodestoneAchievement = achievements.FirstOrDefault(a => a.Id == achievement.id);
                    if (lodestoneAchievement == null)
                    {
                        continue;
                    }

                    achievement.timestamp = lodestoneAchievement.TimeAchieved;
                }
                */
            }
            catch (Exception ex)
            {
                // Just don't change the lodestoneId
                Service.Log.Error(ex, "Failed to get character from Lodestone.");
            }
        }
    }
}

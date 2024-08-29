using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Tomestone.API;
using Dalamud.Tomestone.Models;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
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
        internal DateTime lastUpdate = DateTime.MinValue;
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

        public void HandleFrameworkUpdate(IPlayerCharacter? localPlayer)
        {
            // Check if the last update was less than 5 seconds ago
            if (DateTime.Now - lastHandledFrameworkUpdate < TimeSpan.FromSeconds(plugin.Configuration.RemoteConfig.updateFrameworkInterval))
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

            GetPlayerState();
            GetUIState();

            var playerData = Features.Player.GetCharacterInfo(player, localPlayer);
            if (!plugin.Configuration.TokenChecked)
            {
                if (plugin.Configuration.DalamudToken == null || plugin.Configuration.DalamudToken == "")
                {
                    return;
                }
                Service.Log.Info("Token not checked, checking now.");

                // Check if the token is valid by sending activity data
                HandleTokenChange(playerData);
            }
            else if (player.currentJobId != (uint)currentJob.RowId || player.currentJobLevel != (uint)localPlayer.Level || player.areaPlaceNameId != playerData.areaPlaceNameId || player.subAreaPlaceNameId != playerData.subAreaPlaceNameId)
            {
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

        /// <summary>
        /// This just prints the current content ID
        /// TODO: This should return true/false if the content causes our gear to iLvl sync
        /// </summary>
        //private unsafe void CheckContent()
        //{
        //    var contentDirector = EventFramework.Instance()->GetContentDirector();
        //    if (contentDirector == null)
        //    {
        //        return;
        //    }

        //    var id = contentDirector->ContentId;
        //    Service.Log.Info($"Content ID: {id}");
        //}

        public void Update(IPlayerCharacter localPlayer)
        {
            // Check if the plugin is enabled
            if (!plugin.Configuration.Enabled || !plugin.Configuration.RemoteConfig.enabled)
            {
                return;
            }

            GetPlayerState();
            GetUIState();

            // Grab character name, job, level territory and traveling data to send to the backend
            var playerData = Features.Player.GetCharacterInfo(player, localPlayer);

            // Check for changes in the player state and send if needed
            HandlePlayerState(playerData);

            // Update all data that might change frequently (this isn't in the 30 minute update check)
            HandleGearState(localPlayer);

            // Check if the last update was less than 30 minutes ago
            if (DateTime.Now - status.lastUpdate < TimeSpan.FromSeconds(plugin.Configuration.RemoteConfig.updateFullInterval))
            {
                Service.Log.Info($"Skipping collections update, last update was less than {plugin.Configuration.RemoteConfig.updateFullInterval} seconds ago.");
                return;
            }

            // Update all data that might change less frequently
            HandleTripleTriadState();
            HandleOrchestrionState();
            HandleBlueMageState();
#if DEBUG
            // These aren't in the official Plugin yet (lack an endpoint)
            HandleJobState();
            HandleMountState();
            HandleMinionState();
            HandleAchievementState();
            HandleFishState();
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

            // Check if the player is currently level synced
            this.player.isLevelSynced = Convert.ToBoolean(this.playerState->IsLevelSynced);
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


        // This exists so we can handle if the token changed and give some feedback to the user
        private unsafe void HandleTokenChange(Models.Player newPlayer)
        {
            // We do the same as in HandlePlayerState, but we don't care if the data changed or not
            this.player = newPlayer;
            var activity = new ActivityDTO
            {
                jobId = newPlayer.currentJobId,
                jobLevel = newPlayer.currentJobLevel,
                territoryId = newPlayer.currentZoneId,
                currentWorld = newPlayer.currentWorldName,
            };

            // Send the stream data to the server, this is a fire and forget operation so we don't await it
            api.DoRequest(() => api.SendActivity(player.name, player.world, activity));
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

            // TODO: This is kind of double-checking, clean up a bit
            // Check if the player state (job, level, zone, current world) has changed
            else if (this.player.currentJobId != newPlayer.currentJobId || this.player.currentJobLevel != newPlayer.currentJobLevel || this.player.currentZoneId != newPlayer.currentZoneId || this.player.currentWorldName != newPlayer.currentWorldName || this.player.areaPlaceNameId != newPlayer.areaPlaceNameId || this.player.subAreaPlaceNameId != newPlayer.subAreaPlaceNameId)
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
                    placeNameId = newPlayer.areaPlaceNameId,
                    subPlaceNameId = newPlayer.subAreaPlaceNameId,
                };
                // Send the stream data to the server, this is a fire and forget operation so we don't await it
                api.DoRequest(() => api.SendActivity(player.name, player.world, activity));
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
            try
            {
                if (!plugin.Configuration.SendTriad || !plugin.Configuration.RemoteConfig.sendTripleTriad)
                {
                    return;
                }

                // Get the triple triad cards from the player state
                var cards = Features.TripleTriad.GetTripleTriadCards(this.uiState);

                if (cards == null)
                {
                    return;
                }

                // Build a DTO object with the card IDs
                var cardDTO = new TriadCardsDTO
                {
                    cards = cards
                };

                // Send the card data to the server
                api.DoRequest(() => api.SendTriadCards(player.name, player.world, cardDTO));
            }
            catch (Exception ex)
            {
                Service.Log.Error(ex, "Failed to get Triple Triad cards.");
            }
        }

        private unsafe void HandleOrchestrionState()
        {
            try
            {
                if (!plugin.Configuration.SendOrchestrion || !plugin.Configuration.RemoteConfig.sendOrchestrionRolls)
                {
                    return;
                }

                // Get the orchestrion rolls from the player state
                var rolls = Features.Orchestrion.GetOrchestrionRolls(this.playerState);

                if (rolls == null)
                {
                    return;
                }

                // Build a DTO object with the roll IDs
                var rollDTO = new OrchestrionDTO
                {
                    rolls = rolls
                };

                // Send the roll data to the server
                api.DoRequest(() => api.SendOrchestrionRolls(player.name, player.world, rollDTO));
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
                if (!plugin.Configuration.SendBlueMage || !plugin.Configuration.RemoteConfig.sendBlueMageSpells)
                {
                    return;
                }

                // Get the blue mage spells from the player state
                var spells = Features.BlueMage.CheckLearnedSpells();

                if (spells == null)
                {
                    return;
                }

                // Build a DTO object with the spell IDs
                var spellDTO = new BlueMageDTO
                {
                    spells = spells
                };

                // Send the spell data to the server
                api.DoRequest(() => api.SendBlueMageSpells(player.name, player.world, spellDTO));
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
            if (player.isLevelSynced || !plugin.Configuration.SendGear || !plugin.Configuration.RemoteConfig.sendGearSets)
            {
                return;
            }

            // Ensure we aren't in a PvP area, since this would mess up attribute data
            if (Service.ClientState.IsPvP || this.player.currentZoneId == 250)
            {
                Service.Log.Debug("Player is in a PvP area, skipping gear update.");
                return;
            }

            // TODO: Ensure we aren't level synced, since this would mess up attribute data
            // TOOD: Ensure we aren't in Bozja/Eureka/Deep Dungeon, since this would mess up attribute data

            // Get the gearsets from the player state
            var gear = Features.Gear.GetGear(localPlayer, this.playerState);
            if (gear == null)
            {
                return;
            }

            // Send gear data to the server
            api.DoRequest(() => api.SendGear(player.name, player.world, gear));
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

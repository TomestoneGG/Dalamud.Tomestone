using Dalamud.Tomestone.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dalamud.Tomestone.API
{
    internal class APIError
    {
        public bool IsError { get; set; } = false;
        public APIErrorType ErrorType { get; set; } = APIErrorType.UnknownError;
        public string ErrorMessage { get; set; } = "";
    }

    internal enum APIErrorType
    {
        UnknownError,
        GenericError,
        InvalidToken,
        UnclaimedCharacter,
    }

    internal class TomestoneAPI
    {
        private static readonly string API_URL = "https://tomestone.gg/api/dalamud";

        private HttpClient client;
        private Configuration c;

        public TomestoneAPI(Configuration c)
        {
            this.client = new HttpClient();
            this.c = c; // Reference to the configuration, so we can get the API key
        }

        private void HandleAPIError(APIError? e)
        {
            if (e == null) {
                return;
            } else
            {
                if (e.IsError)
                {
                    // Handle specific error types
                    if (e.ErrorType == APIErrorType.InvalidToken)
                    {
                        this.c.TokenChecked = true;
                        this.c.TokenValid = false;
                        this.c.CharacterClaimed = true; // We will assume the character is claimed, as we can't check it. This will be set to false once the token is valid.
                        this.c.Save();
                    }

                    if (e.ErrorType == APIErrorType.UnclaimedCharacter)
                    {
                        this.c.TokenChecked = true;
                        this.c.TokenValid = true;
                        this.c.CharacterClaimed = false;
                        this.c.Save();
                    }

                    throw new Exception(e.ErrorMessage);
                }
            }
        }

        /// <summary>
        /// Do Request is a wrapper for requests to the Tomestone API.
        /// </summary>
        /// <param name="request"></param>
        internal void DoRequest(Func<Task<APIError?>> request)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    APIError? e = this.PrepareRequest();
                    HandleAPIError(e);

                    e = await request();
                    HandleAPIError(e);

                    if (!this.c.TokenChecked)
                    {
                        this.c.TokenChecked = true;
                        this.c.TokenValid = true;
                        this.c.CharacterClaimed = true;
                        this.c.Save();
                    }
                }
                catch (Exception ex)
                {
                    Service.Log.Warning($"API Error: {ex.Message}");
                }
                finally
                {
                    // Clean up
                }
            });
        }

        private APIError? PrepareRequest()
        {
            // Check if the first-run setup is done
            if (this.c.IsFirstLaunch && this.c.TokenChecked)
            {
                return new APIError { IsError = true, ErrorType = APIErrorType.GenericError, ErrorMessage = "First-run setup is not done." };
            }

            // Check if the Plugin is enabled
            if (!this.c.Enabled && this.c.TokenChecked)
            {
                return new APIError { IsError = true, ErrorType = APIErrorType.GenericError, ErrorMessage = "Plugin is not enabled." };
            }

            // Check if the API key is set
            if (this.c.DalamudToken == null || this.c.DalamudToken == "")
            {
                return new APIError { IsError = true, ErrorType = APIErrorType.GenericError, ErrorMessage = "API key is not set." };
            }

            // Check if the API key is marked as invalid
            if (!this.c.TokenValid && this.c.TokenChecked)
            {
                // We use a generic error here, as we already know the token is invalid and don't need to handle it again
                return new APIError { IsError = true, ErrorType = APIErrorType.GenericError, ErrorMessage = "API key is invalid. Please set a valid key." };
            }

            // Set the authorization header, it's fine if this overwrites the previous one, but we always want to make sure the latest token is used
            this.client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.c.DalamudToken);

            // Setup the Accept header so we don't get HTML responses from Laravel :)
            this.client.DefaultRequestHeaders.Accept.Clear();
            this.client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

            // Setup the User-Agent (Dalamud/PluginVersion)
            this.client.DefaultRequestHeaders.UserAgent.Clear();
            this.client.DefaultRequestHeaders.UserAgent.ParseAdd($"Dalamud.Tomestone/{Configuration.VersionString}");

            return null;
        }

        private const string ACTIVITY_ENDPOINT = @"{0}/update-activity/{1}/{2}";
        public async Task<APIError?> SendActivity(string playerName, string worldName, ActivityDTO payload)
        {
            // Ensure playerName and worldName are not empty
            if (playerName == "" || worldName == "")
            {
                return new APIError { IsError = true, ErrorType = APIErrorType.GenericError, ErrorMessage = "Player name or world name is empty." };
            }

            // Lowercase the player and world name, just to be sure
            playerName = playerName.ToLower();
            worldName = worldName.ToLower();

            // URL Encode the Player Name
            playerName = Uri.EscapeDataString(playerName);

            // Serialize the payload to JSON
            string json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Build the URL
            var url = string.Format(ACTIVITY_ENDPOINT, API_URL, playerName, worldName);

#if DEBUG
            Service.Log.Debug($"POST {url}: {json}");
#endif

            // Send the request
            var response = await this.client.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return new APIError { IsError = true, ErrorType = APIErrorType.UnclaimedCharacter, ErrorMessage = $"Character {playerName} on {worldName} is not claimed." };
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return new APIError { IsError = true, ErrorType = APIErrorType.InvalidToken, ErrorMessage = "Invalid API key." };
                }

                return new APIError { IsError = true, ErrorType = APIErrorType.GenericError, ErrorMessage = $"Activity ({response.StatusCode})" };
            }

            return null;
        }

        private const string GEAR_ENDPOINT = @"{0}/update-gear/{1}/{2}";
        public async Task<APIError?> SendGear(string playerName, string worldName, Gearset payload)
        {
            // Ensure playerName and worldName are not empty
            if (playerName == "" || worldName == "")
            {
                return new APIError { IsError = true, ErrorMessage = "Player name or world name is empty." };
            }

            // Lowercase the player and world name, just to be sure
            playerName = playerName.ToLower();
            worldName = worldName.ToLower();

            // URL Encode the Player Name
            playerName = Uri.EscapeDataString(playerName);

            // Serialize the payload to JSON
            string json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Build the URL
            var url = string.Format(GEAR_ENDPOINT, API_URL, playerName, worldName);

#if DEBUG
            Service.Log.Debug($"POST {url}: {json}");
#endif

            //Send the request
            var response = await this.client.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return new APIError { IsError = true, ErrorType = APIErrorType.UnclaimedCharacter, ErrorMessage = $"Character {playerName} on {worldName} is not claimed." };
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return new APIError { IsError = true, ErrorType = APIErrorType.InvalidToken, ErrorMessage = "Invalid API key." };
                }

                return new APIError { IsError = true, ErrorMessage = $"Gear ({response.StatusCode})" };
            }

            return null;
        }

        private const string TRIAD_CARDS_ENDPOINT = @"{0}/update-triple-triad-cards/{1}/{2}";
        public async Task<APIError?> SendTriadCards(string playerName, string worldName, TriadCardsDTO payload)
        {
            // Ensure playerName and worldName are not empty
            if (playerName == "" || worldName == "")
            {
                return new APIError { IsError = true, ErrorMessage = "Player name or world name is empty." };
            }

            // Lowercase the player and world name, just to be sure
            playerName = playerName.ToLower();
            worldName = worldName.ToLower();

            // URL Encode the Player Name
            playerName = Uri.EscapeDataString(playerName);

            // Serialize the payload to JSON
            string json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Build the URL
            var url = string.Format(TRIAD_CARDS_ENDPOINT, API_URL, playerName, worldName);

#if DEBUG
            Service.Log.Debug($"POST {url}: {json}");
#endif

            // Send the request
            var response = await this.client.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return new APIError { IsError = true, ErrorType = APIErrorType.UnclaimedCharacter, ErrorMessage = $"Character {playerName} on {worldName} is not claimed." };
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return new APIError { IsError = true, ErrorType = APIErrorType.InvalidToken, ErrorMessage = "Invalid API key." };
                }

                return new APIError { IsError = true, ErrorMessage = $"Triple Triad Cards ({response.StatusCode})" };
            }

            return null;
        }

        private const string ORCHESTRION_ENDPOINT = @"{0}/update-orchestrion-rolls/{1}/{2}";
        public async Task<APIError?> SendOrchestrionRolls(string playerName, string worldName, OrchestrionDTO payload)
        {
            // Ensure playerName and worldName are not empty
            if (playerName == "" || worldName == "")
            {
                return new APIError { IsError = true, ErrorMessage = "Player name or world name is empty." };
            }

            // Lowercase the player and world name, just to be sure
            playerName = playerName.ToLower();
            worldName = worldName.ToLower();

            // URL Encode the Player Name
            playerName = Uri.EscapeDataString(playerName);

            // Serialize the payload to JSON
            string json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Build the URL
            var url = string.Format(ORCHESTRION_ENDPOINT, API_URL, playerName, worldName);

#if DEBUG
            Service.Log.Debug($"POST {url}: {json}");
#endif

            // Send the request
            var response = await this.client.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return new APIError { IsError = true, ErrorType = APIErrorType.UnclaimedCharacter, ErrorMessage = $"Character {playerName} on {worldName} is not claimed." };
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return new APIError { IsError = true, ErrorType = APIErrorType.InvalidToken, ErrorMessage = "Invalid API key." };
                }

                return new APIError { IsError = true, ErrorMessage = $"Orchestrion Rolls ({response.StatusCode})" };
            }

            return null;
        }
    }
}

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
        public string ErrorMessage { get; set; } = "";
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

        private APIError? PrepareRequest()
        {
            // Check if the first-run setup is done
            if (this.c.IsFirstLaunch)
            {
                return new APIError { IsError = true, ErrorMessage = "First-run setup is not done." };
            }

            // Check if the Plugin is enabled
            if (!this.c.Enabled)
            {
                return new APIError { IsError = true, ErrorMessage = "Plugin is not enabled." };
            }

            // Check if the API key is set
            if (this.c.DalamudToken == null || this.c.DalamudToken == "")
            {
                return new APIError { IsError = true, ErrorMessage = "API key is not set." };
            }

            // Set the authorization header, it's fine if this overwrites the previous one, but we always want to make sure the latest token is used
            this.client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.c.DalamudToken);

            // Setup the User-Agent (Dalamud/PluginVersion)
            this.client.DefaultRequestHeaders.UserAgent.Clear();
            this.client.DefaultRequestHeaders.UserAgent.ParseAdd($"Dalamud/{Configuration.VersionString}");

            return null;
        }

        private const string ACTIVITY_ENDPOINT = @"{0}/update-activity/{1}/{2}";
        public async Task<APIError?> SendActivity(string playerName, string worldName, ActivityDTO payload)
        {
            APIError? e = this.PrepareRequest();
            if (e != null)
            {
                return e;
            }

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
            var url = string.Format(ACTIVITY_ENDPOINT, API_URL, playerName, worldName);

            // Send the request
            var response = await this.client.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                return new APIError { IsError = true, ErrorMessage = $"Failed to send activity data ({response.StatusCode})" };
            }

            return null;
        }

        private const string GEAR_ENDPOINT = @"{0}/update-gear/{1}/{2}";
        public async Task<APIError?> SendGear(string playerName, string worldName, Gearset payload)
        {
            APIError? e = this.PrepareRequest();
            if (e != null)
            {
                return e;
            }

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

            // Send the request
            var response = await this.client.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                return new APIError { IsError = true, ErrorMessage = $"Failed to send gear data ({response.StatusCode})" };
            }

            return null;
        }
    }
}

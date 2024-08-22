using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;

namespace Dalamud.Tomestone.UI
{
    internal class FirstLaunchUI : Window
    {
        public OnboardingStep onboardingStep;

        private string? stepWelcome;
        private string? stepCharacterClaim;
        private string? stepDalamudToken;
        private string? stepSettings;
        private bool hasCachedLocStrings;

        public FirstLaunchUI() : base($"{Tomestone.T.Name} {Tomestone.T.GetType().Assembly.GetName().Version}###TomestoneFirstLaunch")
        {
            this.RespectCloseHotkey = false;
            this.SizeConstraints = new()
            {
                MinimumSize = new(1000, 500),
                MaximumSize = new(9999, 9999)
            };

            // Calculate the middle of the screen
            var screen = ImGui.GetMainViewport();

            // Set the position of the window to the middle of the screen
            this.Position = new Vector2(screen.Pos.X + screen.Size.X / 2 - this.SizeConstraints.Value.MinimumSize.X / 2, screen.Pos.Y + screen.Size.Y / 2 - this.SizeConstraints.Value.MinimumSize.Y / 2);

            Tomestone.T.WindowSystem.AddWindow(this);

            if (Tomestone.CurrentLocManager != null)
            {
                Tomestone.CurrentLocManager.LocalizationChanged += (langCode) => { hasCachedLocStrings = false; };
            }
        }

        public void Dispose() { }

        private void UpdateLocalizationCache()
        {
            if (hasCachedLocStrings) { return; }
            hasCachedLocStrings = true;

            // Update the strings
            stepWelcome = Localization.Localize("FL_StepWelcome", "Welcome");
            stepCharacterClaim = Localization.Localize("FL_StepCharacterClaim", "Character Claim");
            stepDalamudToken = Localization.Localize("FL_StepDalamudToken", "Dalamud Token");
            stepSettings = Localization.Localize("FL_StepSettings", "Settings");
        }

        public override void Draw()
        {
            UpdateLocalizationCache();

            var region = ImGui.GetContentRegionAvail();
            var itemSpacing = ImGui.GetStyle().ItemSpacing;

            var topLeftSideHeight = region.Y;

            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(5f, 0));
            try
            {
                using (var table = ImRaii.Table($"TomestoneTableContainer", 2, ImGuiTableFlags.Resizable))
                {
                    if (!table)
                        return;

                    ImGui.TableSetupColumn("##LeftColumn", ImGuiTableColumnFlags.WidthFixed, ImGui.GetWindowWidth() / 2);

                    ImGui.TableNextColumn();

                    var regionSize = ImGui.GetContentRegionAvail();

                    ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(0.5f, 0.5f));
                    using (var leftChild = ImRaii.Child($"###TomestoneLeftSide", regionSize with { Y = topLeftSideHeight }, false, ImGuiWindowFlags.NoDecoration))
                    {
                        ImGui.Selectable(stepWelcome, onboardingStep == OnboardingStep.Welcome, ImGuiSelectableFlags.SpanAllColumns);
                        ImGui.Spacing();
                        ImGui.Selectable(stepCharacterClaim, onboardingStep == OnboardingStep.CharacterClaim, ImGuiSelectableFlags.SpanAllColumns);
                        ImGui.Spacing();
                        ImGui.Selectable(stepDalamudToken, onboardingStep == OnboardingStep.DalamudToken, ImGuiSelectableFlags.SpanAllColumns);
                        ImGui.Spacing();
                        ImGui.Selectable(stepSettings, onboardingStep == OnboardingStep.Settings, ImGuiSelectableFlags.SpanAllColumns);
                    }

                    ImGui.PopStyleVar();

                    ImGui.TableNextColumn();

                    ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(0, 0));
                    using (var rightChild = ImRaii.Child($"###TomestoneRightSide", Vector2.Zero, false))
                    {
                        switch (this.onboardingStep)
                        {
                            case OnboardingStep.Welcome:
                                DrawWelcome();
                                break;
                            case OnboardingStep.CharacterClaim:
                                DrawCharacterClaim();
                                break;
                            case OnboardingStep.DalamudToken:
                                DrawDalamudToken();
                                break;
                            case OnboardingStep.Settings:
                                DrawSettings();
                                break;
                            case OnboardingStep.Finish:
                                DrawFinish();
                                break;
                        }
                    }
                    ImGui.PopStyleVar();
                }
            }
            catch (Exception ex)
            {
                Service.Log.Error(ex, "Error in Tomestone UI");
            }
            ImGui.PopStyleVar();
        }

        private void DrawWelcome()
        {
            ImGui.TextWrapped($"Welcome to {Tomestone.T.Name}! This is your first time using the plugin, so we need to set up a few things first.");
            ImGui.TextWrapped($"Please follow the steps on the next pages to get started.");
            ImGui.Spacing();
            if (ImGui.Button("Next"))
            {
                this.onboardingStep = OnboardingStep.CharacterClaim;
            }
        }

        private void DrawCharacterClaim()
        {
            ImGui.TextWrapped($"To use the {Tomestone.T.Name} plugin, you need to create a Tomestone.gg account and claim your character.");
            ImGui.TextWrapped($"Please click the button below to create your account and claim your character now.");
            ImGui.Spacing();

            if (ImGui.Button("Create Account"))
            {
                // Open "https://tomestone.gg/register" in the default browser
                Utils.OpenLink("https://tomestone.gg/register");
            }

            if (ImGui.Button("Claim Character"))
            {
                // Open "https://tomestone.gg/import/character" in the default browser
                Utils.OpenLink("https://tomestone.gg/import/character");
            }
            ImGui.Spacing();
            ImGui.TextWrapped($"After you have claimed your character, please click the button below to continue.");
            if (ImGui.Button("Next"))
            {
                this.onboardingStep = OnboardingStep.DalamudToken;
            }
        }

        private void DrawDalamudToken()
        {
            string dalamudToken = Tomestone.T.Configuration.DalamudToken;

            ImGui.TextWrapped($"In order to connect the plugin to your Tomestone account, we need you to set a Dalamud access token.");
            ImGui.TextWrapped($"You can obtain this token on your account settings page on Tomestone.gg. Click the Button below to open that Page and scroll down to \"Dalamud access token\".");
            ImGui.TextWrapped($"Press the \"Generate access token\" button, copy the token and paste it into the field below.");
            if (ImGui.Button("Open Tomestone Account Settings"))
            {
                // Open "https://tomestone.gg/profile/account" in the default browser
                Utils.OpenLink("https://tomestone.gg/profile/account");
            }
            ImGui.Spacing();
            if (ImGui.InputText("Dalamud Access Token", ref dalamudToken, 64, ImGuiInputTextFlags.None))
            {
                Tomestone.T.Configuration.DalamudToken = dalamudToken;
                Tomestone.T.Configuration.TokenChecked = false;
                Tomestone.T.Configuration.TokenValid = false;
                Tomestone.T.Configuration.CharacterClaimed = false;
                Tomestone.T.Configuration.Save();
            }
            ImGui.Spacing();
            // Check if a token is set before displaying the next button
            if (string.IsNullOrEmpty(dalamudToken))
            {
                ImGui.TextWrapped($"Please set your Dalamud access token before continuing.");
            }
            else
            {
                // Check if the token is still being checked
                if (!Tomestone.T.Configuration.TokenChecked)
                {
                    // Set the text color to yellow
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 0, 1));
                    ImGui.TextWrapped($"Please wait while we check your token...");
                    ImGui.PopStyleColor();
                }
                else
                {
                    // Check if the token is valid
                    if (Tomestone.T.Configuration.TokenValid && Tomestone.T.Configuration.CharacterClaimed)
                    {
                        // Set the text color to green
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 1, 0, 1));
                        ImGui.TextWrapped($"Your token is valid! You can now continue.");
                        ImGui.PopStyleColor();
                        if (ImGui.Button("Next"))
                        {
                            this.onboardingStep = OnboardingStep.Settings;
                        }
                    }
                    else
                    {
                        var errorText = Tomestone.T.Configuration.CharacterClaimed ? $"Your token is invalid. Please check if you copied it correctly." : $"Your character is not claimed on Tomestone.gg. Please claim it first - press Refresh after claiming it!";

                        // Set the text color to red
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
                        ImGui.TextWrapped(errorText);
                        ImGui.PopStyleColor();

                        if (!Tomestone.T.Configuration.CharacterClaimed)
                        {
                            if (ImGui.Button("Claim Character"))
                            {
                                // Open "https://tomestone.gg/import/character" in the default browser
                                Utils.OpenLink("https://tomestone.gg/import/character");
                            }

                            if (ImGui.Button("Refresh"))
                            {
                                Tomestone.T.Configuration.TokenChecked = false;
                                Tomestone.T.Configuration.TokenValid = false;
                                Tomestone.T.Configuration.CharacterClaimed = false;
                                Tomestone.T.Configuration.Save();
                            }
                        }
                    }
                }


            }
        }

        private void DrawSettings()
        {
            bool sendActivity = Tomestone.T.Configuration.SendActivity;
            bool sendGear = Tomestone.T.Configuration.SendGear;
            bool sendTriad = Tomestone.T.Configuration.SendTriad;
            bool sendOrchestrion = Tomestone.T.Configuration.SendOrchestrion;

            ImGui.TextWrapped($"Before we finish, check which data you want to send to {Tomestone.T.Name}.");
            ImGui.Separator();

            // TODO: Make sure wording here equals the wording on the plugin settings page
            if (ImGui.Checkbox("Send activity data to Tomestone.gg", ref sendActivity))
            {
                Tomestone.T.Configuration.SendActivity = sendActivity;
                Tomestone.T.Configuration.Save();
            }
            ImGuiComponents.HelpMarker("If enabled, Tomestone will send your activity data to Tomestone.gg. This includes your current job, level, zone and if you are traveling to another world. This is NOT displayed on your activity page, but used for the streams list.");

            if (ImGui.Checkbox("Send gear data to Tomestone.gg", ref sendGear))
            {
                Tomestone.T.Configuration.SendGear = sendGear;
                Tomestone.T.Configuration.Save();
            }
            ImGuiComponents.HelpMarker("If enabled, Tomestone will send your current gear data to Tomestone.gg.");

            if (ImGui.Checkbox("Send Triple Triad data to Tomestone.gg", ref sendTriad))
            {
                Tomestone.T.Configuration.SendTriad = sendTriad;
                Tomestone.T.Configuration.Save();
            }
            ImGuiComponents.HelpMarker("If enabled, Tomestone will send your Triple Triad card data to Tomestone.gg.");

            if (ImGui.Checkbox("Send Orchestrion data to Tomestone.gg", ref sendOrchestrion))
            {
                Tomestone.T.Configuration.SendOrchestrion = sendOrchestrion;
                Tomestone.T.Configuration.Save();
            }
            ImGuiComponents.HelpMarker("If enabled, Tomestone will send your Orchestrion roll data to Tomestone.gg.");

            ImGui.Separator();
            if (ImGui.Button("Finish"))
            {
                this.onboardingStep = OnboardingStep.Finish;
            }
        }

        private void DrawFinish()
        {
            // TODO: Implement some kind of initial data transfer, e.g. send character data to Tomestone.gg, so we can make sure the user has everything set up correctly
            //bool checkDataTransfer = false;
            //if (!checkDataTransfer)
            //{
            //    ImGui.TextWrapped($"Please wait a moment while we set up {Tomestone.T.Name} for you. This should only take a few seconds.");
            //    ImGui.TextWrapped($"If everything is set up correctly, the window will close automatically.");
            //}
            //else
            //{
                Tomestone.T.Configuration.IsFirstLaunch = false;
                Tomestone.T.Configuration.Save();
                this.Toggle();
            //}
        }
    }

    public enum OnboardingStep
    {
        Welcome,
        CharacterClaim,
        DalamudToken,
        Settings,
        Finish
    }
}

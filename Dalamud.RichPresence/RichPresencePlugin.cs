using System;
using System.Collections.Generic;
using System.Linq;

using DiscordRPC;
using Lumina.Excel.GeneratedSheets;

using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Utility;

using Dalamud.RichPresence.Configuration;
using Dalamud.RichPresence.Interface;
using Dalamud.RichPresence.Managers;
using Dalamud.RichPresence.Models;

namespace Dalamud.RichPresence
{
    internal class RichPresencePlugin : IDalamudPlugin, IDisposable
    {
        [PluginService]
        internal static DalamudPluginInterface DalamudPluginInterface { get; private set; }

        [PluginService]
        internal static ClientState ClientState { get; private set; }

        [PluginService]
        internal static CommandManager CommandManager { get; private set; }

        [PluginService]
        internal static DataManager DataManager { get; private set; }

        [PluginService]
        internal static Framework Framework { get; private set; }

        [PluginService]
        internal static PartyList PartyList { get; private set; }

        internal static LocalizationManager LocalizationManager { get; private set; }
        internal static DiscordPresenceManager DiscordPresenceManager { get; private set; }

        private static RichPresenceConfigWindow RichPresenceConfigWindow;
        internal static RichPresenceConfig RichPresenceConfig { get; set; }

        private List<TerritoryType> Territories;
        private DateTime startTime = DateTime.UtcNow;

        private const string DEFAULT_LARGE_IMAGE_KEY = "li_1";
        private const string DEFAULT_SMALL_IMAGE_KEY = "class_0";
        private static readonly DiscordRPC.RichPresence DEFAULT_PRESENCE = new DiscordRPC.RichPresence
        {
            Assets = new Assets
            {
                LargeImageKey = DEFAULT_LARGE_IMAGE_KEY,
                SmallImageKey = DEFAULT_SMALL_IMAGE_KEY
            }
        };

        public RichPresencePlugin()
        {
            RichPresenceConfig = DalamudPluginInterface.GetPluginConfig() as RichPresenceConfig ?? new RichPresenceConfig();

            DiscordPresenceManager = new DiscordPresenceManager();
            LocalizationManager = new LocalizationManager();
            SetDefaultPresence();

            RichPresenceConfigWindow = new RichPresenceConfigWindow();
            DalamudPluginInterface.UiBuilder.Draw += RichPresenceConfigWindow.DrawRichPresenceConfigWindow;
            DalamudPluginInterface.UiBuilder.OpenConfigUi += RichPresenceConfigWindow.Open;

            Framework.Update += UpdateRichPresence;

            ClientState.Login += State_Login;
            ClientState.TerritoryChanged += State_TerritoryChanged;
            ClientState.Logout += State_Logout;

            RegisterCommand();
            DalamudPluginInterface.LanguageChanged += ReregisterCommand;

            Territories = DataManager.GetExcelSheet<TerritoryType>().ToList();
        }

        public string Name => "Discord Rich Presence";

        public void Dispose()
        {
            DalamudPluginInterface.LanguageChanged -= ReregisterCommand;
            UnregisterCommand();

            ClientState.Login -= State_Login;
            ClientState.TerritoryChanged -= State_TerritoryChanged;
            ClientState.Logout -= State_Logout;

            Framework.Update -= UpdateRichPresence;

            DalamudPluginInterface.UiBuilder.OpenConfigUi -= RichPresenceConfigWindow.Open;
            DalamudPluginInterface.UiBuilder.Draw -= RichPresenceConfigWindow.DrawRichPresenceConfigWindow;

            LocalizationManager?.Dispose();

            DiscordPresenceManager.ClearPresence();
            DiscordPresenceManager?.Dispose();
        }

        private void SetDefaultPresence()
        {
            DiscordPresenceManager.SetPresence(DEFAULT_PRESENCE);
            DiscordPresenceManager.UpdatePresenceDetails(LocalizationManager.Localize("DalamudRichPresenceInMenus", LocalizationLanguage.Client));
            UpdateStartTime();
        }

        private void UpdateStartTime()
        {
            if (RichPresenceConfig.ResetTimeWhenChangingZones)
            {
                startTime = DateTime.UtcNow;
            }

            if (RichPresenceConfig.ShowStartTime)
            {
                DiscordPresenceManager.UpdatePresenceStartTime(startTime);
            }
        }

        private void State_Login(object sender, System.EventArgs e)
        {
            UpdateStartTime();
        }

        private void State_TerritoryChanged(object sender, ushort e)
        {
            UpdateStartTime();
        }

        private void State_Logout(object sender, System.EventArgs e)
        {
            SetDefaultPresence();
            UpdateStartTime();
        }

        private void ReregisterCommand(string langCode)
        {
            this.UnregisterCommand();
            this.RegisterCommand();
        }

        private void UnregisterCommand()
        {
            CommandManager.RemoveHandler("/prp");
        }

        private void RegisterCommand()
        {
            CommandManager.AddHandler("/prp",
                new CommandInfo((string cmd, string args) => RichPresenceConfigWindow.Toggle())
                {
                    HelpMessage = LocalizationManager.Localize("DalamudRichPresenceOpenConfiguration", LocalizationLanguage.Plugin)
                }
            );
        }

        private void UpdateRichPresence(Framework framework)
        {
            try
            {
                DiscordPresenceManager.Update();

                var localPlayer = ClientState.LocalPlayer;

                // Return early if data is not ready
                if (localPlayer is null)
                {
                    return;
                }

                var territoryId = ClientState.TerritoryType;
                var territoryName = LocalizationManager.Localize("DalamudRichPresenceTheSource", LocalizationLanguage.Client);
                var territoryRegion = LocalizationManager.Localize("DalamudRichPresenceVoid", LocalizationLanguage.Client);

                // Details defaults to player name
                var richPresenceDetails = localPlayer.Name.ToString();
                // State defaults to current world
                var richPresenceState = localPlayer.CurrentWorld.GameData.Name.ToString();
                // Large image defaults to world map
                var richPresenceLargeImageText = territoryName;
                var richPresenceLargeImageKey = DEFAULT_LARGE_IMAGE_KEY;
                // Small image defaults to "Online"
                var richPresenceSmallImageKey = DEFAULT_SMALL_IMAGE_KEY;
                var richPresenceSmallImageText = LocalizationManager.Localize("DalamudRichPresenceOnline", LocalizationLanguage.Client);
                // Show start timestamp if configured
                Timestamps richPresenceTimestamps = (RichPresenceConfig.ShowStartTime) ? new Timestamps(startTime) : null;

                if (territoryId != 0)
                {
                    // Read territory data from generated sheet
                    var territory = Territories.First(Row => Row.RowId == territoryId);
                    territoryName = territory.PlaceName.Value?.Name ?? LocalizationManager.Localize("DalamudRichPresenceUnknown", LocalizationLanguage.Client);
                    territoryRegion = territory.PlaceNameRegion.Value?.Name ?? LocalizationManager.Localize("DalamudRichPresenceUnknown", LocalizationLanguage.Client);
                    // Set large image to territory
                    richPresenceLargeImageText = territoryName;
                    richPresenceLargeImageKey = $"li_{territory.LoadingImage}";
                }

                // Show character name if configured
                if (RichPresenceConfig.ShowName)
                {
                    // Show free company tag if configured
                    if (RichPresenceConfig.ShowFreeCompany && localPlayer.CurrentWorld.Id == localPlayer.HomeWorld.Id)
                    {
                        var fcTag = localPlayer.CompanyTag.ToString();
                        // Append free company tag to player name if it exists
                        richPresenceDetails = (fcTag.IsNullOrEmpty()) ? richPresenceDetails : $"{richPresenceDetails} «{fcTag}»";
                    }
                    else if (RichPresenceConfig.ShowWorld && localPlayer.CurrentWorld.Id != localPlayer.HomeWorld.Id)
                    {
                        // Append home world name to current player name while visiting another world
                        richPresenceDetails = $"{richPresenceDetails} ❀ {localPlayer.HomeWorld.GameData.Name.ToString()}";
                    }
                }
                else
                {
                    // Replace character name with territory name
                    richPresenceDetails = territoryName;
                }

                // Show current job if configured
                if (RichPresenceConfig.ShowJob)
                {
                    // Set small image to job icon
                    richPresenceSmallImageKey = $"class_{localPlayer.ClassJob.Id}";
                    // Abbreviate job name if configured
                    richPresenceSmallImageText = (RichPresenceConfig.AbbreviateJob)
                        ? localPlayer.ClassJob.GameData.Abbreviation
                        : LocalizationManager.TitleCase(localPlayer.ClassJob.GameData.Name.ToString());

                    // Show current job level if configured
                    if (RichPresenceConfig.ShowLevel)
                    {
                        var levelText = String.Format(LocalizationManager.Localize("DalamudRichPresenceLevel", LocalizationLanguage.Client), localPlayer.Level);
                        richPresenceSmallImageText = $"{richPresenceSmallImageText} {levelText}";
                    }
                }

                // Hide world name if configured
                if (!RichPresenceConfig.ShowWorld)
                {
                    // Replace world name with territory name or territory region
                    richPresenceState = (RichPresenceConfig.ShowName) ? territoryName : territoryRegion;
                }

                // Create rich presence object
                var richPresence = new DiscordRPC.RichPresence
                {
                    Details = richPresenceDetails,
                    State = richPresenceState,
                    Assets = new Assets
                    {
                        LargeImageKey = richPresenceLargeImageKey,
                        LargeImageText = richPresenceLargeImageText,
                        SmallImageKey = richPresenceSmallImageKey,
                        SmallImageText = richPresenceSmallImageText
                    },
                    Timestamps = richPresenceTimestamps
                };

                // Request new presence to be set
                DiscordPresenceManager.SetPresence(richPresence);
            }
            catch (Exception ex)
            {
                PluginLog.LogError(ex, "Could not run OnUpdate.");
            }
        }
    }
}

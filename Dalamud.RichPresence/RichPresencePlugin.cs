using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Game.Command;
using Dalamud.Game.Internal;
using Dalamud.Plugin;
using Dalamud.RichPresence.Config;
using DiscordRPC;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace Dalamud.RichPresence
{
    public class RichPresencePlugin : IDalamudPlugin
    {
        private DalamudPluginInterface _pi;

        private RichPresenceConfig Config;

        private List<TerritoryType> _territoryTypes;

        private const string DISCORD_CLIENT_ID = "478143453536976896";
        private DiscordPresenceManager _discordPresenceManager;

        private DateTime startTime = DateTime.UtcNow;

        private bool _isMainConfigWindowDrawing = false;

        private static readonly DiscordRPC.RichPresence DefaultPresence = new DiscordRPC.RichPresence
        {
            Details = "In menus",
            State = "",
            Assets = new Assets
            {
                LargeImageKey = "li_1",
                LargeImageText = "",
                SmallImageKey = "class_0",
                SmallImageText = ""
            }
        };

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            _pi = pluginInterface;

            Config = pluginInterface.GetPluginConfig() as RichPresenceConfig ?? new RichPresenceConfig();

            _pi.UiBuilder.OnBuildUi += UiBuilder_OnBuildUi;
            _pi.UiBuilder.OnOpenConfigUi += (sender, args) => _isMainConfigWindowDrawing = true;

            _discordPresenceManager = new DiscordPresenceManager(DefaultPresence, DISCORD_CLIENT_ID);
            _discordPresenceManager.SetPresence(DefaultPresence);

            _pi.Framework.OnUpdateEvent += Framework_OnUpdateEvent;

            _pi.ClientState.TerritoryChanged += TerritoryChanged;

            _pi.CommandManager.AddHandler("/prp",
                new CommandInfo((string cmd, string args) => _isMainConfigWindowDrawing = true)
                {
                    HelpMessage = "Open the Discord Rich Presence configuration."
                });
        }

        private void TerritoryChanged(object sender, ushort e)
        {
            if (Config.ResetTimeWhenChangingZones)
                this.startTime = DateTime.UtcNow;
        }

        private void Framework_OnUpdateEvent(Framework framework)
        {
            try
            {
                if (!_pi.Data.IsDataReady)
                    return;

                if (_territoryTypes == null)
                {
                    _territoryTypes = _pi.Data.GetExcelSheet<TerritoryType>().ToList();
                }

                var localPlayer = _pi.ClientState.LocalPlayer;
                var territoryTypeId = _pi.ClientState.TerritoryType;

                // Data not ready
                if (localPlayer == null)
                    return;

                var placeName = "The Source";
                var placeNameZone = "Void";
                var loadingImageKey = 1;

                if (territoryTypeId != 0)
                {
                    var territoryType = _territoryTypes.First(x => x.RowId == territoryTypeId);
                    placeName = territoryType.PlaceName.Value?.Name ?? "Unknown";
                    placeNameZone = territoryType.PlaceNameRegion.Value?.Name ?? "Unknown";
                    loadingImageKey = territoryType.LoadingImage;
                }

                var largeImageKey = $"li_{loadingImageKey}";

                var fcName = localPlayer.CompanyTag;

                if (fcName != string.Empty)
                {
                    fcName = $" <{fcName}>";
                }

                var worldName = localPlayer.CurrentWorld.GameData.Name.ToString();

                if (localPlayer.CurrentWorld.Id != localPlayer.HomeWorld.Id)
                    worldName = $"{worldName} (🏠{localPlayer.HomeWorld.GameData.Name})";

                var playerName = $"{localPlayer.Name}{fcName}";
                if (!Config.ShowName)
                    playerName = placeName;

                if (!Config.ShowWorld)
                    worldName = Config.ShowName ? placeName : placeNameZone;


                var rp = new DiscordRPC.RichPresence
                {
                    Details = playerName,
                    State = worldName,
                    Assets = new Assets
                    {
                        LargeImageKey = largeImageKey,
                        LargeImageText = placeName,
                        SmallImageKey = $"class_{localPlayer.ClassJob.Id}",
                        SmallImageText = localPlayer.ClassJob.GameData.Abbreviation + " Lv." + localPlayer.Level
                    },
                };

                if (Config.ShowStartTime)
                {
                    rp.Timestamps = new Timestamps(startTime);
                }
                    

                _discordPresenceManager.SetPresence(rp);
            }
            catch (Exception ex)
            {
                PluginLog.LogError(ex, "Could not run RichPresence OnUpdate.");
            }
        }

        private void UiBuilder_OnBuildUi()
        {
            if (!_isMainConfigWindowDrawing) return;
            ImGui.SetNextWindowSize(new Vector2(750, 520));
            
            if (ImGui.Begin("RichPresence Config", ref _isMainConfigWindowDrawing,
                ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar))
            {
                ImGui.Text("This window allows you to configure Discord Rich Presence.");
                ImGui.Text("Please make sure Discord is not running as admin and you have game activity enabled.");
                ImGui.Separator();

                ImGui.BeginChild("scrolling", new Vector2(0, 400), true, ImGuiWindowFlags.HorizontalScrollbar);

                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(1, 3));

                ImGui.Checkbox("Show name", ref Config.ShowName);
                ImGui.Checkbox("Show world", ref Config.ShowWorld);
                ImGui.Separator();
                ImGui.Checkbox("Show start time", ref Config.ShowStartTime);
                ImGui.Checkbox("Reset start time when changing zones", ref Config.ResetTimeWhenChangingZones);

                ImGui.PopStyleVar();

                ImGui.EndChild();

                ImGui.Separator();

                if (ImGui.Button("Save and Close"))
                {
                    _isMainConfigWindowDrawing = false;
                    _pi.SavePluginConfig(Config);
                    PluginLog.Log("RP saved.");
                }

                ImGui.End();
            }
        }

        public string Name => "Discord Rich Presence";

        public void Dispose()
        {
            _pi.Framework.OnUpdateEvent -= Framework_OnUpdateEvent;
            _pi.ClientState.TerritoryChanged -= TerritoryChanged;
            _discordPresenceManager?.Dispose();

            _pi.CommandManager.RemoveHandler("/prp");
            _pi.Dispose();
        }
    }
}

using System.Numerics;

using ImGuiNET;

using Dalamud.Logging;

using Dalamud.RichPresence.Models;

namespace Dalamud.RichPresence.Interface
{
    internal class RichPresenceConfigWindow
    {
        private bool IsOpen = false;

        public void DrawRichPresenceConfigWindow()
        {
            if (!this.IsOpen)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(750, 520));
            var imGuiReady = ImGui.Begin(
                RichPresencePlugin.LocalizationManager.Localize("DalamudRichPresenceConfiguration", LocalizationLanguage.Plugin),
                ref IsOpen,
                ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar
            );

            if (imGuiReady)
            {
                ImGui.Text(RichPresencePlugin.LocalizationManager.Localize("DalamudRichPresencePreface1", LocalizationLanguage.Plugin));
                ImGui.Text(RichPresencePlugin.LocalizationManager.Localize("DalamudRichPresencePreface2", LocalizationLanguage.Plugin));
                ImGui.Separator();

                ImGui.BeginChild("scrolling", new Vector2(0, 400), true, ImGuiWindowFlags.HorizontalScrollbar);

                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(1, 3));

                ImGui.Checkbox(RichPresencePlugin.LocalizationManager.Localize("DalamudRichPresenceShowName", LocalizationLanguage.Plugin), ref RichPresencePlugin.RichPresenceConfig.ShowName);
                ImGui.Checkbox(RichPresencePlugin.LocalizationManager.Localize("DalamudRichPresenceShowFreeCompany", LocalizationLanguage.Plugin), ref RichPresencePlugin.RichPresenceConfig.ShowFreeCompany);
                ImGui.Checkbox(RichPresencePlugin.LocalizationManager.Localize("DalamudRichPresenceShowWorld", LocalizationLanguage.Plugin), ref RichPresencePlugin.RichPresenceConfig.ShowWorld);
                ImGui.Separator();
                ImGui.Checkbox(RichPresencePlugin.LocalizationManager.Localize("DalamudRichPresenceShowStartTime", LocalizationLanguage.Plugin), ref RichPresencePlugin.RichPresenceConfig.ShowStartTime);
                ImGui.Checkbox(RichPresencePlugin.LocalizationManager.Localize("DalamudRichPresenceResetTimeWhenChangingZones", LocalizationLanguage.Plugin), ref RichPresencePlugin.RichPresenceConfig.ResetTimeWhenChangingZones);
                ImGui.Separator();
                ImGui.Checkbox(RichPresencePlugin.LocalizationManager.Localize("DalamudRichPresenceShowJob", LocalizationLanguage.Plugin), ref RichPresencePlugin.RichPresenceConfig.ShowJob);
                ImGui.Checkbox(RichPresencePlugin.LocalizationManager.Localize("DalamudRichPresenceAbbreviateJob", LocalizationLanguage.Plugin), ref RichPresencePlugin.RichPresenceConfig.AbbreviateJob);
                ImGui.Checkbox(RichPresencePlugin.LocalizationManager.Localize("DalamudRichPresenceShowLevel", LocalizationLanguage.Plugin), ref RichPresencePlugin.RichPresenceConfig.ShowLevel);

                ImGui.PopStyleVar();

                ImGui.EndChild();

                ImGui.Separator();

                if (ImGui.Button(RichPresencePlugin.LocalizationManager.Localize("DalamudRichPresenceSaveAndClose", LocalizationLanguage.Plugin)))
                {
                    this.Close();
                    RichPresencePlugin.DalamudPluginInterface.SavePluginConfig(RichPresencePlugin.RichPresenceConfig);
                    PluginLog.Log("Settings saved.");
                }

                ImGui.End();
            }
        }

        public void Open()
        {
            this.IsOpen = true;
        }

        public void Close()
        {
            this.IsOpen = false;
        }

        public void Toggle()
        {
            this.IsOpen = !this.IsOpen;
        }
    }
}

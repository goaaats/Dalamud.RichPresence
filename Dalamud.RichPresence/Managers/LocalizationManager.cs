using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using Newtonsoft.Json;

using Dalamud.Logging;

using Dalamud.RichPresence.Models;
using Dalamud.Game;
using Dalamud.Plugin.Services;

namespace Dalamud.RichPresence.Managers
{
    internal class LocalizationManager : IDisposable
    {
        private CultureInfo clientCultureInfo;
        private Dictionary<string, LocalizationEntry> clientLocalizationDictonary;
        private Dictionary<string, LocalizationEntry> pluginLocalizationDictionary;
        private Dictionary<string, LocalizationEntry> defaultLocalizationDictionary;


        private const string PREFIX = "dalamud_richpresence_";
        private const string DEFAULT_DICT_LANGCODE = "en";

        public LocalizationManager()
        {
            this.ReadClientLanguageLocFile(ClientLanguageToLangCode(RichPresencePlugin.ClientState.ClientLanguage));
            this.ReadPluginLanguageLocFile(RichPresencePlugin.DalamudPluginInterface.UiLanguage);
            this.defaultLocalizationDictionary = ReadFileWithLangCode(DEFAULT_DICT_LANGCODE);
            RichPresencePlugin.DalamudPluginInterface.LanguageChanged += ReadPluginLanguageLocFile;
        }

        public string Localize(string localizationStringKey, LocalizationLanguage localizationSource)
        {
            LocalizationEntry dictValue;
            bool entryFound;

            if (localizationSource == LocalizationLanguage.Client)
            {
                entryFound = clientLocalizationDictonary.TryGetValue(localizationStringKey, out dictValue);
            }
            else
            {
                entryFound = pluginLocalizationDictionary.TryGetValue(localizationStringKey, out dictValue);
            }

            if (entryFound && !string.IsNullOrEmpty(dictValue.Message))
            {
                return dictValue.Message;
            }
            else
            {
                entryFound = defaultLocalizationDictionary.TryGetValue(localizationStringKey, out dictValue);
                return dictValue.Message;
            }
        }

        public string TitleCase(string input) => clientCultureInfo.TextInfo.ToTitleCase(input);

        public void Dispose()
        {
            RichPresencePlugin.DalamudPluginInterface.LanguageChanged -= ReadPluginLanguageLocFile;
        }

        private void ReadClientLanguageLocFile(string langCode)
        {
            RichPresencePlugin.PluginLog.Debug("Loading client localization file...");
            clientLocalizationDictonary = this.ReadFileWithLangCode(langCode);
            clientCultureInfo = new CultureInfo(langCode);
            RichPresencePlugin.PluginLog.Debug("Client localization file loaded.");
        }

        private void ReadPluginLanguageLocFile(string langCode)
        {
            RichPresencePlugin.PluginLog.Debug("Loading plugin localization file...");
            pluginLocalizationDictionary = ReadFileWithLangCode(langCode);
            RichPresencePlugin.PluginLog.Debug("Plugin localization file loaded.");
        }

        private Dictionary<string, LocalizationEntry> ReadFileWithLangCode(string langCode)
        {
            try
            {
                RichPresencePlugin.PluginLog.Debug($"Reading localization file with language code {langCode}...");
                return JsonConvert.DeserializeObject<Dictionary<string, LocalizationEntry>>(
                    File.ReadAllText(Path.Combine(
                        RichPresencePlugin.DalamudPluginInterface.AssemblyLocation.DirectoryName,
                        "Resources",
                        "loc",
                        $"{PREFIX}{langCode}.json"
                    ))
                );
            }
            catch (Exception ex)
            {
                RichPresencePlugin.PluginLog.Debug(ex, $"File with language code {langCode} not loaded, using fallbacks...");
                return JsonConvert.DeserializeObject<Dictionary<string, LocalizationEntry>>(
                    File.ReadAllText(Path.Combine(
                        RichPresencePlugin.DalamudPluginInterface.AssemblyLocation.DirectoryName,
                        "Resources",
                        "loc",
                        $"{PREFIX}{DEFAULT_DICT_LANGCODE}.json"
                    ))
                );
            }
        }

        private string ClientLanguageToLangCode(ClientLanguage clientLanguage)
        {
            string langCode = clientLanguage switch
            {
                ClientLanguage.Japanese => "ja",
                ClientLanguage.German => "de",
                ClientLanguage.French => "fr",
                _ => "en"
            };
            return langCode;
        }
    }
}

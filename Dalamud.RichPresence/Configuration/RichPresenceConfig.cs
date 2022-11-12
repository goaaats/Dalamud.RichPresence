using Dalamud.Configuration;

namespace Dalamud.RichPresence.Configuration
{
    class RichPresenceConfig : IPluginConfiguration
    {
        public int Version { get; set; } = 1;

        // Show login queue position
        public bool ShowLoginQueuePosition = true;
        // Show character name
        public bool ShowName = true;
        // Show Free Company Tag
        public bool ShowFreeCompany = true;
        // Show world name
        public bool ShowWorld = true;

        // Show elapsed time in zones
        public bool ShowStartTime = false;
        // Reset timer when changing zones
        public bool ResetTimeWhenChangingZones = true;

        // Show current job
        public bool ShowJob = true;
        // Abbreviate current job name
        public bool AbbreviateJob = true;
        // Show current job level
        public bool ShowLevel = true;

        public bool ShowParty = true;

        public bool ShowAfk = true;

        // RPC Bridge for Wine.
        public bool RPCBridgeEnabled = true;
        public string RPCBridgePath = $@"{RichPresencePlugin.DalamudPluginInterface.AssemblyLocation.Directory}\binaries\WineLinuxRPCBridge.exe";
    }
}
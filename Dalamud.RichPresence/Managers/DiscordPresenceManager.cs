using System;
using System.Diagnostics;
using System.IO;
using Dalamud.Utility;
using DiscordRPC;
using DiscordRPC.Logging;

namespace Dalamud.RichPresence.Managers
{
    internal class DiscordPresenceManager : IDisposable
    {
        private DirectoryInfo RPC_BRIDGE_PATH => new(Path.Combine(RichPresencePlugin.DalamudPluginInterface.AssemblyLocation.Directory!.FullName, "Resources/binaries", "WineRPCBridge.exe"));

        private const string DISCORD_CLIENT_ID = "478143453536976896";
        private DiscordRpcClient RpcClient;
        private Process? bridgeProcess;

        internal DiscordPresenceManager()
        {
            this.CreateClient();

            if (Util.IsWine() && RichPresencePlugin.RichPresenceConfig.RPCBridgeEnabled)
            {
                this.StartWineRPCBridge();
            }
        }

        private void CreateClient()
        {
            if (RpcClient is null || RpcClient.IsDisposed)
            {
                // Create new RPC client
                RpcClient = new DiscordRpcClient(DISCORD_CLIENT_ID);

                // Skip identical presences
                RpcClient.SkipIdenticalPresence = true;

                // Set logger
                RpcClient.Logger = new ConsoleLogger { Level = LogLevel.Warning };

                // Subscribe to events
                RpcClient.OnPresenceUpdate += (sender, e) => { System.Console.WriteLine("Received Update! {0}", e.Presence); };
            }

            if (!RpcClient.IsInitialized)
            {
                // Connect to the RPC
                RpcClient.Initialize();
            }
        }

        public void StartWineRPCBridge()
        {
            try
            {
                // Check if bridge is already running.
                var wineBridge = Process.GetProcessesByName(this.RPC_BRIDGE_PATH.Name);
                if (wineBridge.Length > 0)
                {
                    RichPresencePlugin.PluginLog.Information($"Found existing RPC bridge process, PID: {wineBridge[0].Id}, not starting a new one.");
                    this.bridgeProcess = wineBridge[0];
                    return;
                }

                // Start the bridge.
                RichPresencePlugin.PluginLog.Information($"Starting RPC bridge process: {this.RPC_BRIDGE_PATH}");
                this.bridgeProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = this.RPC_BRIDGE_PATH.FullName,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                })!;
                RichPresencePlugin.PluginLog.Information($"Started RPC bridge process, PID: {this.bridgeProcess.Id}");
            }
            catch (Exception e)
            {
                RichPresencePlugin.PluginLog.Error(e, "Error starting Wine bridge process.");
            }
        }

        public void SetPresence(DiscordRPC.RichPresence newPresence)
        {
            this.CreateClient();
            RpcClient.SetPresence(newPresence);
        }

        public void ClearPresence()
        {
            this.CreateClient();
            RpcClient.ClearPresence();
        }

        public void UpdatePresenceDetails(string details)
        {
            this.CreateClient();
            RpcClient.UpdateDetails(details);
        }

        public void UpdatePresenceStartTime(DateTime newStartTime)
        {
            this.CreateClient();
            RpcClient.UpdateStartTime(newStartTime);
        }

        public void Dispose()
        {
            if (this.bridgeProcess is not null)
            {
                this.bridgeProcess.Kill();
                RichPresencePlugin.PluginLog.Information($"Killed RPC bridge process: {this.bridgeProcess.Id}");
            }

            RpcClient?.Dispose();
        }
    }
}
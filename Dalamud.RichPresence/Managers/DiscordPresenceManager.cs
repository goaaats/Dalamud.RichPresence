using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Dalamud.Logging;
using DiscordRPC;
using DiscordRPC.Logging;
using RichPresencePlugin.Utils;

namespace Dalamud.RichPresence.Managers
{
    internal class DiscordPresenceManager : IDisposable
    {
        private const string DISCORD_CLIENT_ID = "478143453536976896";
        private DiscordRpcClient RpcClient;
        private Process bridgeProcess;

        internal DiscordPresenceManager()
        {
            this.CreateClient();

            if (CommonUtil.IsOnLinuxOrWine() && RichPresencePlugin.RichPresenceConfig.RPCBridgeEnabled)
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
                RpcClient.OnPresenceUpdate += (sender, e) => { Console.WriteLine("Received Update! {0}", e.Presence); };
            }

            if (!RpcClient.IsInitialized)
            {
                // Connect to the RPC
                RpcClient.Initialize();
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

        public void StartWineRPCBridge()
        {
            try
            {
                var bridgeExecutableName = new DirectoryInfo(RichPresencePlugin.RichPresenceConfig.RPCBridgePath).Name;
                var bridgeExecutablePath = RichPresencePlugin.RichPresenceConfig.RPCBridgePath;

                // Check if bridge is already running.
                var wineBridge = Process.GetProcessesByName(bridgeExecutableName);
                if (wineBridge.Length > 0)
                {
                    PluginLog.LogInformation($"Found existing Wine bridge process, PID: {wineBridge[0].Id}, not starting a new one.");
                    bridgeProcess = wineBridge[0];
                    return;
                }

                PluginLog.LogInformation($"Starting Wine bridge process: {bridgeExecutablePath}");
                this.bridgeProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = bridgeExecutablePath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                });
                PluginLog.LogInformation($"Started Wine bridge process, PID: {bridgeProcess.Id}");
            }
            catch (Exception e)
            {
                PluginLog.LogError(e, "Error starting Wine bridge process.");
            }
        }

        public void Dispose()
        {
            RpcClient?.Dispose();

            PluginLog.LogInformation("Killing Wine bridge process.");
            bridgeProcess?.Kill();

        }
    }
}
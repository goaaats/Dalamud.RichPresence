using System;

using RichPresencePlugin.Utils;
using DiscordRPC;
using DiscordRPC.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using Dalamud.Logging;

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

                // Start a new bridge process.
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = bridgeExecutablePath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                Process.Start(startInfo);
                PluginLog.LogInformation($"Starting Wine bridge process: {bridgeExecutablePath} {startInfo.Arguments}");

                // Setup a task that waits for the bridge to be active, and bind it to bridgeProcess.
                new Task(() =>
                {
                    var bridge = Process.GetProcessesByName(bridgeExecutableName);
                    while (bridge.Length == 0)
                    {
                        bridge = Process.GetProcessesByName(bridgeExecutableName);
                    }
                    bridgeProcess = bridge[0];
                }).Start();
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
using System;

using DiscordRPC;
using DiscordRPC.Logging;

namespace Dalamud.RichPresence.Managers
{
    internal class DiscordPresenceManager : IDisposable
    {
        private const string DISCORD_CLIENT_ID = "478143453536976896";
        private DiscordRpcClient RpcClient;

        internal DiscordPresenceManager()
        {
            this.CreateClient();
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
            RpcClient?.Dispose();
        }
    }
}
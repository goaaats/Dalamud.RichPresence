using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordRPC;
using DiscordRPC.Logging;

namespace Dalamud.RichPresence
{
    class DiscordPresenceManager : IDisposable
    {
        private readonly DiscordRPC.RichPresence _initialPresence;
        private readonly string _clientId;
        private DiscordRpcClient _rpcClient;

        public DiscordPresenceManager(DiscordRPC.RichPresence initialPresence, string clientId)
        {
            _initialPresence = initialPresence;
            _clientId = clientId;

            CreateClient();
        }

        private void CreateClient()
        {
            _rpcClient = new DiscordRpcClient(_clientId);

            //Set the logger
            _rpcClient.Logger = new ConsoleLogger { Level = LogLevel.Warning };

            //Subscribe to events
            _rpcClient.OnPresenceUpdate += (sender, e) => { Console.WriteLine("Received Update! {0}", e.Presence); };

            //Connect to the RPC
            _rpcClient.Initialize();

            _rpcClient.SetPresence(_initialPresence);
        }

        public void Update()
        {
            //Invoke all the events, such as OnPresenceUpdate
            _rpcClient.Invoke();
        }

        public void SetPresence(DiscordRPC.RichPresence presence)
        {
            if (_rpcClient.IsDisposed)
                CreateClient();

            if (presence.State != _rpcClient.CurrentPresence.State ||
                presence.Details != _rpcClient.CurrentPresence.Details ||
                presence.Assets.SmallImageText != _rpcClient.CurrentPresence.Assets.SmallImageText ||
                presence.Assets.LargeImageText != _rpcClient.CurrentPresence.Assets.LargeImageText)
                _rpcClient.SetPresence(presence);
        }

        public void Dispose()
        {
            _rpcClient?.Dispose();
        }
    }
}

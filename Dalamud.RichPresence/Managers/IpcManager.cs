using System;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;

namespace Dalamud.RichPresence.Managers
{
    internal class IpcManager : IDisposable
    {
        // Waitingway IPCs
        private readonly ICallGateSubscriber<int?> wwQueueType;
        private readonly ICallGateSubscriber<int?> wwCurrentPosition;
        // private readonly ICallGateSubscriber<TimeSpan?> wwElapsedTime;
        private readonly ICallGateSubscriber<TimeSpan?> wwEstimatedTimeRemaining;

        public IpcManager()
        {
            wwQueueType = RichPresencePlugin.DalamudPluginInterface.GetIpcSubscriber<int?>("Waitingway.QueueType");
            wwCurrentPosition = RichPresencePlugin.DalamudPluginInterface.GetIpcSubscriber<int?>("Waitingway.CurrentPosition");
            //wwElapsedTime =
            //    RichPresencePlugin.DalamudPluginInterface.GetIpcSubscriber<TimeSpan?>("Waitingway.ElapsedTime");
            wwEstimatedTimeRemaining =
                RichPresencePlugin.DalamudPluginInterface.GetIpcSubscriber<TimeSpan?>("Waitingway.EstimatedTimeRemaining");
        }

        public bool IsInLoginQueue()
        {
            try
            {
                // We only care about login queues
                return wwQueueType.InvokeFunc() == 1;
            }
            catch (IpcNotReadyError)
            {
                return false;
            }
        }

        public int GetQueuePosition()
        {
            try
            {
                return wwCurrentPosition.InvokeFunc() ?? -1;
            }
            catch (IpcNotReadyError)
            {
                return -1;
            }
        }

        public TimeSpan? GetQueueEstimate()
        {
            try
            {
                return wwEstimatedTimeRemaining.InvokeFunc();
            }
            catch (IpcNotReadyError)
            {
                return null;
            }
        }

        public void Dispose()
        {
        }
    }
}
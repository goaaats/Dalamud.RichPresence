using System;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;

namespace Dalamud.RichPresence.Managers
{
    internal class IpcManager : IDisposable
    {
        // Waitingway IPCs
        private readonly ICallGateSubscriber<bool> wwIsInQueue;
        private readonly ICallGateSubscriber<int> wwGetQueueType;
        private readonly ICallGateSubscriber<int> wwGetQueuePosition;
        private readonly ICallGateSubscriber<TimeSpan?> wwGetQueueEstimate;

        public IpcManager()
        {
            wwIsInQueue = RichPresencePlugin.DalamudPluginInterface.GetIpcSubscriber<bool>("Waitingway.IsInQueue");
            wwGetQueueType = RichPresencePlugin.DalamudPluginInterface.GetIpcSubscriber<int>("Waitingway.GetQueueType");
            wwGetQueuePosition =
                RichPresencePlugin.DalamudPluginInterface.GetIpcSubscriber<int>("Waitingway.GetQueuePosition");
            wwGetQueueEstimate =
                RichPresencePlugin.DalamudPluginInterface.GetIpcSubscriber<TimeSpan?>("Waitingway.GetQueueEstimate");
        }

        public bool IsInLoginQueue()
        {
            try
            {
                // We only care about login queues
                return wwIsInQueue.InvokeFunc() && wwGetQueueType.InvokeFunc() == 1;
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
                return wwGetQueuePosition.InvokeFunc();
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
                return wwGetQueueEstimate.InvokeFunc();
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
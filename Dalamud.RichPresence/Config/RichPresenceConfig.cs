using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Configuration;

namespace Dalamud.RichPresence.Config
{
    class RichPresenceConfig : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool ShowName = true;
        public bool ShowWorld = true;

        public bool ShowStartTime = false;
        public bool ResetTimeWhenChangingZones = true;
    }
}

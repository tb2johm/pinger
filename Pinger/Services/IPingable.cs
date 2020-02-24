using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Pinger.Services
{
    public interface IPingable
    {
        void ConfigPingServer(PingConfig config);
        void StartPingServer();
        void StopPingServer();
        bool Running { get; }

        event EventHandler PingStatsUpdated;
        event PingCompletedEventHandler PingCompleted;
    }
}

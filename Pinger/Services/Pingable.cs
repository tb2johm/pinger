using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Timers;

namespace Pinger.Services
{
    public class Pingable : IPingable
    {
        private PingConfig _pingConfig;
        private Timer _timer;

        public event EventHandler PingStatsUpdated;
        public event PingCompletedEventHandler PingCompleted;

        public bool Running { get; private set; }

        public Pingable()
        {
            _timer = new Timer();
            _timer.Elapsed += timer_Elapsed;
        }

        public void ConfigPingServer(PingConfig config)
        {
            if (Running)
            {
                throw new Exception("Can't configure while server is running");
            }

            _pingConfig = config;
            _timer.Interval = config.Interval;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        public void StartPingServer()
        {
            _timer.Start();
            Running = true;
            var ea = new EventArgs();
            PingStatsUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void StopPingServer()
        {
            _timer.Stop();
            Running = false;
            PingStatsUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SendPing();

            PingStatsUpdated?.Invoke(this, EventArgs.Empty);
        }

        private bool SendPing()
        {
            using (var ping = new Ping())
            {
                var data = "aaaa";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                ping.PingCompleted += PingCompleted;
                ping.SendAsync(_pingConfig.Address, _pingConfig.Timeout, buffer, _pingConfig.PingOptions);
            }

            return false;
        }
    }
}

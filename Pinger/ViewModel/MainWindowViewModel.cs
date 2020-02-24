using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using MMVVM.Commands;
using Pinger.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Pinger.ViewModel
{
    public class MainWindowViewModel : MMVVM.ViewModelBase.ViewModelBase
    {
        private readonly IPingable _pingable;
        private List<bool> _success;

        public PingConfig Config { get; set; }
        public bool PingServiceRunning => _pingable.Running;
        public string Status { get; set; }
        public RelayCommand StartCommand { get; private set; }
        public RelayCommand StopCommand { get; private set; }

        // Graph
        public SeriesCollection SeriesCollection { get; private set; }
        public Func<double, string> XFormatter { get; private set; }
        public DateTime[] Labels { get; private set; }
        public long AxisMax { get; private set; }
        public long AxisMin { get; private set; }


        public MainWindowViewModel() : this(default) { }

        public MainWindowViewModel(IPingable pingable = default)
        {
            _pingable = pingable;

            if (pingable == default)
            {
                _pingable = new Pingable();
            }

            Config = new PingConfig();
            Config.Address = "192.168.229.95"; // "192.168.229.86";
            Config.Interval = 2000;
            Config.Timeout = 100;

            _pingable.PingStatsUpdated += pingable_PingStatsUpdated;
            _pingable.PingCompleted += pingable_PingCompleted;

            StartCommand = new RelayCommand(StartAction, () => !PingServiceRunning);
            StopCommand = new RelayCommand(StopAction, () => PingServiceRunning);

            SeriesCollection = new SeriesCollection { new ScatterSeries { Title = "Successful RTT", Values = new ChartValues<DateTimePoint>() },
                                                      new ScatterSeries { Title = "Failures", Values = new ChartValues<DateTimePoint>() } };

            _success = new List<bool>();

            /*SeriesCollection[0].Values.Add(new DateTimePoint(DateTime.Now, 0));
            _success.Add(true);
            SeriesCollection[1].Values.Add(new DateTimePoint(DateTime.Now, 0));
            _success.Add(false);*/
            AxisMin = DateTime.Now.AddSeconds(-5).Ticks;
            AxisMax = DateTime.Now.AddSeconds(1).Ticks;
            XFormatter = (val) => new DateTime((long)val).ToString("HH:mm:ss");
        }


        private void pingable_PingStatsUpdated(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StartCommand.RaiseCanExecuteChanged();
                StopCommand.RaiseCanExecuteChanged();
            });
        }
        private void pingable_PingCompleted(object sender, PingCompletedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var reply = e.Reply;
                var point = new DateTimePoint(DateTime.Now, reply.RoundtripTime);

                if (reply.Status == IPStatus.Success)
                {
                    Status += $"{point.DateTime.ToString("HH:mm:ss")} - ping pong\n";
                    SeriesCollection[0].Values.Add(point);
                    _success.Add(true);
                } else {
                    Status += $"{point.DateTime.ToString("HH:mm:ss.")} - fail! {reply.Status}\n";
                    SeriesCollection[1].Values.Add(point);
                    _success.Add(false);
                }

                if (_success.Count > Math.Max(3600000 / Config.Interval, 3600))
                {
                    var index = _success[0] ? 0 : 1;
                    SeriesCollection[index].Values.RemoveAt(0);
                    _success.RemoveAt(0);
                    AxisMin = ((DateTimePoint)SeriesCollection[index].Values[0]).DateTime.AddSeconds(-5).Ticks;
                    Notify("AxisMin");
                }

                AxisMax = point.DateTime.AddSeconds(1).Ticks;
                Notify("AxisMax");

                Notify("Status");
            });
        }

        private void StartAction()
        {
            _pingable.ConfigPingServer(Config);
            _pingable.StartPingServer();
        }

        private void StopAction()
        {
            _pingable.StopPingServer();
        }
    }
}

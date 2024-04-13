using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System;
using System.Linq;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class PulsoidServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public int CommandTriggerDelay
        {
            get { return ChannelSession.Settings.PulsoidCommandTriggerDelay; }
            set
            {
                ChannelSession.Settings.PulsoidCommandTriggerDelay = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }

        public string HeartRateRanges
        {
            get
            {
                if (ChannelSession.Settings.PulsoidCommandHeartRateRangeTriggers.Count > 0)
                {
                    return string.Join(",", ChannelSession.Settings.PulsoidCommandHeartRateRangeTriggers.Select(range => (range.Item1 == range.Item2) ? range.Item1.ToString() : $"{range.Item1}-{range.Item2}"));
                }
                return string.Empty;
            }
            set
            {
                ChannelSession.Settings.PulsoidCommandHeartRateRangeTriggers.Clear();
                if (!string.IsNullOrEmpty(value))
                {
                    string[] splits = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    if (splits != null)
                    {
                        foreach (string split in splits)
                        {
                            if (split.Contains("-"))
                            {
                                string[] numbers = split.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                                if (numbers != null && numbers.Length == 2)
                                {
                                    if (int.TryParse(numbers[0], out int number1) && number1 > 0 && int.TryParse(numbers[1], out int number2) && number2 > 0)
                                    {
                                        ChannelSession.Settings.PulsoidCommandHeartRateRangeTriggers.Add(new Tuple<int, int>(number1, number2));
                                    }
                                }
                            }
                            else
                            {
                                if (int.TryParse(split, out int number) && number > 0)
                                {
                                    ChannelSession.Settings.PulsoidCommandHeartRateRangeTriggers.Add(new Tuple<int, int>(number, number));
                                }
                            }
                        }
                    }
                }
            }
        }

        public override string WikiPageName { get { return "pulsoid"; } }

        public PulsoidServiceControlViewModel()
            : base(Resources.Pulsoid)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                Result result = await ServiceManager.Get<PulsoidService>().Connect();
                if (result.Success)
                {
                    this.IsConnected = true;
                }
                else
                {
                    await ServiceManager.Get<PulsoidService>().Disconnect();
                    ChannelSession.Settings.PulsoidOAuthToken = null;

                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.LogOutCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<PulsoidService>().Disconnect();

                ChannelSession.Settings.PulsoidOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<PulsoidService>().IsConnected;
        }
    }
}

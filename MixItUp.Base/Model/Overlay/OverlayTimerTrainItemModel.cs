using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayTimerTrainItemModel : OverlayHTMLTemplateItemModelBase, IDisposable
    {
        public const string HTMLTemplate = @"<p style=""position: absolute; font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(-50%, -50%);"">{TIME}</p>";

        [DataMember]
        public int MinimumSecondsToShow { get; set; }

        [DataMember]
        public string TextColor { get; set; }
        [DataMember]
        public string TextFont { get; set; }
        [DataMember]
        public int TextSize { get; set; }

        [DataMember]
        public double FollowBonus { get; set; }
        [DataMember]
        public double HostBonus { get; set; }
        [DataMember]
        public double SubscriberBonus { get; set; }
        [DataMember]
        public double DonationBonus { get; set; }
        [DataMember]
        public double SparkBonus { get; set; }
        [DataMember]
        public double EmberBonus { get; set; }

        [JsonIgnore]
        private int timeLeft;
        [JsonIgnore]
        private int stackedTime;

        [JsonIgnore]
        private CancellationTokenSource backgroundThreadCancellationTokenSource = new CancellationTokenSource();

        [JsonIgnore]
        private HashSet<uint> follows = new HashSet<uint>();
        [JsonIgnore]
        private HashSet<uint> hosts = new HashSet<uint>();
        [JsonIgnore]
        private HashSet<uint> subs = new HashSet<uint>();

        [JsonIgnore]
        private SemaphoreSlim timeSemaphore = new SemaphoreSlim(1);

        public OverlayTimerTrainItemModel() : base() { }

        public OverlayTimerTrainItemModel(string htmlText, int minimumSecondsToShow, string textColor, string textFont, int textSize, double followBonus,
            double hostBonus, double subscriberBonus, double donationBonus, double sparkBonus, double emberBonus)
            : base(OverlayItemModelTypeEnum.TimerTrain, htmlText)
        {
            this.MinimumSecondsToShow = minimumSecondsToShow;
            this.TextColor = textColor;
            this.TextFont = textFont;
            this.TextSize = textSize;
            this.FollowBonus = followBonus;
            this.HostBonus = hostBonus;
            this.SubscriberBonus = subscriberBonus;
            this.DonationBonus = donationBonus;
            this.SparkBonus = sparkBonus;
            this.EmberBonus = emberBonus;
        }

        [JsonIgnore]
        public override bool SupportsTestData { get { return false; } }

        public override async Task Initialize()
        {
            if (this.FollowBonus > 0.0)
            {
                GlobalEvents.OnFollowOccurred += GlobalEvents_OnFollowOccurred;
            }
            if (this.HostBonus > 0.0)
            {
                GlobalEvents.OnHostOccurred += GlobalEvents_OnHostOccurred;
            }
            if (this.SubscriberBonus > 0.0)
            {
                GlobalEvents.OnSubscribeOccurred += GlobalEvents_OnSubscribeOccurred;
                GlobalEvents.OnResubscribeOccurred += GlobalEvents_OnResubscribeOccurred;
            }
            if (this.DonationBonus > 0.0)
            {
                GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
            }
            if (this.SparkBonus > 0.0)
            {
                GlobalEvents.OnSparkUseOccurred += GlobalEvents_OnSparkUseOccurred;
            }
            if (this.EmberBonus > 0.0)
            {
                GlobalEvents.OnEmberUseOccurred += GlobalEvents_OnEmberUseOccurred;
            }

            this.timeLeft = 0;
            this.stackedTime = 0;

            this.backgroundThreadCancellationTokenSource = new CancellationTokenSource();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () => { await this.TimerBackground(); }, this.backgroundThreadCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            await base.Initialize();
        }

        public override async Task Disable()
        {
            GlobalEvents.OnFollowOccurred -= GlobalEvents_OnFollowOccurred;
            GlobalEvents.OnHostOccurred -= GlobalEvents_OnHostOccurred;
            GlobalEvents.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            GlobalEvents.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            GlobalEvents.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;
            GlobalEvents.OnSparkUseOccurred -= GlobalEvents_OnSparkUseOccurred;
            GlobalEvents.OnEmberUseOccurred -= GlobalEvents_OnEmberUseOccurred;

            this.timeLeft = 0;
            this.stackedTime = 0;

            if (this.backgroundThreadCancellationTokenSource != null)
            {
                this.backgroundThreadCancellationTokenSource.Cancel();
                this.backgroundThreadCancellationTokenSource = null;
            }

            await base.Disable();
        }

        protected override async Task PerformReplacements(JObject jobj, UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            await base.PerformReplacements(jobj, user, arguments, extraSpecialIdentifiers);
            if (this.timeLeft == 0)
            {
                jobj["HTML"] = "";
            }
        }

        protected override Task<Dictionary<string, string>> GetTemplateReplacements(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["TEXT_SIZE"] = this.TextSize.ToString();
            replacementSets["TIME"] = TimeSpan.FromSeconds(this.timeLeft).ToString("hh\\:mm\\:ss");

            return Task.FromResult(replacementSets);
        }
        
        private async void GlobalEvents_OnFollowOccurred(object sender, UserViewModel user)
        {
            if (!this.follows.Contains(user.ID))
            {
                this.follows.Add(user.ID);
                await this.AddSeconds(this.FollowBonus);
            }
        }

        private async void GlobalEvents_OnHostOccurred(object sender, Tuple<UserViewModel, int> host)
        {
            if (!this.hosts.Contains(host.Item1.ID))
            {
                this.hosts.Add(host.Item1.ID);
                await this.AddSeconds(Math.Max(host.Item2, 1) * this.HostBonus);
            }
        }

        private async void GlobalEvents_OnSubscribeOccurred(object sender, UserViewModel user)
        {
            if (!this.subs.Contains(user.ID))
            {
                this.subs.Add(user.ID);
                await this.AddSeconds(this.SubscriberBonus);
            }
        }

        private async void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserViewModel, int> user)
        {
            if (!this.subs.Contains(user.Item1.ID))
            {
                this.subs.Add(user.Item1.ID);
                await this.AddSeconds(this.SubscriberBonus);
            }
        }

        private async void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation) { await this.AddSeconds(donation.Amount * this.DonationBonus); }

        private async void GlobalEvents_OnSparkUseOccurred(object sender, Tuple<UserViewModel, int> sparkUsage) { await this.AddSeconds(sparkUsage.Item2 * this.SparkBonus); }

        private async void GlobalEvents_OnEmberUseOccurred(object sender, UserEmberUsageModel emberUsage) { await this.AddSeconds(emberUsage.Amount * this.EmberBonus); }

        private async Task AddSeconds(double seconds)
        {
            await this.timeSemaphore.WaitAndRelease(() =>
            {
                if (this.timeLeft > 0)
                {
                    this.timeLeft += (int)Math.Round(seconds);
                }
                else
                {
                    this.stackedTime += (int)Math.Round(seconds);
                    if (this.stackedTime >= this.MinimumSecondsToShow)
                    {
                        this.timeLeft += this.stackedTime;
                        this.stackedTime = 0;
                    }
                }
                return Task.FromResult(0);
            });

            if (this.timeLeft > 0)
            {
                this.SendUpdateRequired();
            }
        }

        private async Task TimerBackground()
        {
            await BackgroundTaskWrapper.RunBackgroundTask(this.backgroundThreadCancellationTokenSource, async (token) =>
            {
                await Task.Delay(1000);

                if (this.timeLeft > 0)
                {
                    this.timeLeft--;
                    if (this.timeLeft == 0)
                    {
                        this.SendHide();
                    }
                    else
                    {
                        this.SendUpdateRequired();
                    }
                }
                else
                {
                    if (this.stackedTime > 0)
                    {
                        this.stackedTime--;
                    }
                }
            });
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    this.backgroundThreadCancellationTokenSource.Dispose();
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}

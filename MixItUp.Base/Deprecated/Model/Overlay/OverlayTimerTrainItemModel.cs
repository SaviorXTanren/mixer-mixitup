using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Twitch.Bits;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.Twitch.New;
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
    [Obsolete]
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
        public double RaidBonus { get; set; }
        [DataMember]
        public double SubscriberBonus { get; set; }
        [DataMember]
        public double DonationBonus { get; set; }
        [DataMember]
        public double BitsBonus { get; set; }

        [JsonIgnore]
        private int timeLeft;
        [JsonIgnore]
        private int stackedTime;

        [JsonIgnore]
        private CancellationTokenSource backgroundThreadCancellationTokenSource = new CancellationTokenSource();

        [JsonIgnore]
        private HashSet<Guid> follows = new HashSet<Guid>();
        [JsonIgnore]
        private HashSet<Guid> hosts = new HashSet<Guid>();
        [JsonIgnore]
        private HashSet<Guid> raids = new HashSet<Guid>();
        [JsonIgnore]
        private HashSet<Guid> subs = new HashSet<Guid>();

        [JsonIgnore]
        private SemaphoreSlim timeSemaphore = new SemaphoreSlim(1);

        public OverlayTimerTrainItemModel() : base() { }

        public OverlayTimerTrainItemModel(string htmlText, int minimumSecondsToShow, string textColor, string textFont, int textSize, double followBonus,
            double hostBonus, double raidBonus, double subscriberBonus, double donationBonus, double bitsBonus)
            : base(OverlayItemModelTypeEnum.TimerTrain, htmlText)
        {
            this.MinimumSecondsToShow = minimumSecondsToShow;
            this.TextColor = textColor;
            this.TextFont = textFont;
            this.TextSize = textSize;
            this.FollowBonus = followBonus;
            this.HostBonus = hostBonus;
            this.RaidBonus = raidBonus;
            this.SubscriberBonus = subscriberBonus;
            this.DonationBonus = donationBonus;
            this.BitsBonus = bitsBonus;
        }

        [JsonIgnore]
        public override bool SupportsTestData { get { return false; } }

        public override async Task Enable()
        {
            if (this.FollowBonus > 0.0)
            {
                EventService.OnFollowOccurred += GlobalEvents_OnFollowOccurred;
            }
            if (this.HostBonus > 0.0)
            {

            }
            if (this.RaidBonus > 0.0)
            {
                EventService.OnRaidOccurred += GlobalEvents_OnRaidOccurred;
            }
            if (this.SubscriberBonus > 0.0)
            {
                //EventService.OnSubscribeOccurred += GlobalEvents_OnSubscribeOccurred;
                //EventService.OnResubscribeOccurred += GlobalEvents_OnResubscribeOccurred;
                //EventService.OnSubscriptionGiftedOccurred += GlobalEvents_OnSubscriptionGiftedOccurred;
            }
            if (this.DonationBonus > 0.0)
            {
                EventService.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
            }
            if (this.BitsBonus > 0.0)
            {
                EventService.OnTwitchBitsCheeredOccurred += GlobalEvents_OnBitsOccurred;
            }

            this.timeLeft = 0;
            this.stackedTime = 0;

            this.backgroundThreadCancellationTokenSource = new CancellationTokenSource();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            AsyncRunner.RunAsyncBackground(this.TimerBackground, this.backgroundThreadCancellationTokenSource.Token, 1000);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            await base.Enable();
        }

        public override async Task Disable()
        {
            EventService.OnFollowOccurred -= GlobalEvents_OnFollowOccurred;
            //EventService.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            //EventService.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            //EventService.OnSubscriptionGiftedOccurred -= GlobalEvents_OnSubscriptionGiftedOccurred;
            EventService.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;
            EventService.OnTwitchBitsCheeredOccurred -= GlobalEvents_OnBitsOccurred;

            this.timeLeft = 0;
            this.stackedTime = 0;

            if (this.backgroundThreadCancellationTokenSource != null)
            {
                this.backgroundThreadCancellationTokenSource.Cancel();
                this.backgroundThreadCancellationTokenSource = null;
            }

            await base.Disable();
        }

        protected override async Task PerformReplacements(JObject jobj, CommandParametersModel parameters)
        {
            await base.PerformReplacements(jobj, parameters);
            if (this.timeLeft == 0)
            {
                jobj["HTML"] = "";
            }
        }

        protected override Task<Dictionary<string, string>> GetTemplateReplacements(CommandParametersModel parameters)
        {
            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["TEXT_SIZE"] = this.TextSize.ToString();
            replacementSets["TIME"] = TimeSpan.FromSeconds(this.timeLeft).ToString("hh\\:mm\\:ss");

            return Task.FromResult(replacementSets);
        }

        private async void GlobalEvents_OnFollowOccurred(object sender, UserV2ViewModel user)
        {
            if (!this.follows.Contains(user.ID))
            {
                this.follows.Add(user.ID);
                await this.AddSeconds(this.FollowBonus);
            }
        }

        private async void GlobalEvents_OnHostOccurred(object sender, UserV2ViewModel host)
        {
            if (!this.hosts.Contains(host.ID))
            {
                this.hosts.Add(host.ID);
                await this.AddSeconds(this.HostBonus);
            }
        }

        private async void GlobalEvents_OnRaidOccurred(object sender, Tuple<UserV2ViewModel, int> raid)
        {
            if (!this.raids.Contains(raid.Item1.ID))
            {
                this.raids.Add(raid.Item1.ID);
                await this.AddSeconds(Math.Max(raid.Item2, 1) * this.RaidBonus);
            }
        }

        private async void GlobalEvents_OnSubscribeOccurred(object sender, UserV2ViewModel user)
        {
            if (!this.subs.Contains(user.ID))
            {
                this.subs.Add(user.ID);
                await this.AddSeconds(this.SubscriberBonus);
            }
        }

        private async void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserV2ViewModel, int> user)
        {
            if (!this.subs.Contains(user.Item1.ID))
            {
                this.subs.Add(user.Item1.ID);
                await this.AddSeconds(this.SubscriberBonus);
            }
        }

        private async void GlobalEvents_OnSubscriptionGiftedOccurred(object sender, Tuple<UserV2ViewModel, UserV2ViewModel> e)
        {
            if (!this.subs.Contains(e.Item2.ID))
            {
                this.subs.Add(e.Item2.ID);
                await this.AddSeconds(this.SubscriberBonus);
            }
        }

        private async void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation) { await this.AddSeconds(donation.Amount * this.DonationBonus); }

        private async void GlobalEvents_OnBitsOccurred(object sender, TwitchBitsCheeredEventModel e) { await this.AddSeconds(e.Amount * this.BitsBonus); }

        private async Task AddSeconds(double seconds)
        {
            await this.timeSemaphore.WaitAsync();

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

            this.timeSemaphore.Release();

            if (this.timeLeft > 0)
            {
                this.SendUpdateRequired();
            }
        }

        private Task TimerBackground(CancellationToken token)
        {
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
            return Task.CompletedTask;
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
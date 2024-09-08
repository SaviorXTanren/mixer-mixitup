using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.Text.Json.Serialization;
using MixItUp.Base.Util;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayPersistentTimerV3Model : OverlayEventCountingV3ModelBase
    {
        public const string SecondsProperty = "Seconds";

        public const string TimerSpecialIdentifierPrefix = "timer";
        public const string TimerSecondsAddedSpecialIdentifierPrefix = TimerSpecialIdentifierPrefix + "secondsadded";

        public static readonly string DefaultHTML = OverlayResources.OverlayTimerDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayTextDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayPersistentTimerDefaultJavascript;

        [DataMember]
        public int InitialAmount { get; set; }

        [DataMember]
        public string DisplayFormat { get; set; }

        [DataMember]
        public int MaxAmount { get; set; }

        [DataMember]
        public bool DisableOnCompletion { get; set; }
        [DataMember]
        public bool ResetOnEnable { get; set; }
        [DataMember]
        public bool AllowAdjustmentWhilePaused { get; set; }

        [DataMember]
        public OverlayAnimationV3Model TimerAdjustedAnimation { get; set; } = new OverlayAnimationV3Model();
        [DataMember]
        public OverlayAnimationV3Model TimerCompletedAnimation { get; set; } = new OverlayAnimationV3Model();

        [DataMember]
        public Guid TimerAdjustedCommandID { get; set; }
        [DataMember]
        public Guid TimerCompletedCommandID { get; set; }

        [DataMember]
        public int CurrentAmount { get; set; }

        [JsonIgnore]
        private CancellationTokenSource cancellationTokenSource;
        [JsonIgnore]
        private bool paused;

        public OverlayPersistentTimerV3Model() : base(OverlayItemV3Type.PersistentTimer) { }

        public override async Task Initialize()
        {
            await base.Initialize();

            this.StartBackgroundTimer();

            this.paused = false;
        }

        public override async Task Uninitialize()
        {
            await base.Uninitialize();

            if (this.cancellationTokenSource != null)
            {
                this.cancellationTokenSource.Cancel();
                this.cancellationTokenSource = null;
            }

            if (this.ResetOnEnable)
            {
                this.CurrentAmount = this.InitialAmount;
            }
        }

        public override async Task Reset()
        {
            await base.Reset();

            this.CurrentAmount = this.InitialAmount;
        }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();

            properties[nameof(this.CurrentAmount)] = this.CurrentAmount;
            properties[nameof(this.DisplayFormat)] = this.DisplayFormat;

            OverlayItemV3ModelBase.AddAnimationProperties(properties, nameof(this.TimerAdjustedAnimation), this.TimerAdjustedAnimation);
            OverlayItemV3ModelBase.AddAnimationProperties(properties, nameof(this.TimerCompletedAnimation), this.TimerCompletedAnimation);

            return properties;
        }

        public override async Task ProcessEvent(UserV2ViewModel user, double amount)
        {
            if (!this.paused || this.AllowAdjustmentWhilePaused)
            {
                amount = Math.Round(amount);
                if (this.MaxAmount > 0 && amount > 0)
                {
                    int previousAmount = this.CurrentAmount;
                    this.CurrentAmount = Math.Min(this.CurrentAmount + (int)amount, this.MaxAmount);
                    amount = this.CurrentAmount - previousAmount;
                }
                else
                {
                    this.CurrentAmount += (int)amount;
                    this.CurrentAmount = Math.Max(this.CurrentAmount, 0);
                }

                if (amount != 0)
                {
                    Dictionary<string, object> properties = new Dictionary<string, object>();
                    properties[SecondsProperty] = amount;
                    await this.CallFunction("adjustTime", properties);

                    Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
                    specialIdentifiers[TimerSecondsAddedSpecialIdentifierPrefix] = amount.ToString();

                    await ServiceManager.Get<CommandService>().Queue(this.TimerAdjustedCommandID, new CommandParametersModel(user, specialIdentifiers));
                }
            }
        }

        public async Task Pause()
        {
            if (!this.paused)
            {
                this.paused = true;
                if (this.cancellationTokenSource != null)
                {
                    this.cancellationTokenSource.Cancel();
                    this.cancellationTokenSource = null;
                }
                await this.CallFunction("pause", new Dictionary<string, object>());
            }
        }

        public async Task Unpause()
        {
            if (this.paused)
            {
                this.paused = false;
                this.StartBackgroundTimer();
                await this.CallFunction("unpause", new Dictionary<string, object>());
            }
        }

        private void StartBackgroundTimer()
        {
            this.cancellationTokenSource = new CancellationTokenSource();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            AsyncRunner.RunAsyncBackground(this.BackgroundTimer, this.cancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async Task BackgroundTimer(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000);

                if (this.CurrentAmount > 0)
                {
                    this.CurrentAmount--;
                    if (this.CurrentAmount == 0)
                    {
                        await ServiceManager.Get<CommandService>().Queue(this.TimerCompletedCommandID);
                        if (this.DisableOnCompletion)
                        {
                            await ServiceManager.Get<OverlayV3Service>().GetWidget(this.ID).Disable();
                        }
                    }
                }
            }
        }
    }
}

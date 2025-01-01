using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayPersistentTimerV3Model : OverlayEventCountingV3ModelBase
    {
        public const string SecondsProperty = "Seconds";

        public const string TimerSpecialIdentifierPrefix = "timer";
        public const string TimerSecondsAdjustedSpecialIdentifierPrefix = TimerSpecialIdentifierPrefix + "secondsadjusted";

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

        [JsonIgnore]
        private object amountLock = new object();

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

        public override void ImportReset()
        {
            base.ImportReset();

            this.TimerAdjustedCommandID = Guid.Empty;
            this.TimerCompletedCommandID = Guid.Empty;
        }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();

            properties[nameof(this.CurrentAmount)] = this.CurrentAmount;
            properties[nameof(this.DisplayFormat)] = this.DisplayFormat;

            this.TimerAdjustedAnimation.AddAnimationProperties(properties, nameof(this.TimerAdjustedAnimation));
            this.TimerCompletedAnimation.AddAnimationProperties(properties, nameof(this.TimerCompletedAnimation));

            return properties;
        }

        public override async Task ProcessEvent(UserV2ViewModel user, double amount)
        {
            if (!this.paused || this.AllowAdjustmentWhilePaused)
            {
                amount = Math.Round(amount);

                Logger.Log(LogLevel.Debug, $"Processing timer amount - {amount}");

                lock (amountLock)
                {
                    if (this.MaxAmount > 0 && amount > 0)
                    {
                        int previousAmount = this.CurrentAmount;
                        this.CurrentAmount = Math.Min(this.CurrentAmount + (int)amount, this.MaxAmount);
                        amount = this.CurrentAmount - previousAmount;
                    }
                    else
                    {
                        this.CurrentAmount += (int)amount;
                    }

                    this.CurrentAmount = Math.Max(this.CurrentAmount, 1);
                }

                Logger.Log(LogLevel.Debug, $"New timer amount - {this.CurrentAmount}");

                if (amount != 0)
                {
                    Dictionary<string, object> properties = new Dictionary<string, object>();
                    properties[SecondsProperty] = amount;
                    await this.CallFunction("adjustTime", properties);

                    Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
                    specialIdentifiers[TimerSecondsAdjustedSpecialIdentifierPrefix] = amount.ToString();

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
            if (this.cancellationTokenSource != null)
            {
                this.cancellationTokenSource.Cancel();
            }
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

                bool timerCompleted = false;
                lock (amountLock)
                {
                    if (this.CurrentAmount > 0)
                    {
                        this.CurrentAmount--;
                        if (this.CurrentAmount == 0)
                        {
                            timerCompleted = true;
                        }
                    }
                }

                if (timerCompleted)
                {
                    await ServiceManager.Get<CommandService>().Queue(this.TimerCompletedCommandID);
                    if (this.DisableOnCompletion)
                    {
                        OverlayWidgetV3Model widget = ServiceManager.Get<OverlayV3Service>().GetWidget(this.ID);
                        if (widget != null)
                        {
                            await widget.Disable();
                        }
                    }
                }
            }
        }
    }
}

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
    public class OverlayPersistentTimerV3Model : OverlayEventTrackingV3ModelBase
    {
        public const string SecondsProperty = "Seconds";

        public static readonly string DefaultHTML = OverlayResources.OverlayTimerDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayTextDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayPersistentTimerDefaultJavascript;

        [DataMember]
        public int InitialAmount { get; set; }

        [DataMember]
        public string DisplayFormat { get; set; }

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

        public OverlayPersistentTimerV3Model() : base(OverlayItemV3Type.PersistentTimer) { }

        public override async Task ProcessEvent(UserV2ViewModel user, double amount)
        {
            amount = Math.Round(amount);
            this.CurrentAmount += (int)amount;
            this.CurrentAmount = Math.Max(this.CurrentAmount, 0);

            if (amount != 0)
            {
                Dictionary<string, object> properties = new Dictionary<string, object>();
                properties[SecondsProperty] = amount;
                await this.CallFunction("adjustTime", properties);

                await ServiceManager.Get<CommandService>().Queue(this.TimerAdjustedCommandID, new CommandParametersModel(user));
            }
        }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();

            properties[nameof(this.CurrentAmount)] = this.CurrentAmount;
            properties[nameof(this.DisplayFormat)] = this.DisplayFormat;

            properties[nameof(this.TimerAdjustedAnimation)] = this.TimerAdjustedAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElement);
            properties[nameof(this.TimerCompletedAnimation)] = this.TimerCompletedAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElement);

            return properties;
        }

        protected override async Task WidgetEnableInternal()
        {
            this.CurrentAmount = this.InitialAmount;

            this.cancellationTokenSource = new CancellationTokenSource();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            AsyncRunner.RunAsyncBackground(this.BackgroundTimer, this.cancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            await base.WidgetEnableInternal();
        }

        protected override async Task WidgetDisableInternal()
        {
            if (this.cancellationTokenSource != null)
            {
                this.cancellationTokenSource.Cancel();
                this.cancellationTokenSource = null;
            }

            await base.WidgetDisableInternal();
        }

        protected override Task WidgetResetInternal()
        {
            this.CurrentAmount = this.InitialAmount;

            return Task.CompletedTask;
        }

        private async Task BackgroundTimer(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && this.CurrentAmount > 0)
            {
                await Task.Delay(1000);
                this.CurrentAmount--;
            }

            if (this.CurrentAmount == 0)
            {
                await ServiceManager.Get<CommandService>().Queue(this.TimerCompletedCommandID);
            }
        }
    }
}

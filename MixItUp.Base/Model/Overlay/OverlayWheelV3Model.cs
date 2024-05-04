using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayWheelOutcomeV3Model
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public double Percentage { get; set; }
        [DataMember]
        public double NotSelectedModifier { get; set; }
        [DataMember]
        public string Color { get; set; }

        [DataMember]
        public Guid CommandID { get; set; }

        [JsonIgnore]
        public double DecimalPercentage { get { return this.CurrentPercentage / 100.0; } }

        [JsonIgnore]
        public double CurrentPercentage { get; set; }
    }

    [DataContract]
    public class OverlayWheelV3Model : OverlayVisualTextV3ModelBase
    {
        public const string WheelLandedPacketType = "WheelLanded";

        public const string WinningPercentagePropertyName = "winningPercentage";

        public static readonly string DefaultHTML = OverlayResources.OverlayWheelDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayTextDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayWheelDefaultJavascript;

        [DataMember]
        public List<OverlayWheelOutcomeV3Model> Outcomes { get; set; } = new List<OverlayWheelOutcomeV3Model>();

        [DataMember]
        public OverlayAnimationV3Model EntranceAnimation { get; set; } = new OverlayAnimationV3Model();
        [DataMember]
        public OverlayAnimationV3Model OutcomeSelectedAnimation { get; set; } = new OverlayAnimationV3Model();
        [DataMember]
        public OverlayAnimationV3Model ExitAnimation { get; set; } = new OverlayAnimationV3Model();

        [JsonIgnore]
        public string OutcomePercentages { get { return string.Join(", ", this.Outcomes.Select(s => s.DecimalPercentage)); } }

        [JsonIgnore]
        public string OutcomeNames { get { return string.Join(", ", this.Outcomes.Select(s => s.Name)); } }

        [JsonIgnore]
        public string OutcomeColors { get { return string.Join(", ", this.Outcomes.Select(s => s.Color)); } }

        private CommandParametersModel spinningParameters = null;
        private double winningPercentage = 0.0;
        private OverlayWheelOutcomeV3Model winningOutcome = null;

        public OverlayWheelV3Model() : base(OverlayItemV3Type.Wheel) { }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();

            properties[nameof(this.OutcomePercentages)] = this.OutcomePercentages;
            properties[nameof(this.OutcomeNames)] = this.OutcomeNames;
            properties[nameof(this.OutcomeColors)] = this.OutcomeColors;

            properties["EntranceAnimationFramework"] = this.EntranceAnimation.AnimationFramework;
            properties["EntranceAnimationName"] = this.EntranceAnimation.AnimationName;
            properties["OutcomeSelectedAnimationFramework"] = this.OutcomeSelectedAnimation.AnimationFramework;
            properties["OutcomeSelectedAnimationName"] = this.OutcomeSelectedAnimation.AnimationName;
            properties["ExitAnimationFramework"] = this.ExitAnimation.AnimationFramework;
            properties["ExitAnimationName"] = this.ExitAnimation.AnimationName;

            return properties;
        }

        protected override async Task WidgetEnableInternal()
        {
            await base.WidgetEnableInternal();

            foreach (OverlayWheelOutcomeV3Model outcome in this.Outcomes)
            {
                outcome.CurrentPercentage = outcome.Percentage;
            }
        }

        public async Task Spin(CommandParametersModel parametersModel)
        {
            this.spinningParameters = parametersModel;
            this.winningPercentage = RandomHelper.GenerateDecimalProbability();
            this.winningOutcome = null;

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties[OverlayWheelV3Model.WinningPercentagePropertyName] = this.winningPercentage.ToString();
            await this.CallFunction("startSpin", properties);

            double tempPercentage = this.winningPercentage;
            foreach (OverlayWheelOutcomeV3Model outcome in this.Outcomes)
            {
                if (tempPercentage <= outcome.DecimalPercentage)
                {
                    winningOutcome = outcome;
                    break;
                }
                tempPercentage += outcome.DecimalPercentage;
            }
        }

        public override async Task ProcessPacket(OverlayV3Packet packet)
        {
            await base.ProcessPacket(packet);

            if (string.Equals(packet.Type, OverlayWheelV3Model.WheelLandedPacketType))
            {
                if (this.winningOutcome != null && this.winningOutcome.CommandID != Guid.Empty)
                {
                    CommandParametersModel parameters = new CommandParametersModel(this.spinningParameters.User);
                    Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
                    specialIdentifiers["outcomename"] = this.winningOutcome.Name;
                    await ServiceManager.Get<CommandService>().Queue(winningOutcome.CommandID, parameters);
                }
            }
        }
    }
}

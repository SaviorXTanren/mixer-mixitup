using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.IO;
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
        public double Probability { get; set; }
        [DataMember]
        public double Modifier { get; set; }
        [DataMember]
        public string Color { get; set; }

        [DataMember]
        public Guid CommandID { get; set; }

        [JsonIgnore]
        public double DecimalProbability { get { return this.CurrentProbability / 100.0; } }

        [JsonIgnore]
        public double CurrentProbability { get; set; }
    }

    [DataContract]
    public class OverlayWheelV3Model : OverlayVisualTextV3ModelBase
    {
        public const string WheelLandedPacketType = "WheelLanded";

        public const string WinningProbabilityPropertyName = "WinningProbability";

        public static readonly string DefaultHTML = OverlayResources.OverlayWheelDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayTextDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayWheelDefaultJavascript;

        public static readonly string DefaultWheelClickSoundFilePath = Path.Combine(ServiceManager.Get<IFileService>().GetApplicationDirectory(), "Assets\\Sounds\\WheelClick.mp3");

        [DataMember]
        public int Size { get; set; }

        [DataMember]
        public string WheelClickSoundFilePath { get; set; }

        [DataMember]
        public Guid DefaultOutcomeCommand { get; set; }

        [DataMember]
        public List<OverlayWheelOutcomeV3Model> Outcomes { get; set; } = new List<OverlayWheelOutcomeV3Model>();

        [DataMember]
        public OverlayAnimationV3Model EntranceAnimation { get; set; } = new OverlayAnimationV3Model();
        [DataMember]
        public OverlayAnimationV3Model OutcomeSelectedAnimation { get; set; } = new OverlayAnimationV3Model();
        [DataMember]
        public OverlayAnimationV3Model ExitAnimation { get; set; } = new OverlayAnimationV3Model();

        [JsonIgnore]
        public string OutcomeProbability { get { return string.Join(", ", this.Outcomes.Select(s => s.DecimalProbability)); } }

        [JsonIgnore]
        public string OutcomeNames { get { return string.Join(", ", this.Outcomes.Select(s => $"\"{s.Name}\"")); } }

        [JsonIgnore]
        public string OutcomeColors { get { return string.Join(", ", this.Outcomes.Select(s => $"\"{s.Color}\"")); } }

        [JsonIgnore]
        public string WheelClickSoundURL { get { return ServiceManager.Get<OverlayV3Service>().GetURLForFile(this.WheelClickSoundFilePath, "sound"); } }

        private CommandParametersModel spinningParameters = null;
        private double winningProbability = 0.0;
        private OverlayWheelOutcomeV3Model winningOutcome = null;

        public OverlayWheelV3Model() : base(OverlayItemV3Type.Wheel) { }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();

            properties[nameof(this.Size)] = this.Size.ToString();
            properties[nameof(this.OutcomeProbability)] = this.OutcomeProbability;
            properties[nameof(this.OutcomeNames)] = this.OutcomeNames;
            properties[nameof(this.OutcomeColors)] = this.OutcomeColors;
            properties[nameof(this.WheelClickSoundURL)] = this.WheelClickSoundURL;

            properties["EntranceAnimationFramework"] = this.EntranceAnimation.AnimationFramework;
            properties["EntranceAnimationName"] = this.EntranceAnimation.AnimationName;
            properties["OutcomeSelectedAnimationFramework"] = this.OutcomeSelectedAnimation.AnimationFramework;
            properties["OutcomeSelectedAnimationName"] = this.OutcomeSelectedAnimation.AnimationName;
            properties["ExitAnimationFramework"] = this.ExitAnimation.AnimationFramework;
            properties["ExitAnimationName"] = this.ExitAnimation.AnimationName;

            return properties;
        }

        public async Task Spin(CommandParametersModel parametersModel)
        {
            this.spinningParameters = parametersModel;
            this.winningProbability = RandomHelper.GenerateDecimalProbability();
            this.winningOutcome = null;

            double tempPercentage = 0;
            foreach (OverlayWheelOutcomeV3Model outcome in this.Outcomes)
            {
                tempPercentage += outcome.DecimalProbability;
                if (tempPercentage >= this.winningProbability)
                {
                    winningOutcome = outcome;
                    break;
                }
            }

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties[OverlayWheelV3Model.WinningProbabilityPropertyName] = this.winningProbability.ToString();
            await this.CallFunction("startSpin", properties);
        }

        public async Task ShowWheel()
        {
            await this.CallFunction("showWheel", new Dictionary<string, object>());
        }

        public override async Task ProcessPacket(OverlayV3Packet packet)
        {
            await base.ProcessPacket(packet);

            if (string.Equals(packet.Type, OverlayWheelV3Model.WheelLandedPacketType))
            {
                if (this.winningOutcome != null)
                {
                    Guid commandID = this.DefaultOutcomeCommand;
                    if (ChannelSession.Settings.Commands.TryGetValue(this.winningOutcome.CommandID, out CommandModelBase command) && command.Actions.Count > 0)
                    {
                        commandID = this.winningOutcome.CommandID;
                    }

                    CommandParametersModel parameters = new CommandParametersModel(this.spinningParameters.User);
                    parameters.SpecialIdentifiers["outcomename"] = this.winningOutcome.Name;
                    await ServiceManager.Get<CommandService>().Queue(commandID, parameters);
                }
            }
        }

        protected override async Task WidgetEnableInternal()
        {
            await base.WidgetEnableInternal();

            foreach (OverlayWheelOutcomeV3Model outcome in this.Outcomes)
            {
                outcome.CurrentProbability = outcome.Probability;
            }
        }
    }
}

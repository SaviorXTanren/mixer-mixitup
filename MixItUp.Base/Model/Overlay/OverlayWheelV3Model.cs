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
    public class OverlayWheelSliceV3Model
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

        public OverlayWheelSliceV3Model(string name, double percentage, double notSelectedModifier, string color)
        {
            this.Name = name;
            this.Percentage = percentage;
            this.NotSelectedModifier = notSelectedModifier;
            this.Color = color;

            this.CurrentPercentage = this.Percentage;
        }
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
        public List<OverlayWheelSliceV3Model> Slices { get; set; } = new List<OverlayWheelSliceV3Model>();

        [DataMember]
        public OverlayAnimationV3Model EntranceAnimation { get; set; } = new OverlayAnimationV3Model();
        [DataMember]
        public OverlayAnimationV3Model OutcomeSelectedAnimation { get; set; } = new OverlayAnimationV3Model();
        [DataMember]
        public OverlayAnimationV3Model ExitAnimation { get; set; } = new OverlayAnimationV3Model();

        [JsonIgnore]
        public string SlicePercentages { get { return string.Join(", ", this.Slices.Select(s => s.DecimalPercentage)); } }

        [JsonIgnore]
        public string SliceNames { get { return string.Join(", ", this.Slices.Select(s => s.Name)); } }

        [JsonIgnore]
        public string SliceColors { get { return string.Join(", ", this.Slices.Select(s => s.Color)); } }

        private CommandParametersModel spinningParameters = null;
        private double winningPercentage = 0.0;
        private OverlayWheelSliceV3Model winningSlice = null;

        public OverlayWheelV3Model() : base(OverlayItemV3Type.Wheel) { }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();

            properties[nameof(this.SlicePercentages)] = this.SlicePercentages;
            properties[nameof(this.SliceNames)] = this.SliceNames;
            properties[nameof(this.SliceColors)] = this.SliceColors;

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
            this.winningPercentage = RandomHelper.GenerateDecimalProbability();
            this.winningSlice = null;

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties[OverlayWheelV3Model.WinningPercentagePropertyName] = this.winningPercentage.ToString();
            await this.CallFunction("startSpin", properties);

            OverlayWheelSliceV3Model winningSlice = null;
            double tempPercentage = this.winningPercentage;
            foreach (OverlayWheelSliceV3Model slice in this.Slices)
            {
                if (tempPercentage <= slice.DecimalPercentage)
                {
                    winningSlice = slice;
                    break;
                }
                tempPercentage += slice.DecimalPercentage;
            }
        }

        public override async Task ProcessPacket(OverlayV3Packet packet)
        {
            await base.ProcessPacket(packet);

            if (string.Equals(packet.Type, OverlayWheelV3Model.WheelLandedPacketType))
            {
                if (this.winningSlice != null && this.winningSlice.CommandID != Guid.Empty)
                {
                    CommandParametersModel parameters = new CommandParametersModel(this.spinningParameters.User);
                    Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
                    specialIdentifiers["outcomename"] = this.winningSlice.Name;
                    await ServiceManager.Get<CommandService>().Queue(winningSlice.CommandID, parameters);
                }
            }
        }
    }
}

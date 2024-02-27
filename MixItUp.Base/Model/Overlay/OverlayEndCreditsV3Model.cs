using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayEndCreditsV3Model : OverlayEventTrackingV3ModelBase
    {
        public static readonly string DefaultHTML = OverlayResources.OverlayEndCreditsDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayTextDefaultCSS + "\n\n" + OverlayResources.OverlayEndCreditsDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayEndCreditsDefaultJavascript;

        [DataMember]
        public Dictionary<string, string> CustomSections { get; set; } = new Dictionary<string, string>();

        [DataMember]
        public Guid StartedCommandID { get; set; }
        [DataMember]
        public Guid EndedCommandID { get; set; }

        public OverlayEndCreditsV3Model() : base(OverlayItemV3Type.EndCredits) { }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();

            //properties[nameof(this.BorderColor)] = this.BorderColor;
            //properties[nameof(this.GoalColor)] = this.GoalColor;
            //properties[nameof(this.ProgressColor)] = this.ProgressColor;

            //properties[nameof(this.ProgressOccurredAnimation)] = this.ProgressOccurredAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElement);
            //properties[nameof(this.SegmentCompletedAnimation)] = this.SegmentCompletedAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElement);

            return properties;
        }

        protected override async Task WidgetEnableInternal()
        {
            await base.WidgetEnableInternal();
        }

        protected override async Task WidgetDisableInternal()
        {
            await base.WidgetDisableInternal();
        }

        protected override Task WidgetResetInternal()
        {
            this.Reset();

            return Task.CompletedTask;
        }

        private void Reset()
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayEndCreditsSectionV3Type
    {
        Chatters = 0,
        Followers,
        Subscribers,
        Moderators,

        NewSubscribers = 20,
        Resubscribers,
        GiftedSubscriptions,
        AllSubscriptions,

        Custom = 100,
    }

    [DataContract]
    public class OverlayEndCreditsSectionV3Model
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public OverlayEndCreditsSectionV3Type Type { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Header { get; set; }
        [DataMember]
        public string Item { get; set; }

        [DataMember]
        public string HTML { get; set; } = string.Empty;
        [DataMember]
        public string CSS { get; set; } = string.Empty;
    }

    [DataContract]
    public class OverlayEndCreditsV3Model : OverlayEventTrackingV3ModelBase
    {
        public static readonly string DefaultHTML = OverlayResources.OverlayEndCreditsDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayTextDefaultCSS + "\n\n" + OverlayResources.OverlayEndCreditsDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayEndCreditsDefaultJavascript;

        [DataMember]
        public List<OverlayEndCreditsSectionV3Model> Sections { get; set; } = new List<OverlayEndCreditsSectionV3Model>();

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

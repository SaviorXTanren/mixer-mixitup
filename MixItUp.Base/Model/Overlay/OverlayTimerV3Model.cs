using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayTimerV3Model : OverlayVisualTextV3ModelBase
    {
        public static readonly string DefaultDisplayFormat = $"{TotalMinutesDisplayFormat}:{SecondsDisplayFormat}";

        public const string DaysDisplayFormat = "dd";

        public const string TotalHoursDisplayFormat = "HH";
        public const string HoursDisplayFormat = "hh";

        public const string TotalMinutesDisplayFormat = "MM";
        public const string MinutesDisplayFormat = "mm";

        public const string TotalSecondsDisplayFormat = "SS";
        public const string SecondsDisplayFormat = "ss";

        public static readonly string DefaultHTML = OverlayResources.OverlayTimerDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayTextDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayActionDefaultJavascript + Environment.NewLine + Environment.NewLine + OverlayResources.OverlayTimerDefaultJavascript;

        [DataMember]
        public string DisplayFormat { get; set; }

        [DataMember]
        public bool CountUp { get; set; }

        public OverlayTimerV3Model() : base(OverlayItemV3Type.Timer) { }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();
            properties[nameof(this.CountUp)] = this.CountUp.ToString().ToLower();
            properties[nameof(this.DisplayFormat)] = this.DisplayFormat;
            return properties;
        }
    }
}

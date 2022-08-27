using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayTimerBasicItemV3Model : OverlayVisualTextItemV3ModelBase
    {
        public static readonly string DefaultDisplayFormat = $"{TotalMinutesDisplayFormat}:{SecondsDisplayFormat}";

        public const string DaysDisplayFormat = "dd";

        public const string TotalHoursDisplayFormat = "HH";
        public const string HoursDisplayFormat = "hh";

        public const string TotalMinutesDisplayFormat = "MM";
        public const string MinutesDisplayFormat = "mm";

        public const string TotalSecondsDisplayFormat = "SS";
        public const string SecondsDisplayFormat = "ss";

        public static readonly string DefaultHTML = Resources.OverlayTimerBasicDefaultHTML;
        public static readonly string DefaultCSS = Resources.OverlayTextDefaultCSS;
        public static readonly string DefaultJavascript = Resources.OverlayTimerBasicDefaultJavascript;

        [DataMember]
        public string DisplayFormat { get; set; }

        [DataMember]
        public Guid TimerFinishedCommandID { get; set; }
        [DataMember]
        public bool AllowRepeatFinishedCommandTriggers { get; set; }

        public OverlayTimerBasicItemV3Model() : base(OverlayItemV3Type.Timer) { }

        protected override async Task<OverlayOutputV3Model> GetProcessedItem(OverlayOutputV3Model item, OverlayEndpointService overlayEndpointService, CommandParametersModel parameters)
        {
            item.HTML = ReplaceProperty(item.HTML, "DisplayFormat", this.DisplayFormat);
            item.CSS = ReplaceProperty(item.CSS, "DisplayFormat", this.DisplayFormat);
            item.Javascript = ReplaceProperty(item.Javascript, "DisplayFormat", this.DisplayFormat);

            item = await base.GetProcessedItem(item, overlayEndpointService, parameters);

            return item;
        }
    }
}

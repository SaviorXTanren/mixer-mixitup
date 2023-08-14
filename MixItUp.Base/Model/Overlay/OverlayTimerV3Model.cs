using MixItUp.Base.Model.Commands;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

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

        public static readonly string DefaultHTML = Resources.OverlayTimerDefaultHTML;
        public static readonly string DefaultCSS = Resources.OverlayTextDefaultCSS;
        public static readonly string DefaultJavascript = Resources.OverlayTimerDefaultJavascript;

        [DataMember]
        public string DisplayFormat { get; set; }

        [DataMember]
        public bool CountUp { get; set; }

        [DataMember]
        public Guid TimerFinishedCommandID { get; set; }
        [DataMember]
        public bool AllowRepeatFinishedCommandTriggers { get; set; }

        public OverlayTimerV3Model() : base(OverlayItemV3Type.Timer) { }

        protected override async Task<OverlayOutputV3Model> GetProcessedItem(OverlayOutputV3Model item, CommandParametersModel parameters)
        {
            string countUp = this.CountUp.ToString().ToLower();
            item.HTML = ReplaceProperty(item.HTML, nameof(this.CountUp), countUp);
            item.CSS = ReplaceProperty(item.CSS, nameof(this.CountUp), countUp);
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.CountUp), countUp);

            item.HTML = ReplaceProperty(item.HTML, nameof(this.DisplayFormat), this.DisplayFormat);
            item.CSS = ReplaceProperty(item.CSS, nameof(this.DisplayFormat), this.DisplayFormat);
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.DisplayFormat), this.DisplayFormat);

            item = await base.GetProcessedItem(item, parameters);

            return item;
        }
    }
}

using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayGoalResetV3Type
    {
        None,
        Daily,
        Weekly,
        Monthly,
    }

    [DataContract]
    public class OverlayGoalSegmentV3Model
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Amount { get; set; }
    }

    [DataContract]
    public class OverlayGoalV3Model : OverlayEventTrackingV3ModelBase
    {
        public const string GoalNameProperty = "GoalName";
        public const string GoalEndProperty = "GoalEnd";
        public const string GoalAmountProperty = "GoalAmount";
        public const string GoalMaxAmountProperty = "GoalMaxAmount";
        public const string GoalBarCompletionPercentageProperty = "GoalBarCompletionPercentage";

        public static readonly string DefaultHTML = OverlayResources.OverlayGoalDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayTextDefaultCSS + "\n\n" + OverlayResources.OverlayGoalDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayGoalDefaultJavascript;

        [DataMember]
        public int TotalAmount { get; set; }
        [DataMember]
        public List<OverlayGoalSegmentV3Model> Segments { get; set; } = new List<OverlayGoalSegmentV3Model>();

        [DataMember]
        public DateTimeOffset EndDate { get; set; }

        [DataMember]
        public OverlayGoalResetV3Type ResetType { get; set; }
        [DataMember]
        public DateTimeOffset ResetCadence { get; set; }

        [DataMember]
        public string BorderColor { get; set; }
        [DataMember]
        public string GoalColor { get; set; }
        [DataMember]
        public string ProgressColor { get; set; }

        [DataMember]
        public OverlayAnimationV3Model ProgressOccurredAnimation { get; set; } = new OverlayAnimationV3Model();
        [DataMember]
        public OverlayAnimationV3Model SegmentCompletedAnimation { get; set; } = new OverlayAnimationV3Model();

        [DataMember]
        public Guid ProgressOccurredCommandID { get; set; }
        [DataMember]
        public Guid SegmentCompletedCommandID { get; set; }

        [JsonIgnore]
        public int CurrentAmount { get; private set; }
        [JsonIgnore]
        public OverlayGoalSegmentV3Model CurrentSegment { get; private set; }

        [JsonIgnore]
        public int GoalBarCompletionPercentage { get { return Math.Max(Math.Min((int)Math.Round(((double)this.CurrentAmount / this.CurrentSegment.Amount) * 100.0), 100), 0); } }

        [JsonIgnore]
        public string GoalEndText
        {
            get
            {
                if (this.ResetType == OverlayGoalResetV3Type.None)
                {
                    return string.Empty;
                }
                return this.EndDate.GetAge();
            }
        }

        [JsonIgnore]
        public override bool IsTestable { get { return true; } }
        [JsonIgnore]
        public override bool IsResettable { get { return true; } }

        public OverlayGoalV3Model() : base(OverlayItemV3Type.Goal) { }

        public void Reset()
        {
            this.TotalAmount = 0;
            this.CurrentAmount = 0;
            this.CurrentSegment = this.Segments.First();
        }

        public override async Task ProcessEvent(UserV2ViewModel user, double amount)
        {
            if (amount > 0)
            {
                amount = Math.Round(amount);
                this.TotalAmount += (int)amount;
                this.CurrentAmount += (int)amount;

                if (this.CurrentAmount < this.CurrentSegment.Amount || this.CurrentSegment == this.Segments.Last())
                {
                    await this.Update();

                    await ServiceManager.Get<CommandService>().Queue(this.ProgressOccurredCommandID, new CommandParametersModel(user));
                }
                else
                {
                    this.ProgressSegments();

                    await this.Complete();

                    await ServiceManager.Get<CommandService>().Queue(this.SegmentCompletedCommandID, new CommandParametersModel(user));
                }
            }
        }

        public async Task Update()
        {
            Dictionary<string, object> properties = this.GetDataProperties();
            await this.CallFunction("update", properties);
        }

        public async Task Complete()
        {
            Dictionary<string, object> properties = this.GetDataProperties();
            await this.CallFunction("complete", properties);
        }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();

            foreach (var kvp in this.GetDataProperties())
            {
                properties[kvp.Key] = kvp.Value;
            }

            properties[nameof(this.BorderColor)] = this.BorderColor;
            properties[nameof(this.GoalColor)] = this.GoalColor;
            properties[nameof(this.ProgressColor)] = this.ProgressColor;

            properties[nameof(this.ProgressOccurredAnimation)] = this.ProgressOccurredAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElementID);
            properties[nameof(this.SegmentCompletedAnimation)] = this.SegmentCompletedAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElementID);

            return properties;
        }

        protected override async Task WidgetEnableInternal()
        {
            this.CurrentSegment = this.Segments.First();
            this.CurrentAmount = this.TotalAmount;
            this.ProgressSegments();

            DateTimeOffset newResetDate = DateTimeOffset.MinValue;
            if (this.ResetType == OverlayGoalResetV3Type.Daily)
            {
                newResetDate = this.EndDate.AddDays(1);
            }
            else if (this.ResetType == OverlayGoalResetV3Type.Weekly)
            {
                newResetDate = new DateTime(this.EndDate.Year, this.EndDate.Month, this.EndDate.Day);
                do
                {
                    newResetDate = newResetDate.AddDays(1);
                } while (newResetDate.DayOfWeek != this.ResetCadence.DayOfWeek);
            }
            else if (this.ResetType == OverlayGoalResetV3Type.Monthly)
            {
                int day = Math.Min(this.ResetCadence.Day, DateTime.DaysInMonth(this.EndDate.Year, this.EndDate.Month));
                newResetDate = new DateTime(this.EndDate.Year, this.EndDate.Month, day);
                newResetDate = newResetDate.AddMonths(1);
            }

            if (newResetDate != DateTimeOffset.MinValue && newResetDate <= DateTimeOffset.Now.Date)
            {
                this.Reset();
            }

            await base.WidgetEnableInternal();
        }

        protected override Task WidgetResetInternal()
        {
            this.TotalAmount = 0;
            this.CurrentAmount = 0;
            this.CurrentSegment = this.Segments.First();

            return Task.CompletedTask;
        }

        private Dictionary<string, object> GetDataProperties()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data[GoalNameProperty] = this.CurrentSegment.Name;
            data[GoalEndProperty] = this.GoalEndText;
            data[GoalAmountProperty] = this.CurrentAmount;
            data[GoalMaxAmountProperty] = this.CurrentSegment.Amount;
            data[GoalBarCompletionPercentageProperty] = this.GoalBarCompletionPercentage;
            return data;
        }

        private void ProgressSegments()
        {
            while (this.CurrentSegment != this.Segments.Last() && this.CurrentAmount >= this.CurrentSegment.Amount)
            {
                this.CurrentAmount -= this.CurrentSegment.Amount;
                this.CurrentSegment = this.Segments[this.Segments.IndexOf(this.CurrentSegment) + 1];
            }
        }
    }
}

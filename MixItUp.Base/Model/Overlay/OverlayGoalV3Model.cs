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
    public enum OverlayGoalSegmentV3Type
    {
        Individual,
        Cumulative
    }

    [DataContract]
    public class OverlayGoalSegmentV3Model
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public double Amount { get; set; }

        [DataMember]
        public Guid CommandID { get; set; }
    }

    [DataContract]
    public class OverlayGoalV3Model : OverlayEventCountingV3ModelBase
    {
        public const string GoalNameProperty = "GoalName";
        public const string GoalEndProperty = "GoalEnd";
        public const string GoalAmountProperty = "GoalAmount";
        public const string GoalMaxAmountProperty = "GoalMaxAmount";
        public const string GoalBarCompletionPercentageProperty = "GoalBarCompletionPercentage";

        public const string TotalAmountSpecialIdentifier = "goaltotalamount";
        public const string ProgressAmountSpecialIdentifier = "goalprogressamount";
        public const string SegmentNameSpecialIdentifier = "goalsegmentname";
        public const string SegmentAmountSpecialIdentifier = "goalsegmentamount";
        public const string NextSegmentNameSpecialIdentifier = "goalnextsegmentname";
        public const string NextSegmentAmountSpecialIdentifier = "goalnextsegmentamount";

        public static readonly string DefaultHTML = OverlayResources.OverlayGoalDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayGoalDefaultCSS + Environment.NewLine + Environment.NewLine + OverlayResources.OverlayTextDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayGoalDefaultJavascript;

        [DataMember]
        public OverlayGoalSegmentV3Type SegmentType { get; set; }
        [DataMember]
        public List<OverlayGoalSegmentV3Model> Segments { get; set; } = new List<OverlayGoalSegmentV3Model>();

        [DataMember]
        public ResetTracker ResetTracker { get; set; }

        [DataMember]
        public int StartingAmountCustom { get; set; }

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

        [DataMember]
        public double TotalAmount { get; set; }

        [JsonIgnore]
        public DateTimeOffset ResetDateTime { get; set; }

        [JsonIgnore]
        public OverlayGoalSegmentV3Model CurrentSegment { get; private set; }

        [JsonIgnore]
        public double GoalAmount { get { return this.CurrentSegment.Amount; } }
        [JsonIgnore]
        public double PreviousGoalAmount { get; set; }

        [JsonIgnore]
        public double CurrentAmount
        {
            get
            {
                if (this.SegmentType == OverlayGoalSegmentV3Type.Individual)
                {
                    return this.TotalAmount - this.PreviousGoalAmount;
                }
                else if (this.SegmentType == OverlayGoalSegmentV3Type.Cumulative)
                {
                    return this.TotalAmount;
                }
                return 0;
            }
        }

        [JsonIgnore]
        public int GoalBarCompletionPercentage
        {
            get
            {
                double percentage = (this.CurrentAmount / this.GoalAmount) * 100;
                percentage = Math.Min(percentage, 100);
                percentage = Math.Max(percentage, 0);
                return (int)Math.Round(percentage);
            }
        }

        [JsonIgnore]
        public string GoalEndText
        {
            get
            {
                if (this.ResetDateTime == DateTimeOffset.MinValue)
                {
                    return string.Empty;
                }
                return this.ResetDateTime.GetAge();
            }
        }

        [JsonIgnore]
        public override bool IsTestable { get { return true; } }
        [JsonIgnore]
        public override bool IsResettable { get { return true; } }

        public OverlayGoalV3Model() : base(OverlayItemV3Type.Goal) { }

        public override async Task ProcessEvent(UserV2ViewModel user, double amount)
        {
            if (this.CurrentSegment != null && amount != 0)
            {
                OverlayGoalSegmentV3Model previousSegment = this.CurrentSegment;
                double previousAmount = this.TotalAmount;
                if (amount > 0)
                {
                    this.TotalAmount += amount;
                }
                else if (amount < 0)
                {
                    this.TotalAmount = Math.Max(this.TotalAmount + amount, this.PreviousGoalAmount);
                }

                CommandParametersModel parameters = new CommandParametersModel(user);
                parameters.SpecialIdentifiers[TotalAmountSpecialIdentifier] = this.CurrentAmount.ToString();
                parameters.SpecialIdentifiers[ProgressAmountSpecialIdentifier] = amount.ToString();
                parameters.SpecialIdentifiers[SegmentNameSpecialIdentifier] = previousSegment.Name;
                parameters.SpecialIdentifiers[SegmentAmountSpecialIdentifier] = previousSegment.Amount.ToString();

                if (this.CurrentAmount >= this.GoalAmount && (this.CurrentSegment != this.Segments.Last() || previousAmount < this.GoalAmount))
                {
                    this.ProgressSegments();

                    await this.Complete();

                    parameters.SpecialIdentifiers[NextSegmentNameSpecialIdentifier] = this.CurrentSegment.Name;
                    parameters.SpecialIdentifiers[NextSegmentAmountSpecialIdentifier] = this.CurrentSegment.Amount.ToString();

                    await ServiceManager.Get<CommandService>().Queue(this.SegmentCompletedCommandID, parameters);
                    await ServiceManager.Get<CommandService>().Queue(previousSegment.CommandID, parameters);
                }
                else
                {
                    await this.Update();

                    await ServiceManager.Get<CommandService>().Queue(this.ProgressOccurredCommandID, parameters);
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

            OverlayItemV3ModelBase.AddAnimationProperties(properties, nameof(this.ProgressOccurredAnimation), this.ProgressOccurredAnimation);
            OverlayItemV3ModelBase.AddAnimationProperties(properties, nameof(this.SegmentCompletedAnimation), this.SegmentCompletedAnimation);

            return properties;
        }

        protected override async Task WidgetInitializeInternal()
        {
            await base.WidgetInitializeInternal();

            this.CurrentSegment = this.Segments.First();
            this.ProgressSegments();

            if (this.ResetTracker?.Amount > 0)
            {
                this.ResetDateTime = this.ResetTracker.GetEndDateTimeOffset();
                if (this.ResetDateTime <= DateTimeOffset.Now)
                {
                    this.Reset();
                }
            }
        }

        protected override Task WidgetResetInternal()
        {
            this.Reset();

            return Task.CompletedTask;
        }

        private Dictionary<string, object> GetDataProperties()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data[GoalNameProperty] = this.CurrentSegment.Name;
            data[GoalEndProperty] = this.GoalEndText;
            data[GoalAmountProperty] = this.CurrentAmount;
            data[GoalMaxAmountProperty] = this.GoalAmount;
            data[GoalBarCompletionPercentageProperty] = this.GoalBarCompletionPercentage;
            return data;
        }

        private void ProgressSegments()
        {
            while (this.CurrentSegment != this.Segments.Last() && this.CurrentAmount >= this.GoalAmount)
            {
                if (this.SegmentType == OverlayGoalSegmentV3Type.Individual)
                {
                    this.PreviousGoalAmount += this.CurrentSegment.Amount;
                }
                else if (this.SegmentType == OverlayGoalSegmentV3Type.Cumulative)
                {
                    this.PreviousGoalAmount = this.CurrentSegment.Amount;
                }
                this.CurrentSegment = this.Segments[this.Segments.IndexOf(this.CurrentSegment) + 1];
            }
        }

        private void Reset()
        {
            this.TotalAmount = 0;
            this.PreviousGoalAmount = 0;
            this.CurrentSegment = this.Segments.First();

            if (this.StartingAmountCustom > 0)
            {
                this.TotalAmount += this.StartingAmountCustom;
            }

            if (this.ResetTracker?.Amount > 0)
            {
                this.ResetTracker.UpdateStartDateTimeToLatest();
                this.ResetDateTime = this.ResetTracker.GetEndDateTimeOffset();
            }
        }
    }
}

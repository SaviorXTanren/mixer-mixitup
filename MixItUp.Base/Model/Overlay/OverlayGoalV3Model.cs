using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.Twitch.New;
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
    public enum OverlayGoalV3Type
    {
        Custom,
        Counter,
        Followers,
        Subscribers,
    }

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
        public const string GoalNextNameProperty = "GoalNextName";
        public const string GoalNextAmountProperty = "GoalNextAmount";

        public const string TotalAmountSpecialIdentifier = "goaltotalamount";
        public const string ProgressAmountSpecialIdentifier = "goalprogressamount";
        public const string RemainingAmountSpecialIdentifier = "goalprogressremaining";
        public const string SegmentNameSpecialIdentifier = "goalsegmentname";
        public const string SegmentAmountSpecialIdentifier = "goalsegmentamount";
        public const string NextSegmentNameSpecialIdentifier = "goalnextsegmentname";
        public const string NextSegmentAmountSpecialIdentifier = "goalnextsegmentamount";

        public static readonly string DefaultHTML = OverlayResources.OverlayGoalDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayGoalDefaultCSS + Environment.NewLine + Environment.NewLine + OverlayResources.OverlayTextDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayGoalDefaultJavascript;

        [DataMember]
        public OverlayGoalV3Type GoalType { get; set; } = OverlayGoalV3Type.Custom;

        [DataMember]
        public StreamingPlatformTypeEnum StreamingPlatform { get; set; } = StreamingPlatformTypeEnum.None;

        [DataMember]
        public OverlayGoalSegmentV3Type SegmentType { get; set; }
        [DataMember]
        public List<OverlayGoalSegmentV3Model> Segments { get; set; } = new List<OverlayGoalSegmentV3Model>();

        [DataMember]
        public ResetTracker ResetTracker { get; set; }

        [DataMember]
        public string CounterName { get; set; }

        [DataMember]
        public double StartingAmountCustom { get; set; }

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
        public double CustomTotalAmount { get; set; }

        [DataMember]
        public double TotalAmount
        {
            get
            {
                if (this.GoalType == OverlayGoalV3Type.Counter && this.Counter != null)
                {
                    return this.Counter.Amount;
                }
                return this.CustomTotalAmount;
            }
            set
            {
                if (this.GoalType == OverlayGoalV3Type.Counter && this.Counter != null)
                {
                    this.Counter.Amount = value;
                }
                this.CustomTotalAmount = value;
            }
        }

        [JsonIgnore]
        public OverlayGoalSegmentV3Model CurrentSegment { get; private set; }

        [JsonIgnore]
        public double GoalAmount { get { return this.CurrentSegment.Amount; } }
        [JsonIgnore]
        public double PreviousGoalAmount { get; set; }

        [JsonIgnore]
        public CounterModel Counter { get; set; }
        [JsonIgnore]
        private double CounterPreviousCurrentAmount { get; set; }

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
                if (this.ResetTracker?.Amount > 0)
                {
                    return this.ResetTracker.GetEndDateTimeOffset().GetAge();
                }
                return string.Empty;
            }
        }

        [JsonIgnore]
        public override bool IsResettable { get { return true; } }

        public OverlayGoalV3Model() : base(OverlayItemV3Type.Goal) { }

        public override async Task Initialize()
        {
            await base.Initialize();

            if (this.ResetTracker == null)
            {
                this.ResetTracker = new ResetTracker();
            }

            CounterModel.OnCounterUpdated -= CounterModel_OnCounterUpdated;
            if (this.GoalType == OverlayGoalV3Type.Counter && ChannelSession.Settings.Counters.TryGetValue(this.CounterName, out CounterModel counter))
            {
                this.Counter = counter;
                this.CounterPreviousCurrentAmount = this.CurrentAmount;
                CounterModel.OnCounterUpdated += CounterModel_OnCounterUpdated;
            }
            else if (this.GoalType == OverlayGoalV3Type.Followers)
            {
                if (!this.IsLivePreview)
                {
                    this.EnableFollows();
                    if (this.StreamingPlatform == StreamingPlatformTypeEnum.Twitch)
                    {
                        this.TotalAmount = await ServiceManager.Get<TwitchSession>().StreamerService.GetFollowerCount(ServiceManager.Get<TwitchSession>().StreamerModel);
                    }
                }
            }
            else if (this.GoalType == OverlayGoalV3Type.Subscribers)
            {
                if (!this.IsLivePreview)
                {
                    this.EnableSubscriptions();
                    if (this.StreamingPlatform == StreamingPlatformTypeEnum.Twitch)
                    {
                        this.TotalAmount = await ServiceManager.Get<TwitchSession>().StreamerService.GetSubscriberCount(ServiceManager.Get<TwitchSession>().StreamerModel);
                    }
                }
            }

            this.PreviousGoalAmount = 0;
            this.CurrentSegment = this.Segments.First();
            this.ProgressSegments();

            if (this.ResetTracker?.Amount > 0 && this.ResetTracker.MustBeReset())
            {
                this.ResetTracker.PerformReset();
                await this.Reset();
            }
        }

        public override async Task Uninitialize()
        {
            await base.Uninitialize();

            CounterModel.OnCounterUpdated -= CounterModel_OnCounterUpdated;
        }

        public override async Task Reset()
        {
            await base.Reset();

            this.TotalAmount = 0;
            this.CounterPreviousCurrentAmount = 0;
            this.PreviousGoalAmount = 0;
            this.CurrentSegment = this.Segments.First();

            if (this.StartingAmountCustom > 0)
            {
                this.TotalAmount += this.StartingAmountCustom;
            }

            if (this.ResetTracker?.Amount > 0)
            {
                this.ResetTracker.PerformReset();
            }
        }

        public override void ImportReset()
        {
            base.ImportReset();

            this.ProgressOccurredCommandID = Guid.Empty;
            this.SegmentCompletedCommandID = Guid.Empty;

            foreach (var segment in this.Segments)
            {
                segment.CommandID = Guid.Empty;
            }
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

            this.ProgressOccurredAnimation.AddAnimationProperties(properties, nameof(this.ProgressOccurredAnimation));
            this.SegmentCompletedAnimation.AddAnimationProperties(properties, nameof(this.SegmentCompletedAnimation));

            return properties;
        }

        public override async Task ProcessEvent(UserV2ViewModel user, double amount)
        {
            if (this.CurrentSegment != null)
            {
                Logger.Log(LogLevel.Debug, $"Processing goal amount - {amount}");

                if (this.GoalType == OverlayGoalV3Type.Followers && user.Platform == this.StreamingPlatform)
                {
                    amount = 1;
                }
                else if (this.GoalType == OverlayGoalV3Type.Subscribers && user.Platform == this.StreamingPlatform)
                {
                    amount = 1;
                }

                OverlayGoalSegmentV3Model previousSegment = this.CurrentSegment;
                double previousAmount = this.TotalAmount;
                if (amount != 0)
                {
                    if (amount > 0)
                    {
                        this.TotalAmount += amount;
                    }
                    else if (amount < 0)
                    {
                        this.TotalAmount = Math.Max(this.TotalAmount + amount, this.PreviousGoalAmount);
                    }
                }
                else if (this.GoalType == OverlayGoalV3Type.Counter && this.Counter != null)
                {
                    previousAmount = this.CounterPreviousCurrentAmount;
                }

                Logger.Log(LogLevel.Debug, $"New amounts - {this.CurrentAmount} / {this.GoalAmount} ({this.TotalAmount})");

                if (this.CurrentAmount >= this.GoalAmount && (this.CurrentSegment != this.Segments.Last() || previousAmount < this.GoalAmount))
                {
                    IEnumerable<OverlayGoalSegmentV3Model> segmentsCompleted = this.ProgressSegments();

                    await this.Complete();

                    foreach (OverlayGoalSegmentV3Model segment in segmentsCompleted)
                    {
                        CommandParametersModel parameters = new CommandParametersModel(user);
                        parameters.SpecialIdentifiers[TotalAmountSpecialIdentifier] = this.CurrentAmount.ToString();
                        parameters.SpecialIdentifiers[ProgressAmountSpecialIdentifier] = amount.ToString();
                        parameters.SpecialIdentifiers[RemainingAmountSpecialIdentifier] = (this.CurrentAmount - amount).ToString();
                        parameters.SpecialIdentifiers[SegmentNameSpecialIdentifier] = segment.Name;
                        parameters.SpecialIdentifiers[SegmentAmountSpecialIdentifier] = segment.Amount.ToString();
                        parameters.SpecialIdentifiers[NextSegmentNameSpecialIdentifier] = this.CurrentSegment.Name;
                        parameters.SpecialIdentifiers[NextSegmentAmountSpecialIdentifier] = this.CurrentSegment.Amount.ToString();

                        if (ChannelSession.Settings.Commands.TryGetValue(segment.CommandID, out CommandModelBase command) && command.Actions.Count > 0)
                        {
                            await ServiceManager.Get<CommandService>().Queue(segment.CommandID, parameters);
                        }
                        else
                        {
                            await ServiceManager.Get<CommandService>().Queue(this.SegmentCompletedCommandID, parameters);
                        }
                    }
                }
                else
                {
                    await this.Update();

                    CommandParametersModel parameters = new CommandParametersModel(user);
                    parameters.SpecialIdentifiers[TotalAmountSpecialIdentifier] = this.CurrentAmount.ToString();
                    parameters.SpecialIdentifiers[ProgressAmountSpecialIdentifier] = amount.ToString();
                    parameters.SpecialIdentifiers[SegmentNameSpecialIdentifier] = previousSegment.Name;
                    parameters.SpecialIdentifiers[SegmentAmountSpecialIdentifier] = previousSegment.Amount.ToString();
                    await ServiceManager.Get<CommandService>().Queue(this.ProgressOccurredCommandID, parameters);
                }

                if (this.GoalType == OverlayGoalV3Type.Counter && this.Counter != null)
                {
                    this.CounterPreviousCurrentAmount = this.CurrentAmount;
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

        private Dictionary<string, object> GetDataProperties()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data[GoalNameProperty] = this.CurrentSegment.Name;
            data[GoalEndProperty] = this.GoalEndText;
            data[GoalAmountProperty] = this.CurrentAmount;
            data[GoalMaxAmountProperty] = this.GoalAmount;
            data[GoalBarCompletionPercentageProperty] = this.GoalBarCompletionPercentage;

            data[GoalNextNameProperty] = null;
            data[GoalNextAmountProperty] = 0;
            if (this.CurrentSegment != null)
            {
                int currentIndex = this.Segments.IndexOf(this.CurrentSegment);
                if (currentIndex + 1 < this.Segments.Count)
                {
                    OverlayGoalSegmentV3Model nextSegment = this.Segments[currentIndex + 1];
                    data[GoalNextNameProperty] = nextSegment.Name;
                    data[GoalNextAmountProperty] = nextSegment.Amount;
                }
            }

            return data;
        }

        private IEnumerable<OverlayGoalSegmentV3Model> ProgressSegments()
        {
            List<OverlayGoalSegmentV3Model> segmentsCompleted = new List<OverlayGoalSegmentV3Model>();
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
                segmentsCompleted.Add(this.CurrentSegment);
                this.CurrentSegment = this.Segments[this.Segments.IndexOf(this.CurrentSegment) + 1];
            }

            if (this.CurrentSegment == this.Segments.Last() && this.CurrentAmount >= this.GoalAmount)
            {
                segmentsCompleted.Add(this.CurrentSegment);
            }

            return segmentsCompleted;
        }

        private async void CounterModel_OnCounterUpdated(object sender, CounterModel counter)
        {
            if (counter == this.Counter)
            {
                await this.ProcessEvent(ChannelSession.User, 0);
            }
        }
    }
}

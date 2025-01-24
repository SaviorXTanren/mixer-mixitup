using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Model.Twitch.EventSub;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public class OverlayPollOptionV3Model
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Color { get; set; }
        public int Amount { get; set; }
        public double Percentage { get; set; }

        public int ChannelPoints { get; set; }

        public OverlayPollOptionV3Model() { }

        public OverlayPollOptionV3Model(PollNotificationChoice choice)
        {
            this.ID = choice.ID;
            this.Name = choice.Title;
        }

        public OverlayPollOptionV3Model(PredictionNotificationOutcome outcome)
        {
            this.ID = outcome.ID;
            this.Name = outcome.Title;
            this.Color = outcome.Color;
            this.ChannelPoints = outcome.ChannelPoints;
        }

        public OverlayPollOptionV3Model(int id, GameOutcomeModel outcome)
        {
            this.ID = id.ToString();
            this.Name = outcome.Name;
        }

        public OverlayPollOptionV3Model(int id, string option)
        {
            this.ID = id.ToString();
            this.Name = option;
        }

        public Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties[nameof(this.ID)] = this.ID;
            properties[nameof(this.Name)] = this.Name;
            properties[nameof(this.Color)] = this.Color;
            properties[nameof(this.Amount)] = this.Amount;
            properties[nameof(this.Percentage)] = this.Percentage;
            properties[nameof(this.ChannelPoints)] = this.ChannelPoints;
            return properties;
        }
    }

    [DataContract]
    public class OverlayPollHeaderV3Model : OverlayHeaderV3ModelBase
    {
        public OverlayPollHeaderV3Model() { }
    }

    public class OverlayPollV3Model : OverlayVisualTextV3ModelBase
    {
        public static IEnumerable<OverlayPollV3Model> GetPollOverlayWidgets(bool forPolls = false, bool forPredictions = false, bool forBet = false, bool forTrivia = false)
        {
            List<OverlayPollV3Model> polls = new List<OverlayPollV3Model>();
            if (ServiceManager.Get<OverlayV3Service>().IsConnected)
            {
                foreach (OverlayWidgetV3Model widget in ServiceManager.Get<OverlayV3Service>().GetWidgets())
                {
                    if (widget.Type == OverlayItemV3Type.Poll)
                    {
                        OverlayPollV3Model poll = (OverlayPollV3Model)widget.Item;
                        if ((forPolls && poll.UseWithTwitchPolls) ||
                            (forPredictions && poll.UseWithTwitchPredictions) ||
                            (forBet && poll.UseWithBetGameCommand) ||
                            (forTrivia && poll.UseWithTriviaGameCommand))
                        {
                            polls.Add(poll);
                        }
                    }
                }
            }
            return polls;
        }

        public const string QuestionPropertyName = "Question";
        public const string TimeLimitPropertyName = "TimeLimit";
        public const string OptionsPropertyName = "Options";
        public const string TotalVotesPropertyName = "TotalVotes";
        public const string WinnerIDPropertyName = "WinnerID";

        public static readonly string DefaultHTML = OverlayResources.OverlayPollDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayPollDefaultCSS + Environment.NewLine + Environment.NewLine + OverlayResources.OverlayTextDefaultCSS + Environment.NewLine + Environment.NewLine + OverlayResources.OverlayHeaderTextDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayPollDefaultJavascript;

        [DataMember]
        public OverlayPollHeaderV3Model Header { get; set; }

        [DataMember]
        public int BarHeight { get; set; }

        [DataMember]
        public bool UseRandomColors { get; set; }
        [DataMember]
        public string BarColor { get; set; }
        [DataMember]
        public bool UseTwitchPredictionColors { get; set; }
        [DataMember]
        public bool ShowTwitchPredictionChannelPoints { get; set; }

        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string BorderColor { get; set; }

        [DataMember]
        public bool UseWithTwitchPolls { get; set; }
        [DataMember]
        public bool UseWithTwitchPredictions { get; set; }
        [DataMember]
        public bool UseWithTriviaGameCommand { get; set; }
        [DataMember]
        public bool UseWithBetGameCommand { get; set; }

        [DataMember]
        public OverlayAnimationV3Model EntranceAnimation { get; set; } = new OverlayAnimationV3Model();
        [DataMember]
        public OverlayAnimationV3Model ExitAnimation { get; set; } = new OverlayAnimationV3Model();

        private Dictionary<string, OverlayPollOptionV3Model> currentOptions = new Dictionary<string, OverlayPollOptionV3Model>();

        public OverlayPollV3Model() : base(OverlayItemV3Type.Poll) { }

        public async Task NewTwitchPoll(PollNotification poll)
        {
            int totalTime = (int)Math.Round((poll.EndsAt - poll.StartedAt).TotalSeconds);
            await this.NewPoll(poll.Title, totalTime, poll.Choices.Select(c => new OverlayPollOptionV3Model(c)));
        }

        public async Task UpdateTwitchPoll(PollNotification poll)
        {
            foreach (var choice in poll.Choices)
            {
                if (this.currentOptions.TryGetValue(choice.ID, out OverlayPollOptionV3Model model))
                {
                    model.Amount = choice.Votes;
                }
            }
            await this.Update();
        }

        public async Task NewTwitchPrediction(PredictionNotification prediction)
        {
            int totalTime = (int)Math.Round((prediction.LocksAt - prediction.StartedAt).TotalSeconds);
            await this.NewPoll(prediction.Title, totalTime, prediction.Outcomes.Select(o => new OverlayPollOptionV3Model(o)), hasChannelPoints: true);
        }

        public async Task UpdateTwitchPrediction(PredictionNotification prediction)
        {
            foreach (var outcome in prediction.Outcomes)
            {
                if (this.currentOptions.TryGetValue(outcome.ID, out OverlayPollOptionV3Model model))
                {
                    model.Amount = outcome.Users;
                    model.ChannelPoints = outcome.ChannelPoints;
                }
            }
            await this.Update();
        }

        public async Task NewBetCommand(string question, int timeLimitSeconds, List<GameOutcomeModel> outcomes)
        {
            await this.NewPoll(question, timeLimitSeconds, outcomes.Select(o => new OverlayPollOptionV3Model(outcomes.IndexOf(o) + 1, o)));
        }

        public async Task UpdateBetCommand(int id)
        {
            if (this.currentOptions.TryGetValue(id.ToString(), out OverlayPollOptionV3Model model))
            {
                model.Amount++;
            }
            await this.Update();
        }

        public async Task NewTriviaCommand(string question, int timeLimitSeconds, Dictionary<int, string> answers)
        {
            await this.NewPoll(question, timeLimitSeconds, answers.Select(a => new OverlayPollOptionV3Model(a.Key, a.Value)));
        }

        public async Task UpdateTriviaCommand(int id)
        {
            if (this.currentOptions.TryGetValue(id.ToString(), out OverlayPollOptionV3Model model))
            {
                model.Amount++;
            }
            await this.Update();
        }

        public async Task End(string id)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties[OverlayPollV3Model.WinnerIDPropertyName] = id;
            await this.CallFunction("end", properties);
        }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();

            foreach (var kvp in this.Header.GetGenerationProperties())
            {
                properties[kvp.Key] = kvp.Value;
            }

            properties[nameof(this.BackgroundColor)] = this.BackgroundColor;
            properties[nameof(this.BorderColor)] = this.BorderColor;
            properties[nameof(this.BarColor)] = this.BarColor;
            properties[nameof(this.BarHeight)] = this.BarHeight;

            this.EntranceAnimation.AddAnimationProperties(properties, nameof(this.EntranceAnimation));
            this.ExitAnimation.AddAnimationProperties(properties, nameof(this.ExitAnimation));

            return properties;
        }

        protected override async Task Loaded()
        {
            if (this.IsLivePreview)
            {
                await this.NewPoll("This is a test poll", 60, new List<OverlayPollOptionV3Model>()
                {
                    new OverlayPollOptionV3Model() { ID = "1", Name = "Option 1", Amount = 100 },
                    new OverlayPollOptionV3Model() { ID = "2", Name = "Option 2", Amount = 200 },
                    new OverlayPollOptionV3Model() { ID = "3", Name = "Option 3", Amount = 300 },
                },
                hasChannelPoints: true);

                await Task.Delay(1000);

                await this.Update();
            }
        }

        protected internal async Task TestPoll()
        {
            List<OverlayPollOptionV3Model> options = new List<OverlayPollOptionV3Model>();
            options.Add(new OverlayPollOptionV3Model(1, "Option 1"));
            options.Add(new OverlayPollOptionV3Model(2, "Option 2"));
            options.Add(new OverlayPollOptionV3Model(3, "Option 3"));

            await this.NewPoll("Here is a simple question", 17, options, hasChannelPoints: true);
            await Task.Delay(2000);

            for (int i = 0; i < 5; i++)
            {
                options[i % options.Count].Amount += i + 1;
                options[i % options.Count].ChannelPoints += (i + 1) * 500;
                await this.Update();
                await Task.Delay(2000);
            }

            await this.End(options.OrderByDescending(o => o.Amount).First().ID);
        }

        private async Task NewPoll(string question, int timeLimitSeconds, IEnumerable<OverlayPollOptionV3Model> options, bool hasChannelPoints = false)
        {
            this.currentOptions.Clear();
            foreach (OverlayPollOptionV3Model option in options)
            {
                if (string.IsNullOrEmpty(option.Color) || !this.UseTwitchPredictionColors)
                {
                    if (this.UseRandomColors)
                    {
                        option.Color = OverlayItemV3ModelBase.GetRandomHTMLColor(option.Name);
                    }
                    else
                    {
                        option.Color = this.BarColor;
                    }
                }
                this.currentOptions[option.ID] = option;
            }

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties[QuestionPropertyName] = question;
            properties[TimeLimitPropertyName] = timeLimitSeconds;
            properties[nameof(this.ShowTwitchPredictionChannelPoints)] = hasChannelPoints ? this.ShowTwitchPredictionChannelPoints : false;
            properties[OptionsPropertyName] = this.currentOptions.Values;
            await this.CallFunction("newpoll", properties);
        }

        private async Task Update()
        {
            int total = this.currentOptions.Values.Sum(o => o.Amount);
            foreach (var kvp in this.currentOptions.Values)
            {
                kvp.Percentage = MathHelper.Truncate(((double)kvp.Amount / total) * 100, 2);
            }

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties[OptionsPropertyName] = this.currentOptions.Values;
            properties[TotalVotesPropertyName] = total;
            await this.CallFunction("update", properties);
        }
    }
}

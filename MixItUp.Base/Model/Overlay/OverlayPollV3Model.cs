using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
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

        public OverlayPollOptionV3Model() { }

        public OverlayPollOptionV3Model(TwitchPollEventModel.TwitchPollChoiceEventModel choice)
        {
            this.ID = choice.ID;
            this.Name = choice.Title;
        }

        public OverlayPollOptionV3Model(TwitchPredictionEventModel.TwitchPredictionOutcomeEventModel outcome)
        {
            this.ID = outcome.ID;
            this.Name = outcome.Title;
            this.Color = outcome.Color;
        }

        public Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties[nameof(this.ID)] = this.ID;
            properties[nameof(this.Name)] = this.Name;
            properties[nameof(this.Color)] = this.Color;
            properties[nameof(this.Amount)] = this.Amount;
            properties[nameof(this.Percentage)] = this.Percentage;
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
            return null;
        }

        public const string QuestionPropertyName = "Question";
        public const string OptionsPropertyName = "Options";
        public const string TotalVotesPropertyName = "TotalVotes";
        public const string WinnerIDPropertyName = "WinnerID";

        public static readonly string DefaultHTML = OverlayResources.OverlayPollDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayPollDefaultCSS + Environment.NewLine + Environment.NewLine + OverlayResources.OverlayTextDefaultCSS + Environment.NewLine + Environment.NewLine + OverlayResources.OverlayHeaderTextDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayPollDefaultJavascript;

        [DataMember]
        public OverlayPollHeaderV3Model Header { get; set; }

        [DataMember]
        public bool UseWithTwitchPolls { get; set; }
        [DataMember]
        public bool UseWithTwitchPredictions { get; set; }
        [DataMember]
        public bool UseWithTriviaGameCommand { get; set; }
        [DataMember]
        public bool UseWithBetGameCommand { get; set; }

        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string BorderColor { get; set; }
        [DataMember]
        public string BarColor { get; set; }

        [DataMember]
        public bool UseTwitchPredictionColors { get; set; }

        [DataMember]
        public OverlayAnimationV3Model EntranceAnimation { get; set; } = new OverlayAnimationV3Model();
        [DataMember]
        public OverlayAnimationV3Model ExitAnimation { get; set; } = new OverlayAnimationV3Model();

        private Dictionary<string, OverlayPollOptionV3Model> currentOptions = new Dictionary<string, OverlayPollOptionV3Model>();

        public OverlayPollV3Model() : base(OverlayItemV3Type.Poll) { }

        public async Task NewTwitchPoll(TwitchPollEventModel poll)
        {
            await this.NewPoll(poll.Title, poll.Choices.Select(c => new OverlayPollOptionV3Model(c)));
        }

        public async Task UpdateTwitchPoll(TwitchPollEventModel poll)
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

        public async Task NewTwitchPrediction(TwitchPredictionEventModel prediction)
        {
            await this.NewPoll(prediction.Title, prediction.Outcomes.Select(o => new OverlayPollOptionV3Model(o)));
        }

        public async Task UpdateTwitchPrediction(TwitchPredictionEventModel prediction)
        {
            foreach (var outcome in prediction.Outcomes)
            {
                if (this.currentOptions.TryGetValue(outcome.ID, out OverlayPollOptionV3Model model))
                {
                    model.Amount = outcome.Users;
                }
            }

            await this.Update();
        }

        public async Task EndTwitchPrediction(TwitchPredictionEventModel prediction)
        {
            await this.End(prediction.WinningOutcomeID);
        }

        public async Task End()
        {
            OverlayPollOptionV3Model option = this.currentOptions.Values.OrderByDescending(x => x.Amount).First();
            await this.End(option.ID);
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

            properties["EntranceAnimationFramework"] = this.EntranceAnimation.AnimationFramework;
            properties["EntranceAnimationName"] = this.EntranceAnimation.AnimationName;
            properties["ExitAnimationFramework"] = this.ExitAnimation.AnimationFramework;
            properties["ExitAnimationName"] = this.ExitAnimation.AnimationName;

            return properties;
        }

        private async Task NewPoll(string question, IEnumerable<OverlayPollOptionV3Model> options)
        {
            this.currentOptions.Clear();
            foreach (OverlayPollOptionV3Model option in options)
            {
                if (string.IsNullOrEmpty(option.Color) || !this.UseTwitchPredictionColors)
                {
                    option.Color = this.BarColor;
                }
                this.currentOptions[option.ID] = option;
            }

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties[QuestionPropertyName] = question;
            properties[OptionsPropertyName] = this.currentOptions.Values;
            await this.CallFunction("newpoll", properties);
        }

        private async Task Update()
        {
            int total = this.currentOptions.Values.Sum(o => o.Amount);
            foreach (var kvp in this.currentOptions.Values)
            {
                kvp.Percentage = MathHelper.Truncate((double)kvp.Amount / total, 2);
            }

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties[OptionsPropertyName] = this.currentOptions.Values;
            properties[TotalVotesPropertyName] = total;
            await this.CallFunction("update", properties);
        }

        private async Task End(string id)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties[OverlayPollV3Model.WinnerIDPropertyName] = id;
            await this.CallFunction("end", properties);
        }
    }
}

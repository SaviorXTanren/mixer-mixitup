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
        public int Amount { get; set; }
        public double Percentage { get; set; }

        public Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties[nameof(this.ID)] = this.ID;
            properties[nameof(this.Name)] = this.Name;
            properties[nameof(this.Amount)] = this.Amount;
            properties[nameof(this.Percentage)] = this.Percentage;
            return properties;
        }
    }

    public class OverlayPollV3Model : OverlayVisualTextV3ModelBase
    {
        public const string QuestionPropertyName = "Question";
        public const string OptionsPropertyName = "Options";

        public static readonly string DefaultHTML = OverlayResources.OverlayPollDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayPollDefaultCSS + Environment.NewLine + Environment.NewLine + OverlayResources.OverlayTextDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayPollDefaultJavascript;

        [DataMember]
        public bool UseWithTwitchPolls { get; set; }
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
        public OverlayAnimationV3Model IncreaseAnimation { get; set; } = new OverlayAnimationV3Model();
        [DataMember]
        public OverlayAnimationV3Model DecreaseAnimation { get; set; } = new OverlayAnimationV3Model();

        private Dictionary<string, OverlayPollOptionV3Model> currentOptions = new Dictionary<string, OverlayPollOptionV3Model>();

        public OverlayPollV3Model() : base(OverlayItemV3Type.Poll) { }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();

            properties[nameof(this.BackgroundColor)] = this.BackgroundColor;
            properties[nameof(this.BorderColor)] = this.BorderColor;
            properties[nameof(this.BarColor)] = this.BarColor;

            properties["IncreaseAnimationFramework"] = this.IncreaseAnimation.AnimationFramework;
            properties["IncreaseAnimationName"] = this.IncreaseAnimation.AnimationName;
            properties["DecreaseAnimationFramework"] = this.DecreaseAnimation.AnimationFramework;
            properties["DecreaseAnimationName"] = this.DecreaseAnimation.AnimationName;

            return properties;
        }

        public async Task NewPoll(string question, Dictionary<string, string> options)
        {
            this.currentOptions.Clear();
            foreach (var kvp in options)
            {
                this.currentOptions[kvp.Key] = new OverlayPollOptionV3Model()
                {
                    ID = kvp.Key,
                    Name = kvp.Value
                };
            }

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties[QuestionPropertyName] = question;
            properties[OptionsPropertyName] = this.currentOptions.Values;
            await this.CallFunction("newpoll", properties);
        }

        public async Task Update(string id, int amount)
        {
            if (this.currentOptions.TryGetValue(id, out OverlayPollOptionV3Model model))
            {
                model.Amount += amount;

                int total = this.currentOptions.Values.Sum(o => o.Amount);

                foreach (var kvp in this.currentOptions.Values)
                {
                    kvp.Percentage = MathHelper.Truncate((double)kvp.Amount / total, 2);
                }

                Dictionary<string, object> properties = new Dictionary<string, object>();
                properties[OptionsPropertyName] = this.currentOptions.Values;
                await this.CallFunction("update", properties);
            }
        }
    }
}

using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Services.Twitch.New;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.EventSub
{
    public class PredictionNotificationOutcome
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Color { get; set; }
        public int Users { get; set; }
        public int ChannelPoints { get; set; }

        public List<PredictionNotificationOutcomePredictor> TopPredictors { get; set; } = new List<PredictionNotificationOutcomePredictor>();
    }

    public class PredictionNotificationOutcomePredictor
    {
        public TwitchUserPlatformV2Model User { get; set; }
        public int ChannelPointsUsed { get; set; }
        public int ChannelPointsWon { get; set; }
    }

    public class PredictionNotification
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string WinningOutcomeID { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset LocksAt { get; set; }
        public List<PredictionNotificationOutcome> Outcomes { get; set; } = new List<PredictionNotificationOutcome>();

        public PredictionNotification(JObject payload)
        {
            this.ID = payload["id"].Value<string>();
            this.Title = payload["title"].Value<string>();

            this.StartedAt = TwitchService.GetTwitchDateTime(payload["started_at"].Value<string>());
            if (payload.ContainsKey("locks_at"))
            {
                this.LocksAt = TwitchService.GetTwitchDateTime(payload["locks_at"].Value<string>());
            }
            else if (payload.ContainsKey("locked_at"))
            {
                this.LocksAt = TwitchService.GetTwitchDateTime(payload["locked_at"].Value<string>());
            }

            this.WinningOutcomeID = payload.GetValue("winning_outcome_id")?.Value<string>();

            foreach (JObject oc in (JArray)payload["outcomes"])
            {
                int.TryParse(oc.GetValue("users")?.Value<string>(), out int users);
                int.TryParse(oc.GetValue("channel_points")?.Value<string>(), out int channelPoints);

                PredictionNotificationOutcome outcome = new PredictionNotificationOutcome()
                {
                    ID = oc["id"].Value<string>(),
                    Title = oc["title"].Value<string>(),
                    Color = oc["color"].Value<string>(),
                    Users = users,
                    ChannelPoints = channelPoints
                };

                this.Outcomes.Add(outcome);

                if (oc.TryGetValue("top_predictors", out JToken topPredictors))
                {
                    foreach (JObject topPredictor in (JArray)topPredictors)
                    {
                        int.TryParse(topPredictor.GetValue("channel_points_used")?.Value<string>(), out int channelPointsUsed);
                        int.TryParse(topPredictor.GetValue("channel_points_won")?.Value<string>(), out int channelPointsWon);

                        outcome.TopPredictors.Add(new PredictionNotificationOutcomePredictor()
                        {
                            User = new TwitchUserPlatformV2Model(topPredictor["user_id"].Value<string>(), topPredictor["user_login"].Value<string>(), topPredictor["user_name"].Value<string>()),
                            ChannelPointsUsed = channelPointsUsed,
                            ChannelPointsWon = channelPointsWon,
                        });
                    }
                }
            }
        }
    }
}

using MixItUp.Base.Services.Twitch.New;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.EventSub
{
    public class PollNotificationChoice
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public int Votes { get; set; }
    }

    public class PollNotification
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public List<PollNotificationChoice> Choices { get; set; } = new List<PollNotificationChoice>();
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset EndsAt { get; set; }

        public PollNotification(JObject payload)
        {
            this.ID = payload["id"].Value<string>();
            this.Title = payload["title"].Value<string>();
            this.StartedAt = TwitchService.GetTwitchDateTime(payload["started_at"].Value<string>());
            if (payload.ContainsKey("ends_at"))
            {
                this.EndsAt = TwitchService.GetTwitchDateTime(payload["ends_at"].Value<string>());
            }
            else if (payload.ContainsKey("ended_at"))
            {
                this.EndsAt = TwitchService.GetTwitchDateTime(payload["ended_at"].Value<string>());
            }

            foreach (JObject choice in (JArray)payload["choices"])
            {
                int.TryParse(choice.GetValue("votes")?.Value<string>(), out int votes);

                this.Choices.Add(new PollNotificationChoice()
                {
                    ID = choice["id"].Value<string>(),
                    Title = choice["title"].Value<string>(),
                    Votes = votes,
                });
            }
        }
    }
}

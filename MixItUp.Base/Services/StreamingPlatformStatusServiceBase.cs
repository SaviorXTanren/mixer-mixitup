using MixItUp.Base.Model;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public class StreamingPlatformStatusModel
    {
        public StreamingPlatformTypeEnum Platform { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTimeOffset LastUpdated { get; set; }

        public string Link { get; set; }

        public StreamingPlatformStatusModel(StreamingPlatformTypeEnum platform, string title, string description, DateTimeOffset lastUpdated, string link)
        {
            this.Platform = platform;
            this.Title = title;
            this.Description = description;
            this.LastUpdated = lastUpdated;
            this.Link = link;
        }
    }

    public class StatusPageStreamingPlatformStatusService : StreamingPlatformStatusServiceBase
    {
        private class StatusPageUnresolvedIncidents
        {
            public List<StatusPageUnresolvedIncident> incidents { get; set; }
        }

        private class StatusPageUnresolvedIncident
        {
            public string id { get; set; }
            public string name { get; set; }
            public string status { get; set; }
            public DateTimeOffset created_at { get; set; }
            public DateTimeOffset updated_at { get; set; }
            public object monitoring_at { get; set; }
            public object resolved_at { get; set; }
            public string impact { get; set; }
            public string shortlink { get; set; }
            public DateTimeOffset started_at { get; set; }
            public string page_id { get; set; }
            public List<StatusPageUnresolvedIncidentUpdate> incident_updates { get; set; }
        }

        private class StatusPageUnresolvedIncidentUpdate
        {
            public string id { get; set; }
            public string status { get; set; }
            public string body { get; set; }
            public string incident_id { get; set; }
            public DateTimeOffset created_at { get; set; }
            public DateTimeOffset updated_at { get; set; }
            public DateTimeOffset display_at { get; set; }
            public bool deliver_notifications { get; set; }
            public object custom_tweet { get; set; }
            public object tweet_id { get; set; }
        }

        private StreamingPlatformTypeEnum platform;
        private string statusFeedLink;

        public StatusPageStreamingPlatformStatusService(StreamingPlatformTypeEnum platform, string statusFeedLink)
        {
            this.platform = platform;
            this.statusFeedLink = statusFeedLink;
        }

        public override Task<IEnumerable<StreamingPlatformStatusModel>> GetCurrentIncidents()
        {
            List<StreamingPlatformStatusModel> incidents = new List<StreamingPlatformStatusModel>();
            //try
            //{
            //    StatusPageUnresolvedIncidents unresolvedIncidents = null;
            //    using (AdvancedHttpClient client = new AdvancedHttpClient())
            //    {
            //        unresolvedIncidents = await client.GetAsync<StatusPageUnresolvedIncidents>(this.statusFeedLink);
            //    }

            //    if (unresolvedIncidents != null && unresolvedIncidents.incidents != null && unresolvedIncidents.incidents.Count > 0)
            //    {
            //        foreach (StatusPageUnresolvedIncident incident in unresolvedIncidents.incidents)
            //        {
            //            if (incident.incident_updates != null && incident.incident_updates.Count > 0)
            //            {
            //                StatusPageUnresolvedIncidentUpdate latestUpdate = incident.incident_updates.OrderByDescending(i => i.updated_at).FirstOrDefault();
            //                if (latestUpdate != null)
            //                {
            //                    incidents.Add(new StreamingPlatformStatusModel(this.platform, incident.name, latestUpdate.body, latestUpdate.updated_at, incident.shortlink));
            //                }
            //            }
            //        }
            //    }
            //    return incidents;
            //}
            //catch (Exception ex) { Logger.Log(ex); }
            return Task.FromResult<IEnumerable<StreamingPlatformStatusModel>>(incidents);
        }
    }

    public abstract class StreamingPlatformStatusServiceBase
    {
        public abstract Task<IEnumerable<StreamingPlatformStatusModel>> GetCurrentIncidents();
    }
}

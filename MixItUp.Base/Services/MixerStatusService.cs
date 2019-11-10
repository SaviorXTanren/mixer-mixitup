using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public class MixerIncident
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset LastUpdate { get; set; }
        public string Link { get; set; }
    }

    public interface IMixerStatusService
    {
        Task<IEnumerable<MixerIncident>> GetCurrentIncidents();
    }

    public class MixerStatusService : IMixerStatusService
    {
        private class MixerUnresolvedIncidents
        {
            public List<MixerUnresolvedIncident> incidents { get; set; }
        }

        private class MixerUnresolvedIncident
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
            public List<MixerUnresolvedIncidentUpdate> incident_updates { get; set; }
        }

        private class MixerUnresolvedIncidentUpdate
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

        private const string MixerStatusFeedLink = "https://status.mixer.com/api/v2/incidents/unresolved.json";

        public async Task<IEnumerable<MixerIncident>> GetCurrentIncidents()
        {
            try
            {
                MixerUnresolvedIncidents unresolvedIncidents = null;
                using (AdvancedHttpClient client = new AdvancedHttpClient())
                {
                    unresolvedIncidents = await client.GetAsync<MixerUnresolvedIncidents>(MixerStatusService.MixerStatusFeedLink);
                }

                List<MixerIncident> incidents = new List<MixerIncident>();
                if (unresolvedIncidents != null && unresolvedIncidents.incidents != null && unresolvedIncidents.incidents.Count > 0)
                {
                    foreach (MixerUnresolvedIncident incident in unresolvedIncidents.incidents)
                    {
                        if (incident.incident_updates != null && incident.incident_updates.Count > 0)
                        {
                            MixerUnresolvedIncidentUpdate latestUpdate = incident.incident_updates.OrderByDescending(i => i.updated_at).FirstOrDefault();
                            if (latestUpdate != null)
                            {

                            }

                            incidents.Add(new MixerIncident()
                            {
                                Title = incident.name,
                                Description = latestUpdate.body,
                                LastUpdate = latestUpdate.updated_at,
                                Link = incident.shortlink
                            });
                        }
                    }
                }
                return incidents;
            }
            catch (Exception ex) { Logger.Log(ex); }
            return new List<MixerIncident>();
        }
    }
}

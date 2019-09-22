using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace MixItUp.Base.Services
{
    public class MixerIncident
    {
        private const string IncidentDescriptionResolvedIndicator = "<strong>Resolved</strong>";
        private const string IncidentDescriptionCompletedIndicator = "<strong>Completed</strong>";

        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset LastUpdate { get; set; }
        public string Link { get; set; }

        public bool IsResolved { get { return this.Description.Contains(IncidentDescriptionResolvedIndicator) || this.Description.Contains(IncidentDescriptionCompletedIndicator); } }
    }

    public interface IMixerStatusService
    {
        Task<IEnumerable<MixerIncident>> GetCurrentIncidents();
    }

    public class MixerStatusService : IMixerStatusService
    {
        private const string MixerStatusRSSFeedLink = "https://status.mixer.com/history.rss";

        public async Task<IEnumerable<MixerIncident>> GetCurrentIncidents()
        {
            string rssData = null;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    rssData = await client.GetStringAsync(MixerStatusService.MixerStatusRSSFeedLink);
                }
                catch (Exception ex) { Logger.Log(ex); }
            }

            List<MixerIncident> incidents = new List<MixerIncident>();
            if (!string.IsNullOrEmpty(rssData))
            {
                try
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(rssData);
                    foreach (XmlNode node in xmlDocument.SelectNodes("rss/channel/item"))
                    {
                        incidents.Add(new MixerIncident()
                        {
                            Title = node.SelectSingleNode("title").InnerText,
                            Description = HttpUtility.HtmlDecode(node.SelectSingleNode("description").InnerText),
                            LastUpdate = DateTimeOffset.Parse(node.SelectSingleNode("pubDate").InnerText),
                            Link = node.SelectSingleNode("link").InnerText,
                        });
                    }
                }
                catch (Exception ex) { Logger.Log(ex); }
            }
            return incidents.Where(i => !i.IsResolved);
        }
    }
}

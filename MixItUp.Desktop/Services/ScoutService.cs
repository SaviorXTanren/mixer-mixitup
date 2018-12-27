using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Mixer.Base.Model.OAuth;
using Mixer.Base.Services;
using Mixer.Base.Web;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;

namespace MixItUp.Desktop.Services
{
    public class ScoutService : RestServiceBase, IScoutService
    {
        private const string BaseAddress = "https://api.scoutsdk.com/";

        private const string ClientID = "ee0f5e9a-af3b-464f-9032-1b72c6903be1";

        public ScoutService() { }

        public async Task<ScoutUser> GetUser(string title, string identifier, Dictionary<string, string> parameters = null)
        {
            try
            {
                if (parameters == null)
                {
                    parameters = new Dictionary<string, string>();
                }

                parameters["title"] = title;
                parameters["identifier"] = identifier;

                List<string> parameterSets = new List<string>();
                foreach (var parameter in parameters)
                {
                    parameterSets.Add(string.Format("{0}: \"{1}\"", parameter.Key, parameter.Value));
                }

                string query = "{ players(" + string.Join(", ", parameterSets) + ") { results { player { playerId handle } persona { id handle } } } }";

                JObject content = new JObject();
                content["query"] = query;
                content["variables"] = new JObject();

                HttpResponseMessage response = await this.PostAsync("graph", this.CreateContentFromObject(content));
                if (response.IsSuccessStatusCode)
                {
                    JObject jobj = await this.ProcessJObjectResponse(response);
                    if (jobj.ContainsKey("data"))
                    {
                        JObject data = (JObject)jobj["data"];
                        if (data.ContainsKey("players"))
                        {
                            JObject players = (JObject)data["players"];
                            if (players.ContainsKey("results"))
                            {
                                JArray results = (JArray)players["results"];
                                foreach (JObject result in results)
                                {
                                    ScoutUser user = new ScoutUser(result);
                                    if (!string.IsNullOrEmpty(user.PlayerID) && !string.IsNullOrEmpty(user.PlayerHandle) && user.PlayerHandle.Equals(identifier, StringComparison.InvariantCulture))
                                    {
                                        return user;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<Dictionary<string, ScoutStat>> GetStats(string title, ScoutUser user, string segment, Dictionary<string, string> parameters = null)
        {
            try
            {
                if (parameters == null)
                {
                    parameters = new Dictionary<string, string>();
                }

                parameters["title"] = title;
                parameters["id"] = user.PlayerID;
                if (!string.IsNullOrEmpty(segment))
                {
                    parameters["segment"] = segment;
                }

                List<string> parameterSets = new List<string>();
                foreach (var parameter in parameters)
                {
                    parameterSets.Add(string.Format("{0}: \"{1}\"", parameter.Key, parameter.Value));
                }

                string query = "{ player(" + string.Join(", ", parameterSets) + ") { id metadata { key value } ";
                if (!string.IsNullOrEmpty(segment))
                {
                    query += "segments { metadata { key value } stats { metadata { key } value } }";
                }
                else
                {
                    query += "stats { metadata { key } value }";
                }
                query += " } }";

                JObject content = new JObject();
                content["query"] = query;
                content["variables"] = new JObject();

                HttpResponseMessage response = await this.PostAsync("graph", this.CreateContentFromObject(content));
                if (response.IsSuccessStatusCode)
                {
                    JObject jobj = await this.ProcessJObjectResponse(response);
                    if (jobj.ContainsKey("data"))
                    {
                        JObject data = (JObject)jobj["data"];
                        if (data.ContainsKey("player"))
                        {
                            JObject player = (JObject)data["player"];
                            if (player.ContainsKey("id") && player["id"].ToString().Equals(user.PlayerID, StringComparison.InvariantCulture))
                            {
                                List<ScoutStat> results = new List<ScoutStat>();

                                if (player.ContainsKey("metadata"))
                                {
                                    JArray metadata = (JArray)player["metadata"];
                                    foreach (JObject stat in metadata)
                                    {
                                        results.Add(new ScoutStat(stat));
                                    }
                                }

                                if (player.ContainsKey("stats"))
                                {
                                    JArray stats = (JArray)player["stats"];
                                    foreach (JObject stat in stats)
                                    {
                                        results.Add(new ScoutStat(stat));
                                    }
                                }

                                if (player.ContainsKey("segments"))
                                {
                                    JArray segments = (JArray)player["segments"];
                                    foreach (JObject seg in segments)
                                    {
                                        if (seg.ContainsKey("stats"))
                                        {
                                            JArray stats = (JArray)seg["stats"];
                                            foreach (JObject stat in stats)
                                            {
                                                results.Add(new ScoutStat(stat));
                                            }
                                        }
                                    }
                                }

                                return results.ToDictionary(s => s.Name, s => s);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        protected override Task<HttpClientWrapper> GetHttpClient(bool autoRefreshToken = true)
        {
            HttpClientWrapper client = new HttpClientWrapper(this.GetBaseAddress());
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/com.scoutsdk.graph+json; version=1.1.0; charset=utf8");
            client.DefaultRequestHeaders.Add("Scout-App", ScoutService.ClientID);
            return Task.FromResult(client);
        }

        protected override Task<OAuthTokenModel> GetOAuthToken(bool autoRefreshToken = true) { return Task.FromResult(new OAuthTokenModel()); }

        protected override string GetBaseAddress() { return ScoutService.BaseAddress; }
    }
}

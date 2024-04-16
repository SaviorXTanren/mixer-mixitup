using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public enum MtionStudioParameterTypeEnum
    {
        String,
        Number,
        Boolean,
        Enum,
    }

    [DataContract]
    public class MtionStudioClubhouse
    {
        [DataMember]
        public List<MtionStudioTrigger> triggers { get; set; }
    }

    [DataContract]
    public class MtionStudioTrigger
    {
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public List<MtionStudioTriggerParameter> output_parameters { get; set; } = new List<MtionStudioTriggerParameter>();
    }

    [DataContract]
    public class MtionStudioTriggerParameter
    {
        private const string StringParameterType = "string";
        private const string NumberParameterType = "number";
        private const string BooleanParameterType = "bool";
        private const string EnumParameterType = "enum";

        [DataMember]
        public int parameter_index { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string data_type { get; set; }

        [JsonIgnore]
        public MtionStudioParameterTypeEnum Type
        {
            get
            {
                if (this.IsNumber)
                {
                    return MtionStudioParameterTypeEnum.Number;
                }
                else if (this.IsEnum)
                {
                    return MtionStudioParameterTypeEnum.Enum;
                }
                else if (this.IsBoolean)
                {
                    return MtionStudioParameterTypeEnum.Boolean;
                }
                return MtionStudioParameterTypeEnum.String;
            }
        }

        [JsonIgnore]
        public bool IsString { get { return string.Equals(StringParameterType, this.data_type, StringComparison.OrdinalIgnoreCase); } }
        [JsonIgnore]
        public bool IsNumber { get { return string.Equals(NumberParameterType, this.data_type, StringComparison.OrdinalIgnoreCase); } }
        [JsonIgnore]
        public bool IsBoolean { get { return string.Equals(BooleanParameterType, this.data_type, StringComparison.OrdinalIgnoreCase); } }
        [JsonIgnore]
        public bool IsEnum { get { return string.Equals(EnumParameterType, this.data_type, StringComparison.OrdinalIgnoreCase); } }
    }

    [DataContract]
    public class MtionStudioTriggerInputParameter
    {
        [DataMember]
        public int parameter_index { get; set; }
        [DataMember]
        public object value { get; set; }
    }

    public class MtionStudioService : IExternalService
    {
        private const string BaseAddress = "http://localhost:35393/external-trigger/";

        private const int MaxCacheDuration = 30;

        public string Name { get { return Resources.MtionStudio; } }

        public bool IsConnected { get; private set; }

        private MtionStudioClubhouse clubhouseCache;
        private DateTimeOffset clubhouseCacheExpiration = DateTimeOffset.MinValue;

        private Dictionary<string, MtionStudioTrigger> triggerCache = new Dictionary<string, MtionStudioTrigger>();
        private DateTimeOffset triggerCacheExpiration = DateTimeOffset.MinValue;

        public async Task<Result> Connect()
        {
            MtionStudioClubhouse clubhouse = await this.GetCurrentClubhouseTriggers();
            if (clubhouse != null)
            {
                this.IsConnected = true;
                return new Result();
            }
            return new Result(Resources.MtionStudioFailedToGetClubhouseData);
        }

        public Task Disconnect()
        {
            this.IsConnected = false;
            this.ClearCaches();
            return Task.CompletedTask;
        }

        public async Task<MtionStudioClubhouse> GetCurrentClubhouseTriggers()
        {
            try
            {
                if (this.clubhouseCacheExpiration <= DateTimeOffset.Now || this.clubhouseCache == null)
                {
                    using (AdvancedHttpClient client = new AdvancedHttpClient(MtionStudioService.BaseAddress))
                    {
                        List<MtionStudioTrigger> triggers = await client.GetAsync<List<MtionStudioTrigger>>("triggers");
                        if (triggers != null)
                        {
                            this.clubhouseCache = new MtionStudioClubhouse()
                            {
                                triggers = triggers
                            };
                        }
                    }
                }

                if (this.clubhouseCache != null)
                {
                    return this.clubhouseCache;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        public async Task<MtionStudioTrigger> GetTrigger(string id)
        {
            try
            {
                if (this.triggerCacheExpiration <= DateTimeOffset.Now || !this.triggerCache.ContainsKey(id))
                {
                    using (AdvancedHttpClient client = new AdvancedHttpClient(MtionStudioService.BaseAddress))
                    {
                        MtionStudioTrigger trigger = await client.GetAsync<MtionStudioTrigger>($"trigger/{id}");
                        if (trigger != null)
                        {
                            this.triggerCache[id] = trigger;
                            this.triggerCacheExpiration = DateTimeOffset.Now.AddMinutes(MaxCacheDuration);
                        }
                    }
                }

                if (this.triggerCache.ContainsKey(id))
                {
                    return this.triggerCache[id];
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        public async Task<bool> FireTrigger(string id, IEnumerable<object> parameters)
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient(MtionStudioService.BaseAddress))
                {
                    List<MtionStudioTriggerInputParameter> inputs = new List<MtionStudioTriggerInputParameter>();
                    for (int i = 0; parameters != null && i < parameters.Count(); i++)
                    {
                        inputs.Add(new MtionStudioTriggerInputParameter()
                        {
                            parameter_index = i,
                            value = parameters.ElementAt(i)
                        });
                    }

                    HttpResponseMessage response = await client.PatchAsync($"fire-trigger/{id}", AdvancedHttpClient.CreateContentFromObject(inputs));
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return false;
        }

        public void ClearCaches()
        {
            this.clubhouseCache = null;
            this.clubhouseCacheExpiration = DateTimeOffset.MinValue;

            this.triggerCache.Clear();
            this.triggerCacheExpiration = DateTimeOffset.MinValue;
        }
    }
}

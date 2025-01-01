using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
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
        Text,
        Number,
        Boolean,
        Enum,
    }

    [DataContract]
    public class MtionStudioClubhouse
    {
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string description { get; set; }
        [DataMember]
        public List<MtionStudioTrigger> external_trigger_datas { get; set; }
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
        private const string TextParameterType = "string";
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
                return MtionStudioParameterTypeEnum.Text;
            }
        }

        [JsonIgnore]
        public bool IsText { get { return string.Equals(TextParameterType, this.data_type, StringComparison.OrdinalIgnoreCase); } }
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
        private const string BaseAddress = "http://localhost:35393/";

        private const int MaxCacheDuration = 30;

        public string Name { get { return Resources.MtionStudio; } }

        public bool IsConnected { get; private set; }

        private List<MtionStudioClubhouse> clubhouseCache = new List<MtionStudioClubhouse>();
        private DateTimeOffset clubhouseCacheExpiration = DateTimeOffset.MinValue;

        public async Task<Result> Connect()
        {
            IEnumerable<MtionStudioClubhouse> clubhouses = await this.GetAllClubhouses();
            if (clubhouses != null)
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

        public async Task<IEnumerable<MtionStudioClubhouse>> GetAllClubhouses()
        {
            try
            {
                if (this.clubhouseCacheExpiration <= DateTimeOffset.Now || this.clubhouseCache == null)
                {
                    using (AdvancedHttpClient client = new AdvancedHttpClient(MtionStudioService.BaseAddress))
                    {
                        List<MtionStudioClubhouse> clubhouses = await client.GetAsync<List<MtionStudioClubhouse>>("clubhouses");
                        if (clubhouses != null)
                        {
                            this.clubhouseCache = clubhouses;
                            this.clubhouseCacheExpiration = DateTimeOffset.Now.AddMinutes(MaxCacheDuration);
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

        public async Task<MtionStudioClubhouse> GetClubhouse(string id)
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient(MtionStudioService.BaseAddress))
                {
                    return await client.GetAsync<MtionStudioClubhouse>($"clubhouse/{id}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        public async Task<MtionStudioClubhouse> GetActiveClubhouse()
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient(MtionStudioService.BaseAddress))
                {
                    return await client.GetAsync<MtionStudioClubhouse>($"active-clubhouse");
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
                        if (parameters.ElementAt(i) != null)
                        {
                            inputs.Add(new MtionStudioTriggerInputParameter()
                            {
                                parameter_index = i,
                                value = parameters.ElementAt(i)
                            });
                        }
                    }

                    HttpResponseMessage response = await client.PatchAsync($"external-trigger/fire-trigger/{id}", AdvancedHttpClient.CreateContentFromObject(inputs));
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
            this.clubhouseCache.Clear();
            this.clubhouseCacheExpiration = DateTimeOffset.MinValue;
        }
    }
}

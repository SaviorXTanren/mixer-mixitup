using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class MtionStudioActionParameterModel
    {
        [DataMember]
        public MtionStudioParameterTypeEnum Type { get; set; }
        [DataMember]
        public string Value { get; set; }
    }

    [DataContract]
    public class MtionStudioActionModel : ActionModelBase
    {
        [DataMember]
        public string ClubhouseID { get; set; }
        [DataMember]
        public string TriggerID { get; set; }
        [DataMember]
        public IEnumerable<MtionStudioActionParameterModel> Parameters { get; set; }

        public MtionStudioActionModel(string clubhouseID, string triggerID, IEnumerable<MtionStudioActionParameterModel> parameters)
            : base(ActionTypeEnum.MtionStudio)
        {
            this.ClubhouseID = clubhouseID;
            this.TriggerID = triggerID;
            this.Parameters = parameters;
        }

        [Obsolete]
        public MtionStudioActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ChannelSession.Settings.MtionStudioEnabled && !ServiceManager.Get<MtionStudioService>().IsConnected)
            {
                Result result = await ServiceManager.Get<MtionStudioService>().Connect();
                if (!result.Success)
                {
                    return;
                }
            }

            if (ServiceManager.Get<MtionStudioService>().IsConnected)
            {
                List<object> inputs = new List<object>();
                for (int i = 0; i < this.Parameters.Count(); i++)
                {
                    MtionStudioActionParameterModel parameter = this.Parameters.ElementAt(i);
                    string value = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(parameter.Value, parameters);
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (parameter.Type == MtionStudioParameterTypeEnum.Number || parameter.Type == MtionStudioParameterTypeEnum.Enum)
                        {
                            int.TryParse(value, out int v);
                            inputs.Add(v);
                        }
                        else if (parameter.Type == MtionStudioParameterTypeEnum.Boolean)
                        {
                            value = value.ToLower();
                            if (bool.TryParse(value, out bool v))
                            {
                                inputs.Add(v);
                            }
                            else if (int.TryParse(value, out int iv))
                            {
                                inputs.Add(iv > 0);
                            }
                        }
                        else
                        {
                            inputs.Add(value);
                        }
                    }
                    else
                    {
                        inputs.Add(null);
                    }
                }
                await ServiceManager.Get<MtionStudioService>().FireTrigger(this.TriggerID, inputs);
            }
        }
    }
}
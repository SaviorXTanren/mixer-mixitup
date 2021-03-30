using MixItUp.Base.Model.Commands;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Store
{
    [DataContract]
    public class StoreCommandModel
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public HashSet<string> Tags { get; set; } = new HashSet<string>();

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string UserAvatarURL { get; set; }

        [DataMember]
        public double AverageRating { get; set; }

        [DataMember]
        public int Downloads { get; set; }

        [DataMember]
        public DateTimeOffset LastUpdated { get; set; }

        [JsonIgnore]
        public string TagsString
        {
            get { return string.Join(",", this.Tags); }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    string[] splits = value.Split(',');
                    if (splits != null && splits.Length > 0)
                    {
                        foreach (string split in splits)
                        {
                            this.Tags.Add(split);
                        }
                    }
                }
            }
        }
    }

    [DataContract]
    public class StoreCommandDetailsModel : StoreCommandModel
    {
        [DataMember]
        public string Data
        {
            get { return JSONSerializerHelper.SerializeToString(this.Commands); }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.Commands.AddRange(JSONSerializerHelper.DeserializeFromString<List<CommandModelBase>>(value));
                }
            }
        }

        [JsonIgnore]
        public List<CommandModelBase> Commands { get; set; } = new List<CommandModelBase>();
    }

    [DataContract]
    public class StoreCommandUploadModel : StoreCommandDetailsModel
    {
        [DataMember]
        public string MixItUpUserID { get; set; }
    }
}

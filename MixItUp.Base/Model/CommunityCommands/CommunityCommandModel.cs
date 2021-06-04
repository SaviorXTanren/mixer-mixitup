using MixItUp.Base.Model.Commands;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Store
{
    [DataContract]
    public class CommunityCommandModel
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string ImageURL { get; set; }

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
    public class CommunityCommandDetailsModel : CommunityCommandModel
    {
        [DataMember]
        public string Data { get; set; }

        [DataMember]
        public List<CommunityCommandReviewModel> Reviews { get; set; } = new List<CommunityCommandReviewModel>();

        public CommandModelBase GetCommand()
        {
            try
            {
                return JSONSerializerHelper.DeserializeFromString<CommandModelBase>(this.Data);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        public void SetCommand(CommandModelBase command)
        {
            try
            {
                this.Data = JSONSerializerHelper.SerializeToString(command);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}

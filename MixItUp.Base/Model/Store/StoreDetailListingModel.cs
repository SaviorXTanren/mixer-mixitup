using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Store
{
    [DataContract]
    public class StoreDetailListingModel : StoreListingModel
    {
        [DataMember]
        public string Data { get; set; }

        [DataMember]
        public byte[] AssetData { get; set; }

        [DataMember]
        public byte[] DisplayImageData { get; set; }

        [DataMember]
        public List<StoreListingReviewModel> Reviews { get; set; }

        public StoreDetailListingModel()
        {
            this.Reviews = new List<StoreListingReviewModel>();
        }

        public StoreDetailListingModel(CommandBase command, string name, string description, IEnumerable<string> tags, string displayImagePath, byte[] displayImageData, byte[] assetData)
            : this()
        {
            this.ID = command.ID;
            this.UserID = ChannelSession.User.id;
            this.AppVersion = ChannelSession.Services.FileService.GetApplicationVersion();
            this.Name = name;
            this.Description = description;
            this.Tags.AddRange(tags);
            this.DisplayImageLink = displayImagePath;
            this.AssetsIncluded = (assetData != null);

            this.DisplayImageData = displayImageData;
            this.AssetData = assetData;
            this.Data = SerializerHelper.SerializeToString(command.Actions);
        }

        public List<ActionBase> GetActions()
        {
            if (!string.IsNullOrEmpty(this.Data))
            {
                return SerializerHelper.DeserializeFromString<List<ActionBase>>(this.Data);
            }
            return new List<ActionBase>();
        }
    }
}

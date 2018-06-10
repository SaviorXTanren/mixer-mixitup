using MixItUp.Base.Actions;
using MixItUp.Base.Util;
using Newtonsoft.Json;
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
        public List<StoreListingReviewModel> Reviews { get; set; }

        public StoreDetailListingModel()
        {
            this.Reviews = new List<StoreListingReviewModel>();
        }

        [JsonIgnore]
        public List<ActionBase> Actions
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Data))
                {
                    return SerializerHelper.DeserializeFromString<List<ActionBase>>(this.Data);
                }
                return new List<ActionBase>();
            }
        }
    }
}

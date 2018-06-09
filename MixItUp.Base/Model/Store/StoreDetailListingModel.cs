using MixItUp.Base.Actions;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Store
{
    [DataContract]
    public class StoreDetailListingModel : StoreListingModel
    {
        [DataMember]
        public List<ActionBase> Actions { get; set; }

        [DataMember]
        public List<StoreListingReviewModel> Reviews { get; set; }

        public StoreDetailListingModel()
        {
            this.Actions = new List<ActionBase>();
            this.Reviews = new List<StoreListingReviewModel>();
        }
    }
}

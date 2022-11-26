using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayEventListV3ViewModel : OverlayItemV3ViewModelBase
    {
        public const string AddedAnimationName = "Added";
        public const string RemovedAnimationName = "Removed";

        public OverlayEventListV3ViewModel()
            : base(OverlayItemV3Type.EventList)
        {
            this.AddAnimations(new List<string>() { OverlayEventListV3ViewModel.AddedAnimationName, OverlayEventListV3ViewModel.RemovedAnimationName });
        }

        public OverlayEventListV3ViewModel(OverlayEventListV3Model item)
            : base(item)
        {

        }

        public override Result Validate()
        {
            Result result = base.Validate();

            if (result.Success)
            {

            }

            return result;
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayEventListV3Model item = (OverlayEventListV3Model)this.GetItem();

            return item;
        }
    }
}

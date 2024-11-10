using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.Overlay
{
    [Obsolete]
    public abstract class OverlayItemViewModelBase : ControlViewModelBase
    {
        public IEnumerable<int> SampleFontSizes { get; } = new List<int>() { 12, 24, 36, 48, 60, 72, 84, 96, 108, 120 };
        public IEnumerable<string> SampleFontSizesStrings { get { return this.SampleFontSizes.Select(f => f.ToString()); } }

        public IEnumerable<string> ColorNames { get; set; }

        public IEnumerable<OverlayItemEffectEntranceAnimationTypeEnum> EntranceAnimations { get; set; } = EnumHelper.GetEnumList<OverlayItemEffectEntranceAnimationTypeEnum>();
        public IEnumerable<OverlayItemEffectVisibleAnimationTypeEnum> VisibleAnimations { get; set; } = EnumHelper.GetEnumList<OverlayItemEffectVisibleAnimationTypeEnum>();
        public IEnumerable<OverlayItemEffectExitAnimationTypeEnum> ExitAnimations { get; set; } = EnumHelper.GetEnumList<OverlayItemEffectExitAnimationTypeEnum>();

        public virtual bool SupportsRefreshUpdating { get { return false; } }

        public virtual OverlayItemModelBase GetOverlayItem() { return null; }
    }
}

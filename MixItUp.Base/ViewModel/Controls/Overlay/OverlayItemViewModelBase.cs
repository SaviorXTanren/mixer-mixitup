using Mixer.Base.Util;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public abstract class OverlayItemViewModelBase : ControlViewModelBase
    {
        public IEnumerable<int> SampleFontSizes { get; } = new List<int>() { 12, 24, 36, 48, 60, 72, 84, 96, 108, 120 };
        public IEnumerable<string> SampleFontSizesStrings { get { return this.SampleFontSizes.Select(f => f.ToString()); } }

        public IEnumerable<string> ColorNames { get; set; } = ColorSchemes.HTMLColorSchemeDictionary.Keys;

        public IEnumerable<string> EntranceAnimationStrings { get; set; } = EnumHelper.GetEnumNames<OverlayItemEffectEntranceAnimationTypeEnum>();
        public IEnumerable<string> VisibleAnimationStrings { get; set; } = EnumHelper.GetEnumNames<OverlayItemEffectVisibleAnimationTypeEnum>();
        public IEnumerable<string> ExitAnimationStrings { get; set; } = EnumHelper.GetEnumNames<OverlayItemEffectExitAnimationTypeEnum>();

        public virtual bool SupportsRefreshUpdating { get { return false; } }

        public virtual OverlayItemModelBase GetOverlayItem() { return null; }
    }
}

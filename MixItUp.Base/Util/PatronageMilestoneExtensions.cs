using Mixer.Base.Model.Patronage;
using System;

namespace MixItUp.Base.Util
{
    public static class PatronageMilestoneExtensions
    {
        public static string PercentageAmountText(this PatronageMilestoneModel patronageMilestone)
        {
            return string.Format("{0}%", patronageMilestone.bonus);
        }
    }
}

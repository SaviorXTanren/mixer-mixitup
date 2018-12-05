using Mixer.Base.Model.Patronage;
using System;

namespace MixItUp.Base.Util
{
    public static class PatronageMilestoneExtensions
    {
        public static double DollarAmount(this PatronageMilestoneModel patronageMilestone)
        {
            return Math.Round(((double)patronageMilestone.reward) / 100.0, 2);
        }

        public static string DollarAmountText(this PatronageMilestoneModel patronageMilestone)
        {
            return string.Format("{0:C}", patronageMilestone.DollarAmount());
        }
    }
}

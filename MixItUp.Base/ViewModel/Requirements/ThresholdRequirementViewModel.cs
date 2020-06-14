using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirements
{
    public class ThresholdRequirementViewModel : RequirementViewModelBase
    {
        public int Amount
        {
            get { return this.amount; }
            set
            {
                if (this.amount >= 0)
                {
                    this.amount = value;
                }
                this.NotifyPropertyChanged();
            }
        }
        private int amount = 0;

        public int TimeSpan
        {
            get { return this.timeSpan; }
            set
            {
                if (this.timeSpan >= 0)
                {
                    this.timeSpan = value;
                }
                this.NotifyPropertyChanged();
            }
        }
        private int timeSpan = 0;

        public ThresholdRequirementViewModel() { }

        public ThresholdRequirementViewModel(ThresholdRequirementModel requirement)
        {
            this.Amount = requirement.Amount;
            this.TimeSpan = requirement.TimeSpan;
        }

        public override async Task<bool> Validate()
        {
            if (this.TimeSpan < 0)
            {
                await DialogHelper.ShowMessage(MixItUp.Base.Resources.ValidThresholdTimeSpanMustBeSpecified);
                return false;
            }

            if (this.Amount < 0 || (this.TimeSpan > 0 && this.Amount == 0))
            {
                await DialogHelper.ShowMessage(MixItUp.Base.Resources.ValidThresholdUserAmountMustBeSpecified);
                return false;
            }

            return true;
        }

        public override RequirementModelBase GetRequirement()
        {
            return new ThresholdRequirementModel(this.Amount, this.TimeSpan);
        }
    }
}

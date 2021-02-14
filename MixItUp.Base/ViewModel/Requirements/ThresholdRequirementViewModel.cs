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

        public bool RunForEachUser
        {
            get { return this.runForEachUser; }
            set
            {
                this.runForEachUser = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool runForEachUser = false;

        public ThresholdRequirementViewModel() { }

        public ThresholdRequirementViewModel(ThresholdRequirementModel requirement)
        {
            this.Amount = requirement.Amount;
            this.TimeSpan = requirement.TimeSpan;
            this.RunForEachUser = requirement.RunForEachUser;
        }

        public override Task<Result> Validate()
        {
            if (this.TimeSpan < 0)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ValidThresholdTimeSpanMustBeSpecified));
            }

            if (this.Amount < 0 || (this.TimeSpan > 0 && this.Amount == 0))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ValidThresholdUserAmountMustBeSpecified));
            }

            return Task.FromResult(new Result());
        }

        public override RequirementModelBase GetRequirement()
        {
            return new ThresholdRequirementModel(this.Amount, this.TimeSpan, this.RunForEachUser);
        }
    }
}

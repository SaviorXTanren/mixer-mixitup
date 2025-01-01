using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirements
{
    public class CooldownRequirementViewModel : RequirementViewModelBase
    {
        public IEnumerable<CooldownTypeEnum> Types { get { return EnumHelper.GetEnumList<CooldownTypeEnum>(); } }

        public CooldownTypeEnum SelectedType
        {
            get { return this.selectedType; }
            set
            {
                this.selectedType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsGroupSelected");

                this.SelectedGroupName = null;
            }
        }
        private CooldownTypeEnum selectedType = CooldownTypeEnum.Standard;

        public bool IsGroupSelected { get { return this.SelectedType == CooldownTypeEnum.Group; } }

        public IEnumerable<string> GroupNames { get { return ChannelSession.Settings.CooldownGroupAmounts.Keys.ToList(); } }

        public string SelectedGroupName
        {
            get { return this.selectedGroupName; }
            set
            {
                this.selectedGroupName = value;
                this.NotifyPropertyChanged();

                if (!string.IsNullOrEmpty(this.SelectedGroupName) && ChannelSession.Settings.CooldownGroupAmounts.ContainsKey(this.SelectedGroupName))
                {
                    this.Amount = ChannelSession.Settings.CooldownGroupAmounts[this.SelectedGroupName];
                }
            }
        }
        private string selectedGroupName;

        public int Amount
        {
            get { return this.amount; }
            set
            {
                this.amount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int amount;

        public CooldownRequirementViewModel() { }

        public CooldownRequirementViewModel(CooldownRequirementModel requirement)
        {
            this.SelectedType = requirement.Type;
            if (requirement.IsGroup)
            {
                this.SelectedGroupName = requirement.GroupName;
            }
            else
            {
                this.Amount = requirement.Amount;
            }
        }

        public override Task<Result> Validate()
        {
            if (this.Amount < 0)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ValidCooldownAmountMustBeSpecified));
            }

            if (this.SelectedType == CooldownTypeEnum.Group && string.IsNullOrEmpty(this.SelectedGroupName))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ValidCooldownGroupMustBeSpecified));
            }

            return Task.FromResult(new Result());
        }

        public override RequirementModelBase GetRequirement()
        {
            if (this.SelectedType == CooldownTypeEnum.Group)
            {
                ChannelSession.Settings.CooldownGroupAmounts[this.SelectedGroupName] = this.Amount;
            }
            return new CooldownRequirementModel(this.SelectedType, this.Amount, this.SelectedGroupName);
        }
    }
}

using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
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

                this.SelectedGroupName = null;
            }
        }
        private CooldownTypeEnum selectedType = CooldownTypeEnum.Individual;

        public IEnumerable<string> GroupNames { get { return ChannelSession.Settings.CooldownGroups.Keys.ToList(); } }

        public string SelectedGroupName
        {
            get { return this.selectedGroupName; }
            set
            {
                this.selectedGroupName = value;
                this.NotifyPropertyChanged();

                if (!string.IsNullOrEmpty(this.SelectedGroupName) && ChannelSession.Settings.CooldownGroups.ContainsKey(this.SelectedGroupName))
                {
                    this.Amount = ChannelSession.Settings.CooldownGroups[this.SelectedGroupName];
                }
            }
        }
        private string selectedGroupName;

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
                this.Amount = requirement.CooldownAmount;
            }
        }

        public override async Task<bool> Validate()
        {
            if (this.Amount < 0)
            {
                await DialogHelper.ShowMessage(MixItUp.Base.Resources.ValidCooldownAmountMustBeSpecified);
                return false;
            }

            if (this.SelectedType == CooldownTypeEnum.Group && string.IsNullOrEmpty(this.SelectedGroupName))
            {
                await DialogHelper.ShowMessage(MixItUp.Base.Resources.ValidCooldownGroupMustBeSpecified);
                return false;
            }

            return true;
        }

        public override RequirementModelBase GetRequirement()
        {
            if (this.SelectedType == CooldownTypeEnum.Group)
            {
                ChannelSession.Settings.CooldownGroups[this.SelectedGroupName] = this.Amount;
            }
            return new CooldownRequirementModel(this.SelectedType, this.Amount, this.SelectedGroupName);
        }
    }
}

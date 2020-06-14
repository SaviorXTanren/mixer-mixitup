using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirements
{
    public class RankRequirementViewModel : RequirementViewModelBase
    {
        public IEnumerable<CurrencyModel> RankSystems { get { return ChannelSession.Settings.Currency.Values.Where(r => r.IsRank); } }

        public CurrencyModel SelectedRankSystem
        {
            get { return this.selectedRankSystem; }
            set
            {
                this.selectedRankSystem = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("Ranks");
                this.NotifyPropertyChanged("SelectedRank");
            }
        }
        private CurrencyModel selectedRankSystem;

        public IEnumerable<string> MatchTypes { get { return EnumHelper.GetEnumNames<RankRequirementMatchTypeEnum>(); } }

        public RankRequirementMatchTypeEnum SelectedMatchType
        {
            get { return this.selectedMatchType; }
            set
            {
                this.selectedMatchType = value;
                this.NotifyPropertyChanged();
            }
        }
        private RankRequirementMatchTypeEnum selectedMatchType = RankRequirementMatchTypeEnum.GreaterThanOrEqualTo;

        public IEnumerable<RankModel> Ranks
        {
            get
            {
                List<RankModel> ranks = new List<RankModel>();
                if (this.SelectedRankSystem != null)
                {
                    ranks.AddRange(this.SelectedRankSystem.Ranks);
                }
                return ranks;
            }
        }

        public RankModel SelectedRank
        {
            get { return this.selectedRank; }
            set
            {
                this.selectedRank = value;
                this.NotifyPropertyChanged();
            }
        }
        private RankModel selectedRank;

        public RankRequirementViewModel() { }

        public RankRequirementViewModel(RankRequirementModel requirement)
        {
            this.SelectedRankSystem = requirement.RankSystem;
            this.SelectedRank = requirement.RequiredRank;
            this.SelectedMatchType = requirement.MatchType;
        }

        public override async Task<bool> Validate()
        {
            if (this.SelectedRankSystem == null)
            {
                await DialogHelper.ShowMessage(MixItUp.Base.Resources.ValidRankSystemMustBeSelected);
                return false;
            }

            if (this.SelectedRank == null)
            {
                await DialogHelper.ShowMessage(MixItUp.Base.Resources.ValidRankMustBeSelected);
                return false;
            }

            return true;
        }

        public override RequirementModelBase GetRequirement()
        {
            return new RankRequirementModel(this.SelectedRankSystem, this.SelectedRank, this.SelectedMatchType);
        }
    }
}

using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Requirements
{
    public enum RankRequirementMatchTypeEnum
    {
        GreaterThanOrEqualTo,
        EqualTo,
        LessThanOrEqualTo,
    }

    [DataContract]
    public class RankRequirementModel : RequirementModelBase
    {
        [DataMember]
        public Guid RankSystemID { get; set; }

        [DataMember]
        public string RankName { get; set; }
        [DataMember]
        public RankRequirementMatchTypeEnum MatchType { get; set; }

        public RankRequirementModel() { }

        public RankRequirementModel(CurrencyModel rankSystem, RankModel rank, RankRequirementMatchTypeEnum matchType = RankRequirementMatchTypeEnum.GreaterThanOrEqualTo)
        {
            this.RankSystemID = rankSystem.ID;
            this.RankName = rank.Name;
            this.MatchType = matchType;
        }

#pragma warning disable CS0612 // Type or member is obsolete
        internal RankRequirementModel(MixItUp.Base.ViewModel.Requirement.CurrencyRequirementViewModel requirement)
        {
            this.RankSystemID = requirement.CurrencyID;
            this.RankName = requirement.RankName;
            this.MatchType = (requirement.MustEqual) ? RankRequirementMatchTypeEnum.EqualTo : RankRequirementMatchTypeEnum.GreaterThanOrEqualTo;
        }
#pragma warning restore CS0612 // Type or member is obsolete

        [JsonIgnore]
        public CurrencyModel RankSystem
        {
            get
            {
                if (ChannelSession.Settings.Currency.ContainsKey(this.RankSystemID) && ChannelSession.Settings.Currency[this.RankSystemID].IsRank)
                {
                    return ChannelSession.Settings.Currency[this.RankSystemID];
                }
                return null;
            }
        }

        [JsonIgnore]
        public RankModel RequiredRank
        {
            get
            {
                CurrencyModel rankSystem = this.RankSystem;
                if (rankSystem != null)
                {
                    RankModel rank = rankSystem.Ranks.FirstOrDefault(r => r.Name.Equals(this.RankName));
                    if (rank != null)
                    {
                        return rank;
                    }
                }
                return CurrencyModel.NoRank;
            }
        }

        public override Task<Result> Validate(CommandParametersModel parameters)
        {
            CurrencyModel rankSystem = this.RankSystem;
            if (rankSystem == null)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.RankSystemDoesNotExist));
            }

            RankModel rank = this.RequiredRank;
            if (rank == null)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.RankDoesNotExist));
            }

            if (!parameters.User.Data.IsCurrencyRankExempt)
            {
                if (this.MatchType == RankRequirementMatchTypeEnum.GreaterThanOrEqualTo)
                {
                    if (!rankSystem.HasAmount(parameters.User.Data, rank.Amount))
                    {
                        return Task.FromResult(new Result(string.Format(MixItUp.Base.Resources.RankRequirementNotGreaterThanOrEqual, rank.Name, rank.Amount, rankSystem.Name)));
                    }
                }
                else if (this.MatchType == RankRequirementMatchTypeEnum.EqualTo)
                {
                    if (rankSystem.GetRank(parameters.User.Data) != rank)
                    {
                        return Task.FromResult(new Result(string.Format(MixItUp.Base.Resources.RankRequirementNotGreaterThanOrEqual, rank.Name, rank.Amount, rankSystem.Name)));
                    }
                }
                else if (this.MatchType == RankRequirementMatchTypeEnum.LessThanOrEqualTo)
                {
                    RankModel nextRank = rankSystem.GetNextRank(parameters.User.Data);
                    if (nextRank != CurrencyModel.NoRank && rankSystem.HasAmount(parameters.User.Data, nextRank.Amount))
                    {
                        return Task.FromResult(new Result(string.Format(MixItUp.Base.Resources.RankRequirementNotLessThan, rank.Name, rank.Amount, rankSystem.Name)));
                    }
                }
            }

            return Task.FromResult(new Result());
        }
    }
}

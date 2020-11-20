using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
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

        internal RankRequirementModel(MixItUp.Base.ViewModel.Requirement.CurrencyRequirementViewModel requirement)
        {
            this.RankSystemID = requirement.CurrencyID;
            this.RankName = requirement.RankName;
            this.MatchType = (requirement.MustEqual) ? RankRequirementMatchTypeEnum.EqualTo : RankRequirementMatchTypeEnum.GreaterThanOrEqualTo;
        }

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

        public override async Task<bool> Validate(CommandParametersModel parameters)
        {
            CurrencyModel rankSystem = this.RankSystem;
            if (rankSystem == null)
            {
                return false;
            }

            RankModel rank = this.RequiredRank;
            if (rank == null)
            {
                return false;
            }

            if (!parameters.User.Data.IsCurrencyRankExempt)
            {
                if (this.MatchType == RankRequirementMatchTypeEnum.GreaterThanOrEqualTo)
                {
                    if (!rankSystem.HasAmount(parameters.User.Data, rank.Amount))
                    {
                        await this.SendChatMessage(string.Format("You do not have the required rank of {0} ({1} {2}) to do this", rank.Name, rank.Amount, rankSystem.Name));
                        return false;
                    }
                }
                else if (this.MatchType == RankRequirementMatchTypeEnum.EqualTo)
                {
                    if (rankSystem.GetRank(parameters.User.Data) != rank)
                    {
                        await this.SendChatMessage(string.Format("You do not have the required rank of {0} to do this", rank.Name, rank.Amount, rankSystem.Name));
                        return false;
                    }
                }
                else if (this.MatchType == RankRequirementMatchTypeEnum.LessThanOrEqualTo)
                {
                    RankModel nextRank = rankSystem.GetNextRank(parameters.User.Data);
                    if (nextRank != CurrencyModel.NoRank && rankSystem.HasAmount(parameters.User.Data, nextRank.Amount))
                    {
                        await this.SendChatMessage(string.Format("You are over the required rank of {0} ({1} {2}) to do this", rank.Name, rank.Amount, rankSystem.Name));
                        return false;
                    }
                }
            }

            return true;
        }
    }
}

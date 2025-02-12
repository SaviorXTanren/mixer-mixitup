using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Commands
{
    [DataContract]
    public class TwitchBitsCommandModel : CommandModelBase
    {
        public static Dictionary<string, string> GetBitsTestSpecialIdentifiers()
        {
            return new Dictionary<string, string>()
            {
                { "bitsamount", "10" },
                { "messagenocheermotes", "Test Message" },
                { "message", "Test Message" },
                { "isanonymous", "false" }
            };
        }

        [DataMember]
        public int StartingAmount { get; set; }

        [DataMember]
        public int EndingAmount { get; set; }

        public TwitchBitsCommandModel(string name, int startingAmount, int endingAmount)
            : base(name, CommandTypeEnum.TwitchBits)
        {
            this.StartingAmount = startingAmount;
            this.EndingAmount = endingAmount;
        }

        [Obsolete]
        public TwitchBitsCommandModel() : base() { }

        public bool IsRange { get { return this.StartingAmount != this.EndingAmount; } }

        public bool IsSingle { get { return !this.IsRange; } }

        public int Range { get { return this.EndingAmount - this.StartingAmount; } }

        public string AmountDisplay
        {
            get
            {
                if (this.IsRange)
                {
                    return $"{this.StartingAmount} - {this.EndingAmount}";
                }
                else
                {
                    return this.StartingAmount.ToString();
                }
            }
        }

        public bool IsInRange(int amount) { return this.StartingAmount <= amount && amount <= this.EndingAmount; }

        public override Dictionary<string, string> GetTestSpecialIdentifiers() { return TwitchBitsCommandModel.GetBitsTestSpecialIdentifiers(); }
    }
}

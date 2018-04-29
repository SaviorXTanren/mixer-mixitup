using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum ConditionalComparisionTypeEnum
    {
        [Name("=")]
        Equals,
        [Name("<>")]
        NotEquals,
        [Name(">")]
        GreaterThan,
        [Name(">=")]
        GreaterThanOrEqual,
        [Name("<")]
        LessThan,
        [Name("<=")]
        LessThanOrEqual,
    }

    [DataContract]
    public class ConditionalAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return ConditionalAction.asyncSemaphore; } }

        [DataMember]
        public ConditionalComparisionTypeEnum ComparisionType { get; set; }

        [DataMember]
        public bool IgnoreCase { get; set; }

        [DataMember]
        public string Value1 { get; set; }
        [DataMember]
        public string Value2 { get; set; }

        [DataMember]
        public Guid CommandID { get; set; }

        public ConditionalAction() : base(ActionTypeEnum.Conditional) { }

        public ConditionalAction(ConditionalComparisionTypeEnum comparisionType, bool ignoreCase, string value1, string value2, CommandBase command)
            : this()
        {
            this.ComparisionType = comparisionType;
            this.IgnoreCase = ignoreCase;
            this.Value1 = value1;
            this.Value2 = value2;
            this.CommandID = command.ID;
        }

        public CommandBase GetCommand() { return ChannelSession.AllCommands.FirstOrDefault(c => c.ID.Equals(this.CommandID)); }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            string v1 = await this.ReplaceStringWithSpecialModifiers(this.Value1, user, arguments);
            string v2 = await this.ReplaceStringWithSpecialModifiers(this.Value2, user, arguments);

            int compareResult = 0;
            if (double.TryParse(v1, out double v1num) && double.TryParse(v2, out double v2num))
            {
                if (v1num < v2num)
                {
                    compareResult = -1;
                }
                else if (v1num > v2num)
                {
                    compareResult = 1;
                }
            }
            else
            {
                compareResult = string.Compare(v1, v2, this.IgnoreCase);
            }

            bool result = false;
            if (compareResult == 0 && (this.ComparisionType == ConditionalComparisionTypeEnum.Equals || this.ComparisionType == ConditionalComparisionTypeEnum.GreaterThanOrEqual ||
                this.ComparisionType == ConditionalComparisionTypeEnum.LessThanOrEqual))
            {
                result = true;
            }
            else if (compareResult < 0 && (this.ComparisionType == ConditionalComparisionTypeEnum.NotEquals || this.ComparisionType == ConditionalComparisionTypeEnum.LessThan ||
                this.ComparisionType == ConditionalComparisionTypeEnum.LessThanOrEqual))
            {
                result = true;
            }
            if (compareResult > 0 && (this.ComparisionType == ConditionalComparisionTypeEnum.NotEquals || this.ComparisionType == ConditionalComparisionTypeEnum.GreaterThan ||
                this.ComparisionType == ConditionalComparisionTypeEnum.GreaterThan))
            {
                result = true;
            }

            if (result)
            {
                CommandBase command = this.GetCommand();
                if (command != null)
                {
                    await command.Perform(user, arguments);
                }
            }
        }
    }
}

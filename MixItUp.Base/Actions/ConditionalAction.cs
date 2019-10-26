using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
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
        [Name("Contains")]
        Contains,
        [Name("<> Contain")]
        DoesNotContain,
        [Name("Between")]
        Between
    }

    public enum ConditionalOperatorTypeEnum
    {
        [Name("AND")]
        And,
        [Name("OR")]
        Or,
        [Name("XOR")]
        ExclusiveOr,
    }

    [DataContract]
    public class ConditionalClauseModel
    {
        [DataMember]
        public ConditionalComparisionTypeEnum ComparisionType { get; set; }

        [DataMember]
        public string Value1 { get; set; }
        [DataMember]
        public string Value2 { get; set; }
        [DataMember]
        public string Value3 { get; set; }

        public ConditionalClauseModel()
        {
            this.ComparisionType = ConditionalComparisionTypeEnum.Equals;
        }

        public ConditionalClauseModel(ConditionalComparisionTypeEnum comparisionType, string value1, string value2, string value3)
        {
            this.ComparisionType = comparisionType;
            this.Value1 = (value1 == null) ? string.Empty : value1;
            this.Value2 = (value2 == null) ? string.Empty : value2;
            this.Value3 = (value3 == null) ? string.Empty : value3;
        }
    }

    [DataContract]
    public class ConditionalAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return ConditionalAction.asyncSemaphore; } }

        [DataMember]
        public bool IgnoreCase { get; set; }

        [DataMember]
        public ConditionalOperatorTypeEnum Operator { get; set; }

        [Obsolete]
        [DataMember]
        public ConditionalComparisionTypeEnum ComparisionType { get; set; }

        [Obsolete]
        [DataMember]
        public string Value1 { get; set; }
        [Obsolete]
        [DataMember]
        public string Value2 { get; set; }
        [Obsolete]
        [DataMember]
        public string Value3 { get; set; }

        [DataMember]
        public List<ConditionalClauseModel> Clauses { get; set; } = new List<ConditionalClauseModel>();

        [DataMember]
        public Guid CommandID { get; set; }

        [DataMember]
        public ActionBase Action { get; set; }

        public ConditionalAction() : base(ActionTypeEnum.Conditional) { }

        public ConditionalAction(bool ignoreCase, ConditionalOperatorTypeEnum op, IEnumerable<ConditionalClauseModel> clauses, CommandBase command)
            : this(ignoreCase, op, clauses)
        {
            this.CommandID = command.ID;
        }

        public ConditionalAction(bool ignoreCase, ConditionalOperatorTypeEnum op, IEnumerable<ConditionalClauseModel> clauses, ActionBase action)
            : this(ignoreCase, op, clauses)
        {
            this.Action = action;
        }

        private ConditionalAction(bool ignoreCase, ConditionalOperatorTypeEnum op, IEnumerable<ConditionalClauseModel> clauses)
            : this()
        {
            this.IgnoreCase = ignoreCase;
            this.Operator = op;
            this.Clauses = new List<ConditionalClauseModel>(clauses);
        }

        public CommandBase GetCommand() { return ChannelSession.AllEnabledCommands.FirstOrDefault(c => c.ID.Equals(this.CommandID)); }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            if (this.Clauses.Count == 0 && !string.IsNullOrEmpty(this.Value1))
            {
                StoreCommandUpgrader.UpdateConditionalAction(new List<ActionBase>() { this });
            }
#pragma warning restore CS0612 // Type or member is obsolete

            List<bool> results = new List<bool>();
            foreach (ConditionalClauseModel clause in this.Clauses)
            {
                results.Add(await this.Check(clause, user, arguments));
            }

            bool finalResult = false;
            if (this.Operator == ConditionalOperatorTypeEnum.And)
            {
                finalResult = results.All(r => r);
            }
            else if (this.Operator == ConditionalOperatorTypeEnum.Or)
            {
                finalResult = results.Any(r => r);
            }
            else if (this.Operator == ConditionalOperatorTypeEnum.ExclusiveOr)
            {
                finalResult = results.Count(r => r) == 1;
            }

            if (finalResult)
            {
                if (this.CommandID != Guid.Empty)
                {
                    CommandBase command = this.GetCommand();
                    if (command != null)
                    {
                        await command.Perform(user, arguments, this.GetExtraSpecialIdentifiers());
                    }
                }
                else if (this.Action != null)
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    AsyncRunner.RunAsync(this.Action.Perform(user, arguments, this.GetExtraSpecialIdentifiers()));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
        }

        private async Task<bool> Check(ConditionalClauseModel clause, UserViewModel user, IEnumerable<string> arguments)
        {
            string v1 = await this.ReplaceStringWithSpecialModifiers(clause.Value1, user, arguments);
            string v2 = await this.ReplaceStringWithSpecialModifiers(clause.Value2, user, arguments);

            if (clause.ComparisionType == ConditionalComparisionTypeEnum.Contains || clause.ComparisionType == ConditionalComparisionTypeEnum.DoesNotContain)
            {
                bool contains = v1.IndexOf(v2, this.IgnoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture) >= 0;
                return ((clause.ComparisionType == ConditionalComparisionTypeEnum.Contains && contains) || (clause.ComparisionType == ConditionalComparisionTypeEnum.DoesNotContain && !contains));
            }
            else if (clause.ComparisionType == ConditionalComparisionTypeEnum.Between)
            {
                string v3 = await this.ReplaceStringWithSpecialModifiers(clause.Value3, user, arguments);
                if (double.TryParse(v1, out double v1num) && double.TryParse(v2, out double v2num) && double.TryParse(v3, out double v3num))
                {
                    return (v2num <= v1num && v1num <= v3num);
                }
            }
            else
            {
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

                if (compareResult == 0 && (clause.ComparisionType == ConditionalComparisionTypeEnum.Equals || clause.ComparisionType == ConditionalComparisionTypeEnum.GreaterThanOrEqual ||
                    clause.ComparisionType == ConditionalComparisionTypeEnum.LessThanOrEqual))
                {
                    return true;
                }
                else if (compareResult < 0 && (clause.ComparisionType == ConditionalComparisionTypeEnum.NotEquals || clause.ComparisionType == ConditionalComparisionTypeEnum.LessThan ||
                    clause.ComparisionType == ConditionalComparisionTypeEnum.LessThanOrEqual))
                {
                    return true;
                }
                else if (compareResult > 0 && (clause.ComparisionType == ConditionalComparisionTypeEnum.NotEquals || clause.ComparisionType == ConditionalComparisionTypeEnum.GreaterThan ||
                    clause.ComparisionType == ConditionalComparisionTypeEnum.GreaterThanOrEqual))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

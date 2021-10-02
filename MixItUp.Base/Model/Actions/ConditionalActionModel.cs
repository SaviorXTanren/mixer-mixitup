using MixItUp.Base.Model.Commands;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum ConditionalComparisionTypeEnum
    {
        [Name("EqualsCompare")]
        Equals,
        NotEquals,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        Contains,
        DoesNotContain,
        Between,
        Replaced,
        NotReplaced,
        RegexMatch,
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

        public ConditionalClauseModel() : this(ConditionalComparisionTypeEnum.Equals, null, null, null) { }

        public ConditionalClauseModel(ConditionalComparisionTypeEnum comparisionType, string value1, string value2, string value3)
        {
            this.ComparisionType = comparisionType;
            this.Value1 = (value1 == null) ? string.Empty : value1;
            this.Value2 = (value2 == null) ? string.Empty : value2;
            this.Value3 = (value3 == null) ? string.Empty : value3;
        }
    }

    [DataContract]
    public class ConditionalActionModel : ActionModelBase
    {
        [DataMember]
        public bool CaseSensitive { get; set; }
        [DataMember]
        public ConditionalOperatorTypeEnum Operator { get; set; }
        [DataMember]
        public bool RepeatWhileTrue { get; set; }

        [DataMember]
        public List<ConditionalClauseModel> Clauses { get; set; } = new List<ConditionalClauseModel>();

        [DataMember]
        public Guid CommandID { get; set; }

        [DataMember]
        public List<ActionModelBase> Actions { get; set; } = new List<ActionModelBase>();

        public ConditionalActionModel(bool caseSensitive, ConditionalOperatorTypeEnum op, bool repeatWhileTrue, IEnumerable<ConditionalClauseModel> clauses, IEnumerable<ActionModelBase> actions)
            : base(ActionTypeEnum.Conditional)
        {
            this.CaseSensitive = caseSensitive;
            this.Operator = op;
            this.RepeatWhileTrue = repeatWhileTrue;
            this.Clauses = new List<ConditionalClauseModel>(clauses);
            this.Actions = new List<ActionModelBase>(actions);
        }

#pragma warning disable CS0612 // Type or member is obsolete
        internal ConditionalActionModel(MixItUp.Base.Actions.ConditionalAction action, ActionModelBase subAction)
            : base(ActionTypeEnum.Conditional)
        {
            this.CaseSensitive = !action.IgnoreCase;
            this.Operator = (ConditionalOperatorTypeEnum)(int)action.Operator;

            foreach (var clause in action.Clauses)
            {
                this.Clauses.Add(new ConditionalClauseModel((ConditionalComparisionTypeEnum)(int)clause.ComparisionType, clause.Value1, clause.Value2, clause.Value3));
            }

            if (subAction != null)
            {
                this.Actions.Add(subAction);
            }
        }
#pragma warning restore CS0612 // Type or member is obsolete

        private ConditionalActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            bool finalResult = false;
            do
            {
                List<bool> results = new List<bool>();
                foreach (ConditionalClauseModel clause in this.Clauses)
                {
                    results.Add(await this.Check(clause, parameters));
                }

                finalResult = false;
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
                    await ChannelSession.Services.Command.RunDirectly(new CommandInstanceModel(this.Actions, parameters));
                }
            } while (this.RepeatWhileTrue && finalResult);
        }

        private async Task<bool> Check(ConditionalClauseModel clause, CommandParametersModel parameters)
        {
            string v1 = await ReplaceStringWithSpecialModifiers(clause.Value1, parameters);
            string v2 = await ReplaceStringWithSpecialModifiers(clause.Value2, parameters);

            if (clause.ComparisionType == ConditionalComparisionTypeEnum.Contains || clause.ComparisionType == ConditionalComparisionTypeEnum.DoesNotContain)
            {
                bool contains = v1.IndexOf(v2, this.CaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase) >= 0;
                return ((clause.ComparisionType == ConditionalComparisionTypeEnum.Contains && contains) || (clause.ComparisionType == ConditionalComparisionTypeEnum.DoesNotContain && !contains));
            }
            else if (clause.ComparisionType == ConditionalComparisionTypeEnum.Between)
            {
                string v3 = await ReplaceStringWithSpecialModifiers(clause.Value3, parameters);
                if (double.TryParse(v1, out double v1num) && double.TryParse(v2, out double v2num) && double.TryParse(v3, out double v3num))
                {
                    return (v2num <= v1num && v1num <= v3num);
                }
            }
            else if (clause.ComparisionType == ConditionalComparisionTypeEnum.Replaced)
            {
                return !clause.Value1.Equals(v1);
            }
            else if (clause.ComparisionType == ConditionalComparisionTypeEnum.NotReplaced)
            {
                return clause.Value1.Equals(v1);
            }
            else if (clause.ComparisionType == ConditionalComparisionTypeEnum.RegexMatch)
            {
                return Regex.IsMatch(v1, v2, this.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
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
                    compareResult = string.Compare(v1, v2, !this.CaseSensitive);
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

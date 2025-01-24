using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
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
        IsIn,
        IsNotIn,
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
    public class ConditionalActionModel : GroupActionModel
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

        public ConditionalActionModel(bool caseSensitive, ConditionalOperatorTypeEnum op, bool repeatWhileTrue, IEnumerable<ConditionalClauseModel> clauses, IEnumerable<ActionModelBase> actions)
            : base(ActionTypeEnum.Conditional, actions)
        {
            this.CaseSensitive = caseSensitive;
            this.Operator = op;
            this.RepeatWhileTrue = repeatWhileTrue;
            this.Clauses = new List<ConditionalClauseModel>(clauses);
        }

        [Obsolete]
        public ConditionalActionModel() : base() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            bool finalResult = false;
            int totalLoops = 0;
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
                    await this.RunSubActions(parameters);
                }

                totalLoops++;
                if (totalLoops == 10)
                {
                    Logger.Log(LogLevel.Error, $"Command: {parameters.InitialCommandID} - Conditional Action - Repeated 10 times, possible endless loop");
                }
            } while (this.RepeatWhileTrue && finalResult);
        }

        private async Task<bool> Check(ConditionalClauseModel clause, CommandParametersModel parameters)
        {
            string v1 = await ReplaceStringWithSpecialModifiers(clause.Value1, parameters);
            string v2 = await ReplaceStringWithSpecialModifiers(clause.Value2, parameters);

            Logger.Log(LogLevel.Debug, $"Conditional Action: Checking clause - {v1} {clause.ComparisionType} {v2}");

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
            else if (clause.ComparisionType == ConditionalComparisionTypeEnum.IsIn || clause.ComparisionType == ConditionalComparisionTypeEnum.IsNotIn)
            {
                string[] splits = v2.Split(new string[] { ChannelSession.Settings.DelimitedArgumentsSeparator }, StringSplitOptions.RemoveEmptyEntries);
                if (splits != null && splits.Length > 0)
                {
                    bool found = false;
                    foreach (string s in splits)
                    {
                        if (string.Equals(v1, s, this.CaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase))
                        {
                            found = true;
                            break;
                        }
                    }

                    if ((found && clause.ComparisionType == ConditionalComparisionTypeEnum.IsIn) ||
                        (!found && clause.ComparisionType == ConditionalComparisionTypeEnum.IsNotIn))
                    {
                        return true;
                    }
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

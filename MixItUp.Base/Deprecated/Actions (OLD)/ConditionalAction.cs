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
    [Obsolete]
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
        NotReplaced
    }

    [Obsolete]
    public enum ConditionalOperatorTypeEnum
    {
        [Name("AND")]
        And,
        [Name("OR")]
        Or,
        [Name("XOR")]
        ExclusiveOr,
    }

    [Obsolete]
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

    [Obsolete]
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

        public CommandBase GetCommand() { return null; }

        protected override Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            return Task.FromResult(0);
        }
    }
}

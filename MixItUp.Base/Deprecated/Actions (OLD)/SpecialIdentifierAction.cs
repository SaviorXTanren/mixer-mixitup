using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [Obsolete]
    [DataContract]
    public class SpecialIdentifierAction : ActionBase
    {
        private const string TextProcessorFunctionRegexPatternFormat = "{0}\\([^)]+\\)";

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return SpecialIdentifierAction.asyncSemaphore; } }

        [DataMember]
        public string SpecialIdentifierName { get; set; }

        [DataMember]
        public string SpecialIdentifierReplacement { get; set; }

        [DataMember]
        public bool MakeGloballyUsable { get; set; }

        [DataMember]
        public bool SpecialIdentifierShouldProcessMath { get; set; }

        public SpecialIdentifierAction()
            : base(ActionTypeEnum.SpecialIdentifier)
        {
            this.MakeGloballyUsable = true;
        }

        public SpecialIdentifierAction(string specialIdentifierName, string specialIdentifierReplacement, bool makeGloballyUsable, bool specialIdentifierShouldProcessMath)
            : this()
        {
            this.SpecialIdentifierName = specialIdentifierName;
            this.SpecialIdentifierReplacement = specialIdentifierReplacement;
            this.MakeGloballyUsable = makeGloballyUsable;
            this.SpecialIdentifierShouldProcessMath = specialIdentifierShouldProcessMath;
        }

        protected override Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            return Task.CompletedTask;
        }
    }
}

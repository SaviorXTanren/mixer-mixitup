using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class SpecialIdentifierAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return SpecialIdentifierAction.asyncSemaphore; } }

        [DataMember]
        public string SpecialIdentifierName { get; set; }

        [DataMember]
        public string SpecialIdentifierReplacement { get; set; }

        public SpecialIdentifierAction() : base(ActionTypeEnum.SpecialIdentifier) { }

        public SpecialIdentifierAction(string specialIdentifierName, string specialIdentifierReplacement)
            : this()
        {
            this.SpecialIdentifierName = specialIdentifierName;
            this.SpecialIdentifierReplacement = specialIdentifierReplacement;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            string replacementText = await this.ReplaceStringWithSpecialModifiers(this.SpecialIdentifierReplacement, user, arguments);
            SpecialIdentifierStringBuilder.AddCustomSpecialIdentifier(this.SpecialIdentifierName, replacementText);
        }
    }
}

using Jace;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
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

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            string replacementText = await this.ReplaceStringWithSpecialModifiers(this.SpecialIdentifierReplacement, user, arguments);

            if (this.SpecialIdentifierShouldProcessMath)
            {
                try
                {
                    // Process Math
                    CalculationEngine engine = new CalculationEngine();
                    double result = engine.Calculate(replacementText);
                    replacementText = result.ToString();
                }
                catch (Exception ex)
                {
                    // Calculation failed, log and set to 0
                    Logger.Log(ex, false, false);
                    replacementText = "0";
                }
            }

            if (this.MakeGloballyUsable)
            {
                SpecialIdentifierStringBuilder.AddCustomSpecialIdentifier(this.SpecialIdentifierName, replacementText);
            }
            else
            {
                this.extraSpecialIdentifiers[this.SpecialIdentifierName] = replacementText;
            }
        }
    }
}

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
        public bool SpecialIdentifierShouldProcessMath { get; set; }

        public SpecialIdentifierAction() : base(ActionTypeEnum.SpecialIdentifier) { }

        public SpecialIdentifierAction(string specialIdentifierName, string specialIdentifierReplacement, bool specialIdentifierShouldProcessMath)
            : this()
        {
            this.SpecialIdentifierName = specialIdentifierName;
            this.SpecialIdentifierReplacement = specialIdentifierReplacement;
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

            SpecialIdentifierStringBuilder.AddCustomSpecialIdentifier(this.SpecialIdentifierName, replacementText);
        }
    }
}

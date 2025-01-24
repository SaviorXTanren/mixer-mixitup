using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class WaitActionModel : ActionModelBase
    {
        [DataMember]
        public string Amount { get; set; }

        public WaitActionModel(string amount)
            : base(ActionTypeEnum.Wait)
        {
            this.Amount = amount;
        }

        [Obsolete]
        public WaitActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            string amountText = await ReplaceStringWithSpecialModifiers(this.Amount, parameters);
            double amount = MathHelper.ProcessMathEquation(amountText);

            if (amount > 0.0)
            {
                if (amount > 30)
                {
                    Logger.Log(LogLevel.Error, $"Command: {parameters.InitialCommandID} - Wait Action - Wait for longer than 30 seconds detected.");
                }

                await Task.Delay((int)(1000 * amount));
            }
        }
    }
}

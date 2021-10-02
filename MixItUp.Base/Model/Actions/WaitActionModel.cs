using MixItUp.Base.Model.Commands;
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

#pragma warning disable CS0612 // Type or member is obsolete
        internal WaitActionModel(MixItUp.Base.Actions.WaitAction action)
            : base(ActionTypeEnum.Wait)
        {
            this.Amount = action.Amount;
        }
#pragma warning restore CS0612 // Type or member is obsolete

        private WaitActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            string amountText = await ReplaceStringWithSpecialModifiers(this.Amount, parameters);
            if (double.TryParse(amountText, out double amount) && amount > 0.0)
            {
                await Task.Delay((int)(1000 * amount));
            }
        }
    }
}

using MixItUp.Base.Model.Commands;
using System.Globalization;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class TranslationActionModel : ActionModelBase
    {
        [DataMember]
        public CultureInfo Culture { get; set; }

        [DataMember]
        public bool AllowProfanity { get; set; }

        [DataMember]
        public string Text { get; set; }

        private TranslationActionModel() { }

        protected override Task PerformInternal(CommandParametersModel parameters)
        {
            return Task.FromResult(0);
        }
    }
}

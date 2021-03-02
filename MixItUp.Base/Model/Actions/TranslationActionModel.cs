using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using System.Globalization;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class TranslationActionModel : ActionModelBase
    {
        public const string ResponseSpecialIdentifier = "translationresult";

        [DataMember]
        public CultureInfo Culture { get; set; }

        [DataMember]
        public bool AllowProfanity { get; set; }

        [DataMember]
        public string Text { get; set; }

        public TranslationActionModel(CultureInfo culture, bool allowProfanity, string text)
            : base(ActionTypeEnum.Translation)
        {
            this.Culture = culture;
            this.AllowProfanity = allowProfanity;
            this.Text = text;
        }

#pragma warning disable CS0612 // Type or member is obsolete
        internal TranslationActionModel(MixItUp.Base.Actions.TranslationAction action)
            : base(ActionTypeEnum.Translation)
        {
            this.Culture = action.Culture;
            this.AllowProfanity = action.AllowProfanity;
            this.Text = action.Text;
        }
#pragma warning restore CS0612 // Type or member is obsolete

        private TranslationActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ServiceManager.Get<TranslationService>() != null)
            {
                string text = await this.ReplaceStringWithSpecialModifiers(this.Text, parameters);
                string translationResult = await ServiceManager.Get<TranslationService>().Translate(this.Culture, text, this.AllowProfanity);
                if (string.IsNullOrEmpty(translationResult))
                {
                    translationResult = this.Text;
                }

                if (!string.IsNullOrEmpty(translationResult) && string.IsNullOrEmpty(await ServiceManager.Get<ModerationService>().ShouldTextBeModerated(parameters.User, translationResult)))
                {
                    parameters.SpecialIdentifiers[ResponseSpecialIdentifier] = translationResult;
                }
            }
        }
    }
}

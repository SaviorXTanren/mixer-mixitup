using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class TranslationActionModel : ActionModelBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return TranslationActionModel.asyncSemaphore; } }

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

        internal TranslationActionModel(MixItUp.Base.Actions.TranslationAction action)
            : base(ActionTypeEnum.Translation)
        {
            this.Culture = action.Culture;
            this.AllowProfanity = action.AllowProfanity;
            this.Text = action.Text;
        }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ChannelSession.Services.Translation != null)
            {
                string text = await this.ReplaceStringWithSpecialModifiers(this.Text, parameters);
                string translationResult = await ChannelSession.Services.Translation.Translate(this.Culture, text, this.AllowProfanity);
                if (string.IsNullOrEmpty(translationResult))
                {
                    translationResult = this.Text;
                }

                if (!string.IsNullOrEmpty(translationResult) && string.IsNullOrEmpty(await ChannelSession.Services.Moderation.ShouldTextBeModerated(parameters.User, translationResult)))
                {
                    parameters.SpecialIdentifiers[ResponseSpecialIdentifier] = translationResult;
                }
            }
        }
    }
}

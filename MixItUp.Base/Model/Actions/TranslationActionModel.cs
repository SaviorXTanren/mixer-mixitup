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
        public string Text { get; set; }

        [DataMember]
        public bool AllowProfanity { get; set; }

        public TranslationActionModel(CultureInfo culture, string text, bool allowProfanity)
            : base(ActionTypeEnum.Translation)
        {
            this.Culture = culture;
            this.Text = text;
            this.AllowProfanity = allowProfanity;
        }

        protected override async Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            if (ChannelSession.Services.TranslationService != null)
            {
                string text = await this.ReplaceStringWithSpecialModifiers(this.Text, user, platform, arguments, specialIdentifiers);
                string translationResult = await ChannelSession.Services.TranslationService.Translate(this.Culture, text, this.AllowProfanity);
                if (string.IsNullOrEmpty(translationResult))
                {
                    translationResult = this.Text;
                }

                if (!string.IsNullOrEmpty(translationResult) && string.IsNullOrEmpty(await ChannelSession.Services.Moderation.ShouldTextBeModerated(user, translationResult)))
                {
                    specialIdentifiers[ResponseSpecialIdentifier] = translationResult;
                }
            }
        }
    }
}

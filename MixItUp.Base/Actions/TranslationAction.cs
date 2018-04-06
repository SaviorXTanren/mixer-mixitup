using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class TranslationAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return TranslationAction.asyncSemaphore; } }

        [DataMember]
        public CultureInfo Culture { get; set; }

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public bool AllowProfanity { get; set; }

        //public TranslationAction() : base(ActionTypeEnum.Translation) { }

        //public TranslationAction(CultureInfo culture, string text, bool allowProfanity)
        //    : this()
        //{
        //    this.Culture = culture;
        //    this.Text = text;
        //    this.AllowProfanity = allowProfanity;
        //}

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Services.TranslationService != null)
            {
                string text = await this.ReplaceStringWithSpecialModifiers(this.Text, user, arguments);
                string translation = await ChannelSession.Services.TranslationService.Translate(this.Culture, text, this.AllowProfanity);
                if (!string.IsNullOrEmpty(translation))
                {

                }
            }
        }
    }
}

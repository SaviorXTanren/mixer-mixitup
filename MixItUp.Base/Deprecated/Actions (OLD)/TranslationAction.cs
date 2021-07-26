using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [Obsolete]
    public enum TranslationResponseActionTypeEnum
    {
        Chat,
        Command,
        SpecialIdentifier
    }

    [Obsolete]
    [DataContract]
    public class TranslationAction : ActionBase
    {
        public static TranslationAction CreateForChat(CultureInfo culture, string text, bool allowProfanity, string chatText)
        {
            TranslationAction action = new TranslationAction(culture, text, allowProfanity, TranslationResponseActionTypeEnum.Chat);
            action.ResponseChatText = chatText;
            return action;
        }

        public static TranslationAction CreateForCommand(CultureInfo culture, string text, bool allowProfanity, CommandBase command, string arguments)
        {
            TranslationAction action = new TranslationAction(culture, text, allowProfanity, TranslationResponseActionTypeEnum.Command);
            action.ResponseCommandID = command.ID;
            action.ResponseCommandArgumentsText = arguments;
            return action;
        }

        public static TranslationAction CreateForSpecialIdentifier(CultureInfo culture, string text, bool allowProfanity, string specialIdentifierName)
        {
            TranslationAction action = new TranslationAction(culture, text, allowProfanity, TranslationResponseActionTypeEnum.SpecialIdentifier);
            action.SpecialIdentifierName = specialIdentifierName;
            return action;
        }

        public const string ResponseSpecialIdentifier = "translationresult";

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return TranslationAction.asyncSemaphore; } }

        [DataMember]
        public CultureInfo Culture { get; set; }

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public bool AllowProfanity { get; set; }

        [DataMember]
        public TranslationResponseActionTypeEnum ResponseAction { get; set; }

        [DataMember]
        public string ResponseChatText { get; set; }

        [DataMember]
        public Guid ResponseCommandID { get; set; }
        [DataMember]
        public string ResponseCommandArgumentsText { get; set; }

        [DataMember]
        public string SpecialIdentifierName { get; set; }

        public TranslationAction() : base(ActionTypeEnum.Translation) { }

        public TranslationAction(CultureInfo culture, string text, bool allowProfanity, TranslationResponseActionTypeEnum responseAction)
            : this()
        {
            this.Culture = culture;
            this.Text = text;
            this.AllowProfanity = allowProfanity;
            this.ResponseAction = responseAction;
        }

        protected override Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            return Task.CompletedTask;
        }
    }
}

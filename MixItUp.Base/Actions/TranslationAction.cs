using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum TranslationResponseActionTypeEnum
    {
        Chat,
        Command,
        [Name("Special Identifier")]
        SpecialIdentifier
    }

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

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Services.TranslationService != null)
            {
                string text = await this.ReplaceStringWithSpecialModifiers(this.Text, user, arguments);
                string translationResult = await ChannelSession.Services.TranslationService.Translate(this.Culture, text, this.AllowProfanity);
                if (string.IsNullOrEmpty(translationResult))
                {
                    translationResult = this.Text;
                }

                if (!string.IsNullOrEmpty(translationResult))
                {
                    if (this.ResponseAction == TranslationResponseActionTypeEnum.Chat)
                    {
                        if (ChannelSession.Chat != null)
                        {
                            await ChannelSession.Chat.SendMessage(await this.ReplaceSpecialIdentifiers(this.ResponseChatText, user, arguments, translationResult));
                        }
                    }
                    else if (this.ResponseAction == TranslationResponseActionTypeEnum.Command)
                    {
                        CommandBase command = ChannelSession.AllCommands.FirstOrDefault(c => c.ID.Equals(this.ResponseCommandID));
                        if (command != null)
                        {
                            string argumentsText = (this.ResponseCommandArgumentsText != null) ? this.ResponseCommandArgumentsText : string.Empty;
                            string commandArguments = await this.ReplaceSpecialIdentifiers(this.ResponseChatText, user, arguments, translationResult);

                            command.AddSpecialIdentifiers(this.GetAdditiveSpecialIdentifiers());
                            await command.Perform(user, commandArguments.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
                        }
                    }
                    else if (this.ResponseAction == TranslationResponseActionTypeEnum.SpecialIdentifier)
                    {
                        string replacementText = await this.ReplaceStringWithSpecialModifiers(translationResult, user, arguments);
                        SpecialIdentifierStringBuilder.AddCustomSpecialIdentifier(this.SpecialIdentifierName, replacementText);
                    }
                }
            }
        }

        private async Task<string> ReplaceSpecialIdentifiers(string text, UserViewModel user, IEnumerable<string> arguments, string translationResult)
        {
            this.AddSpecialIdentifier(WebRequestAction.ResponseSpecialIdentifier, translationResult);
            return await this.ReplaceStringWithSpecialModifiers(text, user, arguments);
        }
    }
}

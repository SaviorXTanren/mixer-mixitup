using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum WebRequestResponseActionTypeEnum
    {
        None,
        Chat,
        Command,
        [Name("Special Identifier")]
        SpecialIdentifier
    }

    [DataContract]
    public class WebRequestAction : ActionBase
    {
        public static WebRequestAction CreateForChat(string url, string chatText)
        {
            WebRequestAction action = new WebRequestAction(url, WebRequestResponseActionTypeEnum.Chat);
            action.ResponseChatText = chatText;
            return action;
        }

        public static WebRequestAction CreateForCommand(string url, CommandBase command, string arguments)
        {
            WebRequestAction action = new WebRequestAction(url, WebRequestResponseActionTypeEnum.Command);
            action.ResponseCommandID = command.ID;
            action.ResponseCommandArgumentsText = arguments;
            return action;
        }

        public static WebRequestAction CreateForSpecialIdentifier(string url, string specialIdentifierName)
        {
            WebRequestAction action = new WebRequestAction(url, WebRequestResponseActionTypeEnum.SpecialIdentifier);
            action.SpecialIdentifierName = specialIdentifierName;
            return action;
        }

        public const string ResponseSpecialIdentifier = "webrequestresult";

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return WebRequestAction.asyncSemaphore; } }

        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public WebRequestResponseActionTypeEnum ResponseAction { get; set; }

        [DataMember]
        public string ResponseChatText { get; set; }

        [DataMember]
        public Guid ResponseCommandID { get; set; }
        [DataMember]
        public string ResponseCommandArgumentsText { get; set; }

        [DataMember]
        public string SpecialIdentifierName { get; set; }

        [DataMember]
        [Obsolete]
        public string ResponseCommandName { get; set; }

        public WebRequestAction() : base(ActionTypeEnum.WebRequest) { }

        public WebRequestAction(string url, WebRequestResponseActionTypeEnum responseAction)
            : this()
        {
            this.Url = url;
            this.ResponseAction = responseAction;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                using (HttpResponseMessage response = await httpClient.GetAsync(await this.ReplaceStringWithSpecialModifiers(this.Url, user, arguments)))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string webRequestResult = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(webRequestResult))
                        {
                            if (this.ResponseAction == WebRequestResponseActionTypeEnum.Chat)
                            {
                                if (ChannelSession.Chat != null)
                                {
                                    await ChannelSession.Chat.SendMessage(await this.ReplaceSpecialIdentifiers(this.ResponseChatText, user, arguments, webRequestResult));
                                }
                            }
                            else if (this.ResponseAction == WebRequestResponseActionTypeEnum.Command)
                            {
                                CommandBase command = ChannelSession.AllCommands.FirstOrDefault(c => c.ID.Equals(this.ResponseCommandID));

#pragma warning disable CS0612 // Type or member is obsolete
                                if (command == null && !string.IsNullOrEmpty(this.ResponseCommandName))
                                {
                                    command = ChannelSession.Settings.ChatCommands.FirstOrDefault(c => c.Name.Equals(this.ResponseCommandName));
                                    if (command != null)
                                    {
                                        this.ResponseCommandID = command.ID;
                                    }
                                }
                                this.ResponseCommandName = null;
#pragma warning restore CS0612 // Type or member is obsolete

                                if (command != null)
                                {
                                    string argumentsText = (this.ResponseCommandArgumentsText != null) ? this.ResponseCommandArgumentsText : string.Empty;
                                    SpecialIdentifierStringBuilder siString = new SpecialIdentifierStringBuilder(argumentsText);
                                    siString.ReplaceSpecialIdentifier(WebRequestAction.ResponseSpecialIdentifier, webRequestResult);
                                    await command.Perform(user, siString.ToString().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
                                }
                            }
                            else if (this.ResponseAction == WebRequestResponseActionTypeEnum.SpecialIdentifier)
                            {
                                string replacementText = await this.ReplaceStringWithSpecialModifiers(webRequestResult, user, arguments);
                                SpecialIdentifierStringBuilder.AddCustomSpecialIdentifier(this.SpecialIdentifierName, replacementText);
                            }
                        }
                    }
                }
            }
        }

        private async Task<string> ReplaceSpecialIdentifiers(string text, UserViewModel user, IEnumerable<string> arguments, string webRequestResult)
        {
            SpecialIdentifierStringBuilder siString = new SpecialIdentifierStringBuilder(this.ResponseChatText);
            await siString.ReplaceCommonSpecialModifiers(user, arguments);
            siString.ReplaceSpecialIdentifier(WebRequestAction.ResponseSpecialIdentifier, webRequestResult);
            return siString.ToString();
        }
    }
}

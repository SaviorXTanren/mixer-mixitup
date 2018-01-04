using MixItUp.Base.Commands;
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
    }

    [DataContract]
    public class WebRequestAction : ActionBase
    {
        private const string ResponseSpecialIdentifier = "$webrequestresult";

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return WebRequestAction.asyncSemaphore; } }

        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public WebRequestResponseActionTypeEnum ResponseAction { get; set; }

        [DataMember]
        public string ResponseChatText { get; set; }

        [DataMember]
        public string ResponseCommandName { get; set; }
        [DataMember]
        public string ResponseCommandArgumentsText { get; set; }

        public WebRequestAction() : base(ActionTypeEnum.WebRequest) { }

        public WebRequestAction(string url, WebRequestResponseActionTypeEnum responseAction = WebRequestResponseActionTypeEnum.None)
            : this()
        {
            this.Url = url;
            this.ResponseAction = responseAction;
        }

        public WebRequestAction(string url, string chatText)
            : this(url, WebRequestResponseActionTypeEnum.Chat)
        {
            this.ResponseChatText = chatText;
        }

        public WebRequestAction(string url, string commandName, string arguments)
            : this(url, WebRequestResponseActionTypeEnum.Command)
        {
            this.ResponseCommandName = commandName;
            this.ResponseCommandArgumentsText = arguments;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                using (HttpResponseMessage response = await httpClient.GetAsync(await this.ReplaceStringWithSpecialModifiers(this.Url, user, arguments)))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string resultString = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(resultString))
                        {
                            if (this.ResponseAction == WebRequestResponseActionTypeEnum.Chat)
                            {
                                if (ChannelSession.Chat != null)
                                {
                                    string chatText = await this.ReplaceStringWithSpecialModifiers(this.ResponseChatText, user, arguments);
                                    chatText = chatText.Replace(ResponseSpecialIdentifier, resultString);
                                    await ChannelSession.Chat.SendMessage(chatText);
                                }
                            }
                            else if (this.ResponseAction == WebRequestResponseActionTypeEnum.Command)
                            {
                                ChatCommand command = ChannelSession.Settings.ChatCommands.FirstOrDefault(c => c.Name.Equals(this.ResponseCommandName));
                                if (command != null)
                                {
                                    string argumentsText = (this.ResponseCommandArgumentsText != null) ? this.ResponseCommandArgumentsText : string.Empty;
                                    await command.Perform(user, argumentsText.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

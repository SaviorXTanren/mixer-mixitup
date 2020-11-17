using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MixItUp.Base.Actions
{
    public enum WebRequestResponseActionTypeEnum
    {
        None,
        Chat,
        Command,
        [Name("Special Identifier")]
        SpecialIdentifier,
        [Name("JSON to Special Identifers")]
        JSONToSpecialIdentifiers
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

        public static WebRequestAction CreateForJSONToSpecialIdentifiers(string url, Dictionary<string, string> jsonToSpecialIdentifiers)
        {
            WebRequestAction action = new WebRequestAction(url, WebRequestResponseActionTypeEnum.JSONToSpecialIdentifiers);
            action.JSONToSpecialIdentifiers = jsonToSpecialIdentifiers;
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
        public Dictionary<string, string> JSONToSpecialIdentifiers { get; set; }

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

        protected override Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            return Task.FromResult(0);
        }
    }
}

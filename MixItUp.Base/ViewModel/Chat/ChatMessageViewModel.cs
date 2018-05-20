using Mixer.Base.Model.Chat;
using MixItUp.Base.Themes;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Chat
{
    public class EmoticonImage
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public CoordinatesModel Coordinates { get; set; }
    }

    public class ChatMessageViewModel : IEquatable<ChatMessageViewModel>
    {
        private const string DefaultEmoticonsLinkFormat = "https://mixer.com/_latest/assets/emoticons/{0}.png";

        public static Dictionary<string, EmoticonImage> EmoticonImages = new Dictionary<string, EmoticonImage>();

        public Guid ID { get; private set; }

        public UserViewModel User { get; private set; }

        public string Message { get; private set; }

        public string TargetUsername { get; private set; }

        public DateTimeOffset Timestamp { get; private set; }

        public bool ContainsLink { get; private set; }

        public bool IsInUsersChannel { get; private set; }

        public bool IsDeleted { get; set; }

        public ChatMessageEventModel ChatMessageEvent { get; private set; }

        public List<ChatMessageDataModel> MessageComponents = new List<ChatMessageDataModel>();

        public ChatMessageViewModel(ChatMessageEventModel chatMessageEvent, UserViewModel user = null)
        {
            this.ChatMessageEvent = chatMessageEvent;
            this.ID = this.ChatMessageEvent.id;

            this.User = (user != null) ? user : new UserViewModel(this.ChatMessageEvent);
            this.IsInUsersChannel = ChannelSession.Channel.id.Equals(this.ChatMessageEvent.channel);
            
            this.TargetUsername = this.ChatMessageEvent.target;
            this.Timestamp = DateTimeOffset.Now;
            this.Message = string.Empty;
            foreach (ChatMessageDataModel message in this.ChatMessageEvent.message.message)
            {
                this.MessageComponents.Add(message);
                switch (message.type)
                {
                    case "emoticon":
                        // Special code here to process emoticons
                        string imageLink = null;
                        if (message.source.Equals("external") && Uri.IsWellFormedUriString(message.pack, UriKind.Absolute))
                        {
                            imageLink = message.pack;
                        }
                        else if (message.source.Equals("builtin"))
                        {
                            imageLink = string.Format(ChatMessageViewModel.DefaultEmoticonsLinkFormat, message.pack);
                        }

                        if (!string.IsNullOrEmpty(imageLink))
                        {
                            if (!ChatMessageViewModel.EmoticonImages.ContainsKey(message.text))
                            {
                                try
                                {
                                    string imageFilePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(imageLink));
                                    if (!File.Exists(imageFilePath))
                                    {
                                        using (WebClient client = new WebClient())
                                        {
                                            client.DownloadFile(new Uri(imageLink), imageFilePath);
                                        }
                                    }
                                    ChatMessageViewModel.EmoticonImages[message.text] = new EmoticonImage() { Name = message.text, FilePath = imageFilePath, Coordinates = message.coords };
                                }
                                catch (Exception ex) { Logger.Log(ex); }
                            }
                        }

                        this.Message += message.text;
                        break;
                    case "link":
                        this.ContainsLink = true;
                        this.Message += message.text;
                        break;
                    case "text":
                    case "tag":
                    default:
                        this.Message += message.text;
                        break;
                }
            }
        }

        public ChatMessageViewModel(string alertText, UserViewModel user = null, string foregroundBrush = null)
        {
            this.ID = Guid.Empty;
            this.User = user;
            this.Timestamp = DateTimeOffset.Now;
            this.Message = "---  " + alertText + "  ---";
            this.AlertMessageBrush = ColorSchemes.GetColorCode(foregroundBrush);
            this.MessageComponents.Add(new ChatMessageDataModel() { type = "text", text = this.Message });
        }

        public bool IsAlertMessage { get { return this.ID == Guid.Empty; } }

        public string AlertMessageBrush { get; private set; }

        public bool IsWhisper { get { return !string.IsNullOrEmpty(this.TargetUsername); } }

        public bool IsUserTagged { get { return this.Message.Contains("@" + ChannelSession.User.username); } }

        public string CommandName { get { return this.CommandPieces.First().ToLower(); } }

        public IEnumerable<string> CommandArguments { get { return this.CommandPieces.Skip(1); } }

        private IEnumerable<string> CommandPieces { get { return this.Message.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries); } }

        public async Task<string> ShouldBeModerated()
        {
            if (this.IsWhisper)
            {
                return string.Empty;
            }

            return await ModerationHelper.ShouldBeModerated(this.User, this.Message, this.ContainsLink);
        }

        public void AddToMessage(string text)
        {
            this.Message += text;
        }

        public override bool Equals(object obj)
        {
            if (obj is ChatMessageViewModel)
            {
                return this.Equals((ChatMessageViewModel)obj);
            }
            return false;
        }

        public bool Equals(ChatMessageViewModel other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }

        public override string ToString() { return string.Format("{0}: {1}", this.User, this.Message); }
    }
}

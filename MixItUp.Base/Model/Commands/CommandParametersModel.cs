using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Commands
{
    [DataContract]
    public class CommandParametersModel
    {
        public static CommandParametersModel GetTestParameters(Dictionary<string, string> specialIdentifiers)
        {
            return new CommandParametersModel(ChannelSession.User, StreamingPlatformTypeEnum.All, new List<string>() { "@" + ChannelSession.User.Username }, specialIdentifiers);
        }

        public static List<string> GenerateArguments(string arguments)
        {
            return (!string.IsNullOrEmpty(arguments)) ? arguments.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>();
        }

        [DataMember]
        public UserV2ViewModel User { get; set; } = ChannelSession.User;
        [DataMember]
        public StreamingPlatformTypeEnum Platform { get; set; } = StreamingPlatformTypeEnum.All;
        [DataMember]
        public List<string> Arguments { get; private set; } = new List<string>();
        [DataMember]
        public Dictionary<string, string> SpecialIdentifiers { get; set; } = new Dictionary<string, string>();

        [DataMember]
        public bool IgnoreRequirements { get; set; }

        [DataMember]
        public bool UseCommandLocks { get; set; }

        [DataMember]
        public UserV2ViewModel TargetUser { get; set; }

        [DataMember]
        public string TriggeringChatMessageID { get; set; }

        [DataMember]
        public bool ExitCommand { get; set; }

        [DataMember]
        public Guid InitialCommandID { get; set; } = Guid.Empty;

        public CommandParametersModel() : this(ChannelSession.User) { }

        public CommandParametersModel(UserV2ViewModel user) : this(user, (user != null) ? user.Platform : StreamingPlatformTypeEnum.None) { }

        public CommandParametersModel(StreamingPlatformTypeEnum platform) : this(null, platform, null, null) { }

        public CommandParametersModel(StreamingPlatformTypeEnum platform, IEnumerable<string> arguments) : this(null, platform, arguments, null) { }

        public CommandParametersModel(StreamingPlatformTypeEnum platform, Dictionary<string, string> specialIdentifiers) : this(null, platform, null, specialIdentifiers) { }

        public CommandParametersModel(UserV2ViewModel user, StreamingPlatformTypeEnum platform) : this(user, platform, null, null) { }

        public CommandParametersModel(UserV2ViewModel user, IEnumerable<string> arguments) : this(user, (user != null) ? user.Platform : StreamingPlatformTypeEnum.None, arguments, null) { }

        public CommandParametersModel(UserV2ViewModel user, Dictionary<string, string> specialIdentifiers) : this(user, null, specialIdentifiers) { }

        public CommandParametersModel(UserV2ViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments) : this(user, platform, arguments, null) { }

        public CommandParametersModel(UserV2ViewModel user, StreamingPlatformTypeEnum platform, Dictionary<string, string> specialIdentifiers) : this(user, platform, null, specialIdentifiers) { }

        public CommandParametersModel(UserV2ViewModel user, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers) : this(user, (user != null) ? user.Platform : StreamingPlatformTypeEnum.None, arguments, specialIdentifiers) { }

        public CommandParametersModel(ChatMessageViewModel message, IEnumerable<string> arguments = null)
            : this(message.User, message.Platform, (arguments != null) ? arguments : message.ToArguments())
        {
            this.SpecialIdentifiers["messageemotecount"] = message.EmotesOnlyContents.Count().ToString();
            this.SpecialIdentifiers["message"] = message.PlainTextMessage;

            this.TriggeringChatMessageID = message.ID;
        }

        public CommandParametersModel(UserV2ViewModel user = null, StreamingPlatformTypeEnum platform = StreamingPlatformTypeEnum.None, IEnumerable<string> arguments = null, Dictionary<string, string> specialIdentifiers = null)
        {
            if (user != null)
            {
                this.User = user;
            }

            if (arguments != null)
            {
                this.Arguments = new List<string>(arguments);
            }

            if (specialIdentifiers != null)
            {
                this.SpecialIdentifiers = new Dictionary<string, string>(specialIdentifiers);
            }

            if (platform != StreamingPlatformTypeEnum.None)
            {
                this.Platform = platform;
            }
            else
            {
                this.Platform = this.User.Platform;
            }

            if (this.Platform == StreamingPlatformTypeEnum.None)
            {
                this.Platform = StreamingPlatformTypeEnum.All;
            }    

            this.SpecialIdentifiers[SpecialIdentifierStringBuilder.StreamingPlatformSpecialIdentifier] = this.Platform.ToString();

            this.ParseArguments();
        }

        public bool IsTargetUserSelf { get { return this.TargetUser == this.User; } }

        public void SetArguments(string arguments)
        {
            if (!string.IsNullOrEmpty(arguments))
            {
                this.SetArguments(arguments.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        public void SetArguments(IEnumerable<string> arguments)
        {
            this.Arguments = new List<string>(arguments);
            this.ParseArguments();
        }

        public CommandParametersModel Duplicate(IEnumerable<string> arguments = null)
        {
            CommandParametersModel result = new CommandParametersModel(this.User, this.Platform, (arguments != null) ? arguments : this.Arguments, this.SpecialIdentifiers);
            result.TargetUser = this.TargetUser;
            return result;
        }

        public void ParseArguments()
        {
            if (this.Arguments != null && this.Arguments.Count > 0)
            {
                if (this.Platform == StreamingPlatformTypeEnum.YouTube)
                {
                    List<string> newArguments = new List<string>();
                    for (int i = 0; i < this.Arguments.Count; i++)
                    {
                        if (this.Arguments[i].StartsWith("@"))
                        {
                            string usernameTag = this.Arguments[i];
                            for (int j = i + 1; j < this.Arguments.Count; j++)
                            {
                                usernameTag += " " + this.Arguments[j];
                                UserV2ViewModel userTag = ServiceManager.Get<UserService>().GetActiveUserByPlatform(StreamingPlatformTypeEnum.YouTube, platformUsername: usernameTag);
                                if (userTag != null)
                                {
                                    newArguments.Add(usernameTag);
                                    usernameTag = null;
                                    i = j;
                                    break;
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(usernameTag))
                            {
                                for (; i < this.Arguments.Count; i++)
                                {
                                    newArguments.Add(this.Arguments[i]);
                                }
                            }
                        }
                        else
                        {
                            newArguments.Add(this.Arguments[i]);
                        }
                    }

                    if (newArguments.Count < this.Arguments.Count)
                    {
                        this.Arguments.Clear();
                        this.Arguments.AddRange(newArguments);
                    }
                }
            }
        }

        public async Task SetTargetUser()
        {
            if (this.TargetUser == null)
            {
                if (this.Arguments != null && this.Arguments.Count > 0)
                {
                    string username = UserService.SanitizeUsername(this.Arguments.ElementAt(0));
                    if (this.Arguments.Count > 0)
                    {
                        this.TargetUser = await ServiceManager.Get<UserService>().GetUserByPlatform(this.Platform, platformUsername: username, performPlatformSearch: true);
                        if (this.TargetUser != null && !string.Equals(username, this.TargetUser.Username, StringComparison.OrdinalIgnoreCase))
                        {
                            this.TargetUser = null;
                        }
                    }
                }

                if (this.TargetUser == null)
                {
                    this.TargetUser = this.User;
                }
                await this.TargetUser.Refresh();
            }
        }
    }
}

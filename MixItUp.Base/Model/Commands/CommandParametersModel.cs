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
            return (!string.IsNullOrEmpty(arguments)) ? arguments.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList() : null;
        }

        [DataMember]
        public UserV2ViewModel User { get; set; } = ChannelSession.User;
        [DataMember]
        public StreamingPlatformTypeEnum Platform { get; set; } = StreamingPlatformTypeEnum.All;
        [DataMember]
        public List<string> Arguments { get; set; } = new List<string>();
        [DataMember]
        public Dictionary<string, string> SpecialIdentifiers { get; set; } = new Dictionary<string, string>();

        [DataMember]
        public bool IgnoreRequirements { get; set; }

        [DataMember]
        public UserV2ViewModel TargetUser { get; set; }

        [DataMember]
        public string TriggeringChatMessageID { get; set; }

        [DataMember]
        public Guid InitialCommandID { get; set; } = Guid.Empty;

        public CommandParametersModel() : this(ChannelSession.User) { }

        public CommandParametersModel(UserV2ViewModel user) : this(user, (user != null) ? user.Platform : StreamingPlatformTypeEnum.None) { }

        public CommandParametersModel(StreamingPlatformTypeEnum platform) : this(platform, null) { }

        public CommandParametersModel(ChatMessageViewModel message)
            : this(message.User, message.Platform, message.ToArguments())
        {
            this.SpecialIdentifiers["message"] = message.PlainTextMessage;

            this.TriggeringChatMessageID = message.ID;
        }

        public CommandParametersModel(StreamingPlatformTypeEnum platform, Dictionary<string, string> specialIdentifiers) : this(null, platform, null, specialIdentifiers) { }

        public CommandParametersModel(UserV2ViewModel user, StreamingPlatformTypeEnum platform) : this(user, platform, null, null) { }

        public CommandParametersModel(UserV2ViewModel user, IEnumerable<string> arguments) : this(user, (user != null) ? user.Platform : StreamingPlatformTypeEnum.None, arguments, null) { }

        public CommandParametersModel(UserV2ViewModel user, Dictionary<string, string> specialIdentifiers) : this(user, null, specialIdentifiers) { }

        public CommandParametersModel(UserV2ViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments) : this(user, platform, arguments, null) { }

        public CommandParametersModel(UserV2ViewModel user, StreamingPlatformTypeEnum platform, Dictionary<string, string> specialIdentifiers) : this(user, platform, null, specialIdentifiers) { }

        public CommandParametersModel(UserV2ViewModel user, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers) : this(user, (user != null) ? user.Platform : StreamingPlatformTypeEnum.None, arguments, specialIdentifiers) { }

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

            this.SpecialIdentifiers[SpecialIdentifierStringBuilder.StreamingPlatformSpecialIdentifier] = this.Platform.ToString();
        }

        public bool IsTargetUserSelf { get { return this.TargetUser == this.User; } }

        public CommandParametersModel Duplicate()
        {
            CommandParametersModel result = new CommandParametersModel(this.User, this.Platform, this.Arguments, this.SpecialIdentifiers);
            result.TargetUser = this.TargetUser;
            return result;
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
                        this.TargetUser = await ServiceManager.Get<UserService>().GetUserByPlatformUsername(this.Platform, username, performPlatformSearch: true);
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
            }
        }
    }
}

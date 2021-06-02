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
            UserViewModel currentUser = ChannelSession.GetCurrentUser();
            return new CommandParametersModel(currentUser, StreamingPlatformTypeEnum.All, new List<string>() { "@" + currentUser.Username }, specialIdentifiers) { TargetUser = currentUser };
        }

        [DataMember]
        public UserViewModel User { get; set; } = ChannelSession.GetCurrentUser();
        [DataMember]
        public StreamingPlatformTypeEnum Platform { get; set; } = StreamingPlatformTypeEnum.All;
        [DataMember]
        public List<string> Arguments { get; set; } = new List<string>();
        [DataMember]
        public Dictionary<string, string> SpecialIdentifiers { get; set; } = new Dictionary<string, string>();

        [DataMember]
        public UserViewModel TargetUser { get; set; }

        public CommandParametersModel() : this(ChannelSession.GetCurrentUser()) { }

        public CommandParametersModel(UserViewModel user) : this(user, StreamingPlatformTypeEnum.All) { }

        public CommandParametersModel(Dictionary<string, string> specialIdentifiers) : this(ChannelSession.GetCurrentUser(), specialIdentifiers) { }

        public CommandParametersModel(UserViewModel user, StreamingPlatformTypeEnum platform) : this(user, platform, null) { }

        public CommandParametersModel(UserViewModel user, IEnumerable<string> arguments) : this(user, StreamingPlatformTypeEnum.All, arguments, null) { }

        public CommandParametersModel(UserViewModel user, Dictionary<string, string> specialIdentifiers) : this(user, null, specialIdentifiers) { }

        public CommandParametersModel(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments) : this(user, platform, arguments, null) { }

        public CommandParametersModel(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers) : this(user, StreamingPlatformTypeEnum.All, arguments, specialIdentifiers) { }

        public CommandParametersModel(ChatMessageViewModel message) : this(message.User, message.Platform, message.ToArguments()) { }

        public CommandParametersModel(UserViewModel user = null, StreamingPlatformTypeEnum platform = StreamingPlatformTypeEnum.All, IEnumerable<string> arguments = null, Dictionary<string, string> specialIdentifiers = null)
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

            if (platform != StreamingPlatformTypeEnum.All)
            {
                this.Platform = platform;
            }
            else
            {
                this.Platform = this.User.Platform;
            }
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
                if (this.Arguments.Count > 0)
                {
                    this.TargetUser = await ChannelSession.Services.User.GetUserFullSearch(this.Platform, userID: null, this.Arguments.First());
                }

                if (this.TargetUser == null || !this.Arguments.ElementAt(0).Replace("@", "").Equals(this.TargetUser.Username, StringComparison.InvariantCultureIgnoreCase))
                {
                    this.TargetUser = this.User;
                }
            }
        }
    }
}

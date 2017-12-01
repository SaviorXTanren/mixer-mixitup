using Mixer.Base.Util;
using MixItUp.Base.Actions;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MixItUp.Base.Commands
{
    public enum CommandTypeEnum
    {
        Chat,
        Interactive,
        Event,
        Timer
    }

    [DataContract]
    public abstract class CommandBase
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public CommandTypeEnum Type { get; set; }

        [DataMember]
        public List<string> Commands { get; set; }

        [DataMember]
        public List<ActionBase> Actions { get; set; }

        [DataMember]
        public bool IsEnabled { get; set; }

        [XmlIgnore]
        public string TypeName { get { return EnumHelper.GetEnumName(this.Type); } }

        public CommandBase()
        {
            this.Commands = new List<string>();
            this.Actions = new List<ActionBase>();
            this.IsEnabled = true;
        }

        public CommandBase(string name, CommandTypeEnum type, string command) : this(name, type, new List<string>() { command }) { }

        public CommandBase(string name, CommandTypeEnum type, IEnumerable<string> commands)
            : this()
        {
            this.Name = name;
            this.Type = type;
            this.Commands.AddRange(commands);
        }

        public string CommandsString { get { return string.Join(" ", this.Commands); } }

        public bool ContainsCommand(string command) { return this.Commands.Contains(command); }

        public async Task Perform() { await this.Perform(null); }

        public async Task Perform(IEnumerable<string> arguments) { await this.Perform(ChannelSession.GetCurrentUser(), arguments); }

        public virtual async Task Perform(UserViewModel user, IEnumerable<string> arguments = null)
        {
            if (!this.IsEnabled)
            {
                return;
            }

            if (arguments == null)
            {
                arguments = new List<string>();
            }

            await this.AsyncSempahore.WaitAsync();

            GlobalEvents.CommandExecuted(this);

            foreach (ActionBase action in this.Actions)
            {
                await action.Perform(user, arguments);
            }

            this.AsyncSempahore.Release();
        }

        protected abstract SemaphoreSlim AsyncSempahore { get; }
    }
}

using Mixer.Base.Util;
using Mixer.Base.ViewModel;
using MixItUp.Base.Actions;
using System.Collections.Generic;
using System.Runtime.Serialization;
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
    public class CommandBase
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public CommandTypeEnum Type { get; set; }

        [DataMember]
        public string Command { get; set; }

        [DataMember]
        public List<ActionBase> Actions { get; set; }

        [XmlIgnore]
        public string TypeName { get { return EnumHelper.GetEnumName(this.Type); } }

        public CommandBase()
        {
            this.Actions = new List<ActionBase>();
        }

        public CommandBase(string name, CommandTypeEnum type, string command)
            : this()
        {
            this.Name = name;
            this.Type = type;
            this.Command = command;
        }

        public async Task Perform() { await this.Perform(null); }

        public async Task Perform(IEnumerable<string> arguments) { await this.Perform(new UserViewModel(ChannelSession.User.id, ChannelSession.User.username), arguments); }

        public async Task Perform(UserViewModel user, IEnumerable<string> arguments = null)
        {
            if (arguments == null)
            {
                arguments = new List<string>();
            }

            List<Task> taskActionsToPerform = new List<Task>();
            foreach (ActionBase action in this.Actions)
            {
                taskActionsToPerform.Add(action.Perform(user, arguments));

                if (action.Type == ActionTypeEnum.Wait)
                {
                    await Task.WhenAll(taskActionsToPerform);
                    taskActionsToPerform.Clear();
                }
            }
            await Task.WhenAll(taskActionsToPerform);
        }
    }
}

using Mixer.Base.ViewModel;
using MixItUp.Base.Actions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Commands
{
    public class CommandBase
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Command { get; set; }

        public string Description { get; set; }

        public List<ActionBase> Actions { get; set; }

        public CommandBase(string name, string type, string command, string description)
        {
            this.Name = name;
            this.Type = type;
            this.Command = command;
            this.Description = description;
            this.Actions = new List<ActionBase>();
        }

        public async Task Perform(UserViewModel user)
        {
            foreach (ActionBase action in this.Actions)
            {
                await action.Perform(user);
            }
        }
    }
}

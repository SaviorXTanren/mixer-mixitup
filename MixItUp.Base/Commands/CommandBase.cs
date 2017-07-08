using Mixer.Base.ViewModel;
using MixItUp.Base.Actions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Commands
{
    public class CommandBase
    {
        public string Name { get; private set; }

        public List<ActionBase> Actions { get; set; }

        public CommandBase(string name)
        {
            this.Name = name;
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

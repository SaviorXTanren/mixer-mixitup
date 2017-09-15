using MixItUp.Base.Commands;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.Generic;
using MixItUp.Base.Actions;

namespace MixItUp.WPF.Controls.Command
{
    public class CommandDetailsControlBase : UserControl
    {
        public virtual Task Initialize() { return Task.FromResult(0); }

        public virtual IEnumerable<ActionTypeEnum> GetAllowedActions() { return new List<ActionTypeEnum>(); }

        public virtual bool Validate() { return false; }

        public virtual CommandBase GetExistingCommand() { return null; }

        public virtual Task<CommandBase> GetNewCommand() { return Task.FromResult<CommandBase>(null); }
    }
}

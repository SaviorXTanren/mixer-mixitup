using MixItUp.Base.Commands;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Command
{
    public class CommandDetailsControlBase : UserControl
    {
        public virtual Task Initialize() { return Task.FromResult(0); }

        public virtual Task<bool> Validate() { return Task.FromResult(false); }

        public virtual CommandBase GetExistingCommand() { return null; }

        public virtual Task<CommandBase> GetNewCommand() { return Task.FromResult<CommandBase>(null); }
    }
}

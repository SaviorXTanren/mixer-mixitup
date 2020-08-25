using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Actions
{
    public abstract class ActionEditorControlViewModelBase : UIViewModelBase
    {
        public abstract ActionTypeEnum Type { get; }

        public virtual Task<Result> Validate()
        {
            return Task.FromResult(new Result());
        }

        public abstract Task<ActionModelBase> GetAction();
    }
}

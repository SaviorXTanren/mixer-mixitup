using MixItUp.Base.Model.Requirements;
using MixItUp.Base.ViewModels;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirements
{
    public abstract class RequirementViewModelBase : UIViewModelBase
    {
        public virtual Task<bool> Validate()
        {
            return Task.FromResult(true);
        }

        public abstract RequirementModelBase GetRequirement();
    }

    public abstract class ListRequirementViewModelBase : UIViewModelBase { }
}

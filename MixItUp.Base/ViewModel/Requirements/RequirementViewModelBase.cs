using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirements
{
    public abstract class RequirementViewModelBase : UIViewModelBase
    {
        public virtual Task<Result> Validate()
        {
            return Task.FromResult(new Result());
        }

        public abstract RequirementModelBase GetRequirement();

        protected bool ValidateStringAmount(string value, bool canBeZero = false)
        {
            if (this.IsSpecialIdentifier(value))
            {
                return true;
            }
            else if (int.TryParse(value, out int iAmount))
            {
                if (canBeZero)
                {
                    return iAmount >= 0;
                }
                else
                {
                    return iAmount > 0;
                }
            }
            return false;
        }

        protected bool IsSpecialIdentifier(string value) { return !string.IsNullOrEmpty(value) && value.StartsWith(SpecialIdentifierStringBuilder.SpecialIdentifierHeader); }
    }

    public abstract class ListRequirementViewModelBase : UIViewModelBase { }
}

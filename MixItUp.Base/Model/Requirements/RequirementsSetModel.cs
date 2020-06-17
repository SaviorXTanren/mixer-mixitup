using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Requirements
{
    [DataContract]
    public class RequirementsSetModel
    {
        [DataMember]
        public List<RequirementModelBase> Requirements { get; set; } = new List<RequirementModelBase>();

        public RequirementsSetModel() { }

        public async Task<bool> Validate(UserViewModel user)
        {
            foreach (RequirementModelBase requirement in this.Requirements)
            {
                if (!await requirement.Validate(user))
                {
                    return false;
                }
            }
            return true;
        }

        public async Task Perform(UserViewModel user)
        {
            List<UserViewModel> users = new List<UserViewModel>();

            ThresholdRequirementModel threshold = (ThresholdRequirementModel)this.Requirements.FirstOrDefault(r => r is ThresholdRequirementModel);
            if (threshold != null)
            {
                foreach (UserViewModel u in threshold.GetApplicableUsers())
                {
                    users.Add(u);
                }
            }
            else
            {
                users.Add(user);
            }

            foreach (RequirementModelBase requirement in this.Requirements)
            {
                foreach (UserViewModel u in users)
                {
                    await requirement.Perform(u);
                }
            }
        }

        public async Task Refund(UserViewModel user)
        {
            foreach (RequirementModelBase requirement in this.Requirements)
            {
                await requirement.Refund(user);
            }
        }

        public void Reset()
        {
            foreach (RequirementModelBase requirement in this.Requirements)
            {
                requirement.Reset();
            }
        }
    }
}

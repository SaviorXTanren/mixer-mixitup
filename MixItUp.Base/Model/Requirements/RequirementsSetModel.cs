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

        public ThresholdRequirementModel Threshold { get { return (ThresholdRequirementModel)this.Requirements.FirstOrDefault(r => r is ThresholdRequirementModel); } }

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
            IEnumerable<UserViewModel> users = this.GetRequirementUsers(user);
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
            IEnumerable<UserViewModel> users = this.GetRequirementUsers(user);
            foreach (RequirementModelBase requirement in this.Requirements)
            {
                foreach (UserViewModel u in users)
                {
                    await requirement.Refund(u);
                }
            }
        }

        public void Reset()
        {
            foreach (RequirementModelBase requirement in this.Requirements)
            {
                requirement.Reset();
            }
        }

        public IEnumerable<UserViewModel> GetPerformingUsers(UserViewModel user)
        {
            ThresholdRequirementModel threshold = this.Threshold;
            if (threshold != null && threshold.IsEnabled && threshold.RunForEachUser)
            {
                return this.GetRequirementUsers(user);
            }
            else
            {
                return new List<UserViewModel>() { user };
            }
        }

        public IEnumerable<UserViewModel> GetRequirementUsers(UserViewModel user)
        {
            List<UserViewModel> users = new List<UserViewModel>();
            ThresholdRequirementModel threshold = this.Threshold;
            if (threshold != null && threshold.IsEnabled)
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
            return users;
        }
    }
}

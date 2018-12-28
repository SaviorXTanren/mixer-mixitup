using MixItUp.Base.ViewModel.User;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirement
{
    [DataContract]
    public class SettingsRequirementViewModel
    {
        [DataMember]
        public bool DeleteChatCommandWhenRun { get; set; }

        public SettingsRequirementViewModel() { }

        public bool DoesMeetRequirement(UserViewModel user)
        {
            return true;
        }

        public Task SendSettingsNotMetWhisper(UserViewModel user)
        {
            return Task.FromResult(0);
        }
    }
}

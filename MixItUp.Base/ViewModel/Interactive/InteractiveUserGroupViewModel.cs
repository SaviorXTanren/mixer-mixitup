using Mixer.Base.Model.Interactive;
using Mixer.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.Interactive
{
    [DataContract]
    public class InteractiveUserGroupViewModel
    {
        public const string DefaultName = "default";

        public InteractiveUserGroupViewModel() { }

        public InteractiveUserGroupViewModel(MixerRoleEnum associatedUserRole)
            : this((associatedUserRole != MixerRoleEnum.User) ? EnumHelper.GetEnumName(associatedUserRole) : DefaultName)
        {
            this.AssociatedUserRole = associatedUserRole;
        }

        public InteractiveUserGroupViewModel(string groupName) : this(groupName, InteractiveUserGroupViewModel.DefaultName) { }

        public InteractiveUserGroupViewModel(string groupName, string defaultScene)
        {
            this.GroupName = groupName;
            this.DefaultScene = defaultScene;
            this.AssociatedUserRole = MixerRoleEnum.Custom;
        }

        public InteractiveUserGroupViewModel(InteractiveGroupModel group) : this(group.groupID, group.sceneID) { }

        [DataMember]
        public string GroupName { get; set; }
        [DataMember]
        public string DefaultScene { get; set; }
        [DataMember]
        public MixerRoleEnum AssociatedUserRole { get; set; }

        [JsonIgnore]
        public string CurrentScene { get; set; }
    }
}

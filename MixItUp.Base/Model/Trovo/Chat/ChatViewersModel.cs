using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Trovo.Chat
{
    /// <summary>
    /// Viewer information for a channel.
    /// </summary>
    [DataContract]
    public class ChatViewersModel : ChatViewersRolesModel
    {
        /// <summary>
        /// The channel's total login users.
        /// </summary>
        [DataMember]
        public int Total { get; set; }

        /// <summary>
        /// The custom role viewers.
        /// </summary>
        [DataMember]
        public Dictionary<string, ChatViewersRoleGroupModel> CustomRoles { get; set; } = new Dictionary<string, ChatViewersRoleGroupModel>();
    }

    /// <summary>
    /// Role viewers for a channel.
    /// </summary>
    [DataContract]
    public class ChatViewersRolesModel
    {
        /// <summary>
        /// The VIP viewers.
        /// </summary>
        [DataMember]
        public ChatViewersRoleGroupModel VIPS { get; set; } = new ChatViewersRoleGroupModel();

        /// <summary>
        /// The ace viewers.
        /// </summary>
        [DataMember]
        public ChatViewersRoleGroupModel ace { get; set; } = new ChatViewersRoleGroupModel();

        /// <summary>
        /// The aceplus viewers.
        /// </summary>
        [DataMember]
        public ChatViewersRoleGroupModel aceplus { get; set; } = new ChatViewersRoleGroupModel();

        /// <summary>
        /// The admins viewers.
        /// </summary>
        [DataMember]
        public ChatViewersRoleGroupModel admins { get; set; } = new ChatViewersRoleGroupModel();

        /// <summary>
        /// The all viewers.
        /// </summary>
        [DataMember]
        public ChatViewersRoleGroupModel all { get; set; } = new ChatViewersRoleGroupModel();

        /// <summary>
        /// The creators viewers.
        /// </summary>
        [DataMember]
        public ChatViewersRoleGroupModel creators { get; set; } = new ChatViewersRoleGroupModel();

        /// <summary>
        /// The editors viewers.
        /// </summary>
        [DataMember]
        public ChatViewersRoleGroupModel editors { get; set; } = new ChatViewersRoleGroupModel();

        /// <summary>
        /// The followers viewers.
        /// </summary>
        [DataMember]
        public ChatViewersRoleGroupModel followers { get; set; } = new ChatViewersRoleGroupModel();

        /// <summary>
        /// The moderators viewers.
        /// </summary>
        [DataMember]
        public ChatViewersRoleGroupModel moderators { get; set; } = new ChatViewersRoleGroupModel();

        /// <summary>
        /// The subscribers viewers.
        /// </summary>
        [DataMember]
        public ChatViewersRoleGroupModel subscribers { get; set; } = new ChatViewersRoleGroupModel();

        /// <summary>
        /// The supermods viewers.
        /// </summary>
        [DataMember]
        public ChatViewersRoleGroupModel supermods { get; set; } = new ChatViewersRoleGroupModel();

        /// <summary>
        /// The wardens viewers.
        /// </summary>
        [DataMember]
        public ChatViewersRoleGroupModel wardens { get; set; } = new ChatViewersRoleGroupModel();
    }

    /// <summary>
    /// Viewers for a specific role.
    /// </summary>
    [DataContract]
    public class ChatViewersRoleGroupModel
    {
        /// <summary>
        /// The list of viewers.
        /// </summary>
        [DataMember]
        public List<string> viewers { get; set; } = new List<string>();
    }

    public class ChatViewersInternalModel : PageDataResponseModel
    {
        public string nickname { get; set; }

        public string live_title { get; set; }

        public ChatViewersRolesModel chatters { get; set; } = new ChatViewersRolesModel();

        public JObject custom_roles { get; set; } = new JObject();

        public JObject custome_roles { get; set; } = new JObject();

        public override int GetItemCount() { return this.total; }
    }
}

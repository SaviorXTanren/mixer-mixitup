using MixItUp.Base.ViewModel.User;
using System.Data.Common;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.ScorpBot
{
    [DataContract]
    public class ScorpBotCommand
    {
        public static bool IsACommand(DbDataReader reader) { return ((string)reader["Command"]).StartsWith("!"); }

        [DataMember]
        public string Command { get; set; }

        [DataMember]
        public UserRole Permission { get; set; }

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public int Cooldown { get; set; }

        [DataMember]
        public bool Enabled { get; set; }

        public ScorpBotCommand() { }

        public ScorpBotCommand(DbDataReader reader)
        {
            this.Command = (string)reader["Command"];
            this.Command = this.Command.ToLower();
            this.Command = this.Command.Replace("!", "");

            this.Permission = UserRole.User;

            string permInfo = (string)reader["PermInfo"];
            if (permInfo.Contains("followed.php"))
            {
                this.Permission = UserRole.Follower;
            }

            string permission = (string)reader["Permission"];
            if (permission.Equals("Moderator"))
            {
                this.Permission = UserRole.Mod;
            }

            this.Text = (string)reader["Response"];
            this.Cooldown = (int)reader["Cooldown"];
            this.Enabled = ((string)reader["Enabled"]).Equals("True");
        }
    }
}

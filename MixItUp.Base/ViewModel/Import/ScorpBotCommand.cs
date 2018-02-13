using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Data.Common;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.Import
{
    [DataContract]
    public class ScorpBotCommand
    {
        public static bool IsACommand(DbDataReader reader) { return ((string)reader["Command"]).StartsWith("!"); }

        [DataMember]
        public string Command { get; set; }

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public int Cooldown { get; set; }

        [DataMember]
        public RequirementViewModel Requirements { get; set; }

        [DataMember]
        public bool Enabled { get; set; }

        public ScorpBotCommand()
        {
            this.Requirements = new RequirementViewModel();
        }

        public ScorpBotCommand(string command, string text)
            : this()
        {
            this.Command = command;
            this.Command = this.Command.ToLower();
            this.Command = this.Command.Replace("!", "");

            this.Text = text;
            this.Text = SpecialIdentifierStringBuilder.ConvertScorpBotText(this.Text);

            this.Requirements.UserRole = UserRole.User;

            this.Enabled = true;
        }

        public ScorpBotCommand(DbDataReader reader)
            : this((string)reader["Command"], (string)reader["Response"])
        {
            string permInfo = (string)reader["PermInfo"];
            if (permInfo.Contains("followed.php"))
            {
                this.Requirements.UserRole = UserRole.Follower;
            }

            string permission = (string)reader["Permission"];
            if (permission.Equals("Moderator"))
            {
                this.Requirements.UserRole = UserRole.Mod;
            }

            this.Cooldown = (int)reader["Cooldown"];
            this.Enabled = ((string)reader["Enabled"]).Equals("True");
        }
    }
}

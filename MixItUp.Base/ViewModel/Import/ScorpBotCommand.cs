using MixItUp.Base.Actions;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.Import
{
    [DataContract]
    public class ScorpBotCommand
    {
        public const string SFXRegexHeaderPattern = "$sfx(";
        public const string ReadAPIRegexHeaderPattern = "$readapi(";

        [DataMember]
        public string Command { get; set; }

        [DataMember]
        public bool ContainsExclamation { get; set; }

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public List<ActionBase> Actions { get; set; }

        [DataMember]
        public int Cooldown { get; set; }

        [DataMember]
        public RequirementViewModel Requirements { get; set; }

        [DataMember]
        public bool Enabled { get; set; }

        public ScorpBotCommand()
        {
            this.Actions = new List<ActionBase>();
            this.Requirements = new RequirementViewModel();
        }

        public ScorpBotCommand(string command, string text)
            : this()
        {
            this.Command = command;
            this.Command = this.Command.ToLower();

            this.ContainsExclamation = this.Command.Contains("!");

            this.Command = this.Command.Replace("!", "");

            this.Text = SpecialIdentifierStringBuilder.ConvertScorpBotText(text);

            this.GetRegexEntries(SFXRegexHeaderPattern, (string entry) =>
            {
                this.Actions.Add(new SoundAction(entry, 100));
                return string.Empty;
            });

            int webRequestCount = 1;
            this.GetRegexEntries(ReadAPIRegexHeaderPattern, (string entry) =>
            {
                string si = "webrequest" + webRequestCount;
                this.Actions.Add(WebRequestAction.CreateForSpecialIdentifier(entry, si));
                webRequestCount++;
                return "$" + si;
            });

            this.Actions.Add(new ChatAction(this.Text));

            this.Requirements.Role.MixerRole = MixerRoleEnum.User;

            this.Enabled = true;
        }

        public ScorpBotCommand(DbDataReader reader)
            : this((string)reader["Command"], (string)reader["Response"])
        {
            string permInfo = (string)reader["PermInfo"];
            if (permInfo.Contains("followed.php"))
            {
                this.Requirements.Role.MixerRole = MixerRoleEnum.Follower;
            }

            string permission = (string)reader["Permission"];
            if (permission.Equals("Moderator"))
            {
                this.Requirements.Role.MixerRole = MixerRoleEnum.Mod;
            }

            this.Requirements.Cooldown.Amount = (int)reader["Cooldown"];
            this.Enabled = ((string)reader["Enabled"]).Equals("True");
        }

        private IEnumerable<string> GetRegexEntries(string pattern, Func<string, string> replacement)
        {
            List<string> entries = new List<string>();

            int startIndex = 0;
            do
            {
                startIndex = this.Text.IndexOf(pattern);
                if (startIndex >= 0)
                {
                    int endIndex = this.Text.IndexOf(")", startIndex);
                    if (endIndex >= 0)
                    {
                        string fullEntry = this.Text.Substring(startIndex, endIndex - startIndex + 1);
                        string entry = fullEntry.Replace(pattern, "").Replace(")", "");
                        entries.Add(entry);
                        this.Text = this.Text.Replace(fullEntry, replacement(entry));
                    }
                }
            } while (startIndex >= 0);

            return entries;
        }
    }
}

using MixItUp.Base.Actions;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Import.ScorpBot
{
    [DataContract]
    public class ScorpBotCommand : ImportDataViewModelBase
    {
        public const string SFXRegexHeaderPattern = "$sfx(";
        public const string ReadAPIRegexHeaderPattern = "$readapi(";
        public const string ReadFileRegexHeaderPattern = "$readfile(";
        public const string WriteFileRegexHeaderPattern = "$writefile(";

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

            this.Text = text;

            this.ContainsExclamation = this.Command.Contains("!");

            this.Command = this.Command.Replace("!", "");

            this.Requirements.Role.MixerRole = MixerRoleEnum.User;

            this.Enabled = true;
        }

        public ScorpBotCommand(Dictionary<string, object> data)
            : this((string)data["Command"], (string)data["Response"])
        {
            string permInfo = (string)data["PermInfo"];
            if (permInfo.Contains("followed.php"))
            {
                this.Requirements.Role.MixerRole = MixerRoleEnum.Follower;
            }

            string permission = (string)data["Permission"];
            if (permission.Equals("Moderator"))
            {
                this.Requirements.Role.MixerRole = MixerRoleEnum.Mod;
            }

            this.Requirements.Cooldown.Amount = (int)data["Cooldown"];
            this.Enabled = ((string)data["Enabled"]).Equals("True");
        }

        public void ProcessData(UserCurrencyViewModel currency, UserCurrencyViewModel rank)
        {
            this.Text = SpecialIdentifierStringBuilder.ConvertScorpBotText(this.Text);

            this.Text = this.GetRegexEntries(this.Text, SFXRegexHeaderPattern, (string entry) =>
            {
                this.Actions.Add(new SoundAction(entry, 100));
                return string.Empty;
            });

            int webRequestCount = 1;
            this.Text = this.GetRegexEntries(this.Text, ReadAPIRegexHeaderPattern, (string entry) =>
            {
                string si = "webrequest" + webRequestCount;
                this.Actions.Add(WebRequestAction.CreateForSpecialIdentifier(entry, si));
                webRequestCount++;
                return "$" + si;
            });

            if (this.Text.Contains("$toppoints("))
            {
                this.Text = SpecialIdentifierStringBuilder.ReplaceParameterVariablesEntries(this.Text, "$toppoints(", "$top", rank.SpecialIdentifier);
            }

            this.Text = this.Text.Replace("$points", "$" + rank.UserAmountSpecialIdentifier);
            this.Text = this.Text.Replace("$rank", "$" + rank.UserRankNameSpecialIdentifier);

            int readCount = 1;
            this.Text = this.GetRegexEntries(this.Text, ReadFileRegexHeaderPattern, (string entry) =>
            {
                string si = "read" + readCount;

                string[] splits = entry.Split(new char[] { ',' });
                FileAction action = new FileAction(FileActionTypeEnum.ReadSpecificLineFromFile, si, splits[0]);
                if (splits.Length > 1)
                {
                    action.FileActionType = FileActionTypeEnum.ReadSpecificLineFromFile;
                    if (splits[1].Equals("first"))
                    {
                        action.LineIndexToRead = "1";
                    }
                    else if (splits[1].Equals("random"))
                    {
                        action.FileActionType = FileActionTypeEnum.ReadRandomLineFromFile;
                    }
                    else
                    {
                        action.LineIndexToRead = splits[1];
                    }
                }
                this.Actions.Add(action);

                readCount++;
                return "$" + si;
            });

            this.Text = this.GetRegexEntries(this.Text, WriteFileRegexHeaderPattern, (string entry) =>
            {
                string[] splits = entry.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (splits.Length == 1)
                {
                    this.Actions.Add(new FileAction(FileActionTypeEnum.AppendToFile, string.Empty, splits[0]));
                }
                else if (splits.Length == 2)
                {
                    this.Actions.Add(new FileAction(FileActionTypeEnum.AppendToFile, splits[1], splits[0]));
                }
                else if (splits.Length > 2)
                {
                    FileAction action = new FileAction(FileActionTypeEnum.AppendToFile, splits[1], splits[0]);
                    if (bool.TryParse(splits[2], out bool overwrite) && overwrite)
                    {
                        action.FileActionType = FileActionTypeEnum.SaveToFile;
                    }
                    this.Actions.Add(action);
                }
                return string.Empty;
            });

            this.Actions.Add(new ChatAction(this.Text));
        }
    }
}

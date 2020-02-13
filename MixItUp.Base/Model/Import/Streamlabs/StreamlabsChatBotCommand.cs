using MixItUp.Base.Actions;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Import.Streamlabs
{
    [DataContract]
    public class StreamlabsChatBotCommand : ImportDataViewModelBase
    {
        public const string ReadAPIRegexHeaderPattern = "$readapi(";

        public const string ReadLineRegexHeaderPattern = "$readline(";
        public const string ReadRandomLineRegexHeaderPattern = "$readrandline(";
        public const string ReadSpecificLineRegexHeaderPattern = "$readspecificline(";

        public const string SaveToFileRegexHeaderPattern = "$savetofile(";
        public const string OverwriteFileRegexHeaderPattern = "$overwritefile(";

        [DataMember]
        public string Command { get; set; }
        [DataMember]
        public string Permission { get; set; }
        [DataMember]
        public string PermInfo { get; set; }
        [DataMember]
        public int Count { get; set; }
        [DataMember]
        public string Response { get; set; }
        [DataMember]
        public int Cooldown { get; set; }
        [DataMember]
        public int UserCooldown { get; set; }
        [DataMember]
        public string Usage { get; set; }
        [DataMember]
        public bool Enabled { get; set; }

        [DataMember]
        public RequirementViewModel Requirements { get; set; }

        [DataMember]
        public List<ActionBase> Actions { get; set; }

        public StreamlabsChatBotCommand()
        {
            this.Actions = new List<ActionBase>();
        }

        public StreamlabsChatBotCommand(List<string> values)
            : this()
        {
            this.Command = values[0];
            this.Permission = values[1];
            this.PermInfo = values[2];

            int.TryParse(values[3], out int count);
            this.Count = count;

            this.Response = values[4];

            int.TryParse(values[5], out int cooldown);
            this.Cooldown = cooldown;

            int.TryParse(values[6], out int userCooldown);
            this.UserCooldown = userCooldown;

            this.Usage = values[7];
            this.Enabled = bool.Parse(values[8]);
        }

        public void ProcessData(UserCurrencyModel currency, UserCurrencyModel rank)
        {
            this.Requirements = new RequirementViewModel();

            if (this.Cooldown > 0)
            {
                this.Requirements.Cooldown = new CooldownRequirementViewModel(CooldownTypeEnum.Individual, this.Cooldown);
            }

            if (!string.IsNullOrEmpty(this.Permission))
            {
                switch (this.Permission)
                {
                    case "Subscriber":
                        this.Requirements.Role = new RoleRequirementViewModel(UserRoleEnum.Subscriber);
                        break;
                    case "Moderator":
                        this.Requirements.Role = new RoleRequirementViewModel(UserRoleEnum.Mod);
                        break;
                    case "Editor":
                        this.Requirements.Role = new RoleRequirementViewModel(UserRoleEnum.ChannelEditor);
                        break;
                    case "Min_Points":
                        this.Requirements.Role = new RoleRequirementViewModel(UserRoleEnum.User);
                        if (!string.IsNullOrEmpty(this.PermInfo) && int.TryParse(this.PermInfo, out int cost))
                        {
                            this.Requirements.Currency = new CurrencyRequirementViewModel(currency, cost);
                        }
                        break;
                    case "Min_Rank":
                        this.Requirements.Role = new RoleRequirementViewModel(UserRoleEnum.User);
                        if (!string.IsNullOrEmpty(this.PermInfo))
                        {
                            UserRankViewModel minRank = rank.Ranks.FirstOrDefault(r => r.Name.ToLower().Equals(this.PermInfo.ToLower()));
                            if (rank != null)
                            {
                                this.Requirements.Rank = new CurrencyRequirementViewModel(rank, minRank);
                            }
                        }
                        break;
                    default:
                        this.Requirements.Role = new RoleRequirementViewModel(UserRoleEnum.User);
                        break;
                }
            }

            this.Response = SpecialIdentifierStringBuilder.ConvertStreamlabsChatBotText(this.Response);

            int readCount = 1;
            this.Response = this.GetRegexEntries(this.Response, ReadLineRegexHeaderPattern, (string entry) =>
            {
                string si = "read" + readCount;
                this.Actions.Add(new FileAction(FileActionTypeEnum.ReadFromFile, si, entry));
                readCount++;
                return "$" + si;
            });

            this.Response = this.GetRegexEntries(this.Response, ReadRandomLineRegexHeaderPattern, (string entry) =>
            {
                string si = "read" + readCount;
                this.Actions.Add(new FileAction(FileActionTypeEnum.ReadRandomLineFromFile, si, entry));
                readCount++;
                return "$" + si;
            });

            this.Response = this.GetRegexEntries(this.Response, ReadRandomLineRegexHeaderPattern, (string entry) =>
            {
                string si = "read" + readCount;

                string[] splits = entry.Split(new char[] { ',' });
                FileAction action = new FileAction(FileActionTypeEnum.ReadSpecificLineFromFile, si, splits[0]);
                action.LineIndexToRead = splits[1];
                this.Actions.Add(action);

                readCount++;
                return "$" + si;
            });

            int webRequestCount = 1;
            this.Response = this.GetRegexEntries(this.Response, ReadAPIRegexHeaderPattern, (string entry) =>
            {
                string si = "webrequest" + webRequestCount;
                this.Actions.Add(WebRequestAction.CreateForSpecialIdentifier(entry, si));
                webRequestCount++;
                return "$" + si;
            });

            this.Response = this.GetRegexEntries(this.Response, SaveToFileRegexHeaderPattern, (string entry) =>
            {
                string[] splits = entry.Split(new char[] { ',' });
                FileAction action = new FileAction(FileActionTypeEnum.AppendToFile, splits[1], splits[0]);
                action.LineIndexToRead = splits[1];
                this.Actions.Add(action);
                return string.Empty;
            });

            this.Response = this.GetRegexEntries(this.Response, OverwriteFileRegexHeaderPattern, (string entry) =>
            {
                string[] splits = entry.Split(new char[] { ',' });
                FileAction action = new FileAction(FileActionTypeEnum.SaveToFile, splits[1], splits[0]);
                action.LineIndexToRead = splits[1];
                this.Actions.Add(action);
                return string.Empty;
            });

            ChatAction chat = new ChatAction(this.Response);
            if (!string.IsNullOrEmpty(this.Usage) && this.Usage.Equals("SW"))
            {
                chat.IsWhisper = true;
            }
            this.Actions.Add(chat);
        }
    }
}

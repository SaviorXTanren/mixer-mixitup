using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Commands
{
    public class ChatCommandEditorWindowViewModel : CommandEditorWindowViewModelBase
    {
        public virtual bool ShowCommandGroupSelector { get { return true; } }

        public string Triggers
        {
            get { return this.triggers; }
            set
            {
                this.triggers = value;
                this.NotifyPropertyChanged();
            }
        }
        private string triggers;

        public bool IncludeExclamation
        {
            get { return this.includeExclamation; }
            set
            {
                this.includeExclamation = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ChatTriggersHintText");
            }
        }
        private bool includeExclamation = true;

        public bool IncludeExclamationEnabled
        {
            get { return this.includeExclamationEnabled; }
            set
            {
                this.includeExclamationEnabled = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool includeExclamationEnabled;

        public bool Wildcards
        {
            get { return this.wildcards; }
            set
            {
                this.wildcards = value;
                this.IncludeExclamation = this.IncludeExclamationEnabled = !this.Wildcards;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ChatTriggersHintText");
            }
        }
        private bool wildcards;

        public string ChatTriggersHintText
        {
            get
            {
                if (this.IncludeExclamation)
                {
                    return MixItUp.Base.Resources.ChatTriggersNoExclamationHintAssist;
                }
                else
                {
                    return MixItUp.Base.Resources.ChatTriggersHintAssist;
                }
            }
        }

        public ChatCommandEditorWindowViewModel(ChatCommandModel existingCommand)
            : base(existingCommand)
        {
            if (existingCommand.Triggers.Any(t => t.Contains(' ')))
            {
                this.Triggers = string.Join(";", existingCommand.Triggers);

                // If this is the only trigger, we need to add a semi-colon to the end
                if (existingCommand.Triggers.Count == 1)
                {
                    this.Triggers += ";";
                }
            }
            else
            {
                this.Triggers = string.Join(" ", existingCommand.Triggers);
            }
            this.Wildcards = existingCommand.Wildcards;
            this.IncludeExclamation = existingCommand.IncludeExclamation;
        }

        public ChatCommandEditorWindowViewModel(CommandTypeEnum commandType) : base(commandType) { }

        public ChatCommandEditorWindowViewModel() : base(CommandTypeEnum.Chat) { }

        public override bool AddRequirementsToCommand { get { return true; } }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ACommandNameMustBeSpecified));
            }

            if (string.IsNullOrWhiteSpace(this.Triggers))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ChatCommandMissingTriggers));
            }

            if (!ChatCommandModel.IsValidCommandTrigger(this.Triggers))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ChatCommandInvalidTriggers));
            }

            HashSet<string> triggers = this.GetChatTriggers();
            if (this.IncludeExclamation)
            {
                triggers = new HashSet<string>(triggers.Select(t => "!" + t));
            }

            foreach (ChatCommandModel command in ServiceManager.Get<CommandService>().AllEnabledChatAccessibleCommands)
            {
                if (this.existingCommand != command)
                {
                    foreach (string trigger in command.GetFullTriggers())
                    {
                        if (triggers.Contains(trigger))
                        {
                            return Task.FromResult(new Result(string.Format(MixItUp.Base.Resources.ChatCommandTriggerAlreadyExists, trigger)));
                        }
                    }
                }
            }

            return Task.FromResult(new Result());
        }

        public override Task<CommandModelBase> CreateNewCommand()
        {
            return Task.FromResult<CommandModelBase>(new ChatCommandModel(this.Name, this.GetChatTriggers(), this.IncludeExclamation, this.Wildcards));
        }

        public override async Task UpdateExistingCommand(CommandModelBase command)
        {
            await base.UpdateExistingCommand(command);
            ChatCommandModel cCommand = (ChatCommandModel)command;
            cCommand.Triggers = this.GetChatTriggers();
            cCommand.IncludeExclamation = this.IncludeExclamation;
            cCommand.Wildcards = this.Wildcards;
        }

        public override Task SaveCommandToSettings(CommandModelBase command)
        {
            ServiceManager.Get<CommandService>().ChatCommands.Remove((ChatCommandModel)this.existingCommand);
            ServiceManager.Get<CommandService>().ChatCommands.Add((ChatCommandModel)command);
            ServiceManager.Get<ChatService>().RebuildCommandTriggers();
            return Task.CompletedTask;
        }

        protected HashSet<string> GetChatTriggers()
        {
            char[] triggerSeparator = new char[] { ' ' };
            if (this.Triggers.Contains(';'))
            {
                triggerSeparator = new char[] { ';' };
            }

            var triggers = this.Triggers.Split(triggerSeparator, StringSplitOptions.RemoveEmptyEntries).Select(s => s);
            if (this.IncludeExclamation)
            {
                triggers = triggers.Select(s => s.StartsWith("!") ? s.Substring(1) : s);
            }

            return new HashSet<string>(triggers);
        }

        public override Dictionary<string, string> GetTestSpecialIdentifiers() { return ChatCommandModel.GetChatTestSpecialIdentifiers(); }
    }

    public class UserOnlyChatCommandEditorWindowViewModel : ChatCommandEditorWindowViewModel
    {
        public override bool ShowCommandGroupSelector { get { return false; } }

        public Guid UserID
        {
            get { return this.userID; }
            set
            {
                this.userID = value;
                this.NotifyPropertyChanged();
            }
        }
        private Guid userID;

        public UserOnlyChatCommandEditorWindowViewModel(UserOnlyChatCommandModel existingCommand)
            : base(existingCommand)
        {
            this.UserID = existingCommand.UserID;
        }

        public UserOnlyChatCommandEditorWindowViewModel(Guid userID) : base(CommandTypeEnum.UserOnlyChat) { this.UserID = userID; }

        public override Task<CommandModelBase> CreateNewCommand()
        {
            return Task.FromResult<CommandModelBase>(new UserOnlyChatCommandModel(this.Name, this.GetChatTriggers(), this.IncludeExclamation, this.Wildcards, this.UserID));
        }

        public override Task SaveCommandToSettings(CommandModelBase command)
        {
            return Task.CompletedTask;
        }
    }
}

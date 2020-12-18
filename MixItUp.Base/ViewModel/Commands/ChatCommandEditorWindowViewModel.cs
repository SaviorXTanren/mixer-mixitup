using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Commands
{
    public class ChatCommandEditorWindowViewModel : CommandEditorWindowViewModelBase
    {
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
            }
            else
            {
                this.Triggers = string.Join(" ", existingCommand.Triggers);
            }
            this.IncludeExclamation = existingCommand.IncludeExclamation;
            this.Wildcards = existingCommand.Wildcards;
        }

        public ChatCommandEditorWindowViewModel() : base() { }

        public override bool AddRequirementsToCommand { get { return true; } }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ACommandNameMustBeSpecified));
            }

            if (string.IsNullOrEmpty(this.Triggers))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ChatCommandMissingTriggers));
            }

            if (!ChatCommandModel.IsValidCommandTrigger(this.Triggers))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ChatCommandInvalidTriggers));
            }

            return Task.FromResult(new Result());
        }

        public override Task<CommandModelBase> GetCommand()
        {
            return Task.FromResult<CommandModelBase>(new ChatCommandModel(this.Name, this.GetChatTriggers(), this.IncludeExclamation, this.Wildcards));
        }

        public override Task SaveCommandToSettings(CommandModelBase command)
        {
            ChatCommandModel c = (ChatCommandModel)command;
            ChannelSession.ChatCommands.Remove(c);
            ChannelSession.ChatCommands.Add(c);
            ChannelSession.Services.Chat.RebuildCommandTriggers();
            return Task.FromResult(0);
        }

        protected HashSet<string> GetChatTriggers()
        {
            char[] triggerSeparator = new char[] { ' ' };
            if (this.Triggers.Contains(';'))
            {
                triggerSeparator = new char[] { ';' };
            }
            return new HashSet<string>(this.Triggers.Split(triggerSeparator, StringSplitOptions.RemoveEmptyEntries));
        }
    }

    public class UserOnlyChatCommandEditorWindowViewModel : ChatCommandEditorWindowViewModel
    {
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

        public UserOnlyChatCommandEditorWindowViewModel(Guid userID) : base() { this.UserID = userID; }

        public override Task<CommandModelBase> GetCommand()
        {
            return Task.FromResult<CommandModelBase>(new UserOnlyChatCommandModel(this.Name, this.GetChatTriggers(), this.IncludeExclamation, this.Wildcards, this.UserID));
        }

        public override Task SaveCommandToSettings(CommandModelBase command)
        {
            return Task.FromResult(0);
        }
    }
}

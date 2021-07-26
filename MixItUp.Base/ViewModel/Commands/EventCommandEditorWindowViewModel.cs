using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Commands
{
    public class EventCommandEditorWindowViewModel : CommandEditorWindowViewModelBase
    {
        public EventTypeEnum EventType { get; private set; }

        public EventCommandEditorWindowViewModel(EventCommandModel existingCommand)
            : base(existingCommand)
        {
            this.EventType = existingCommand.EventType;
            this.Name = EnumLocalizationHelper.GetLocalizedName(this.EventType);
        }

        public EventCommandEditorWindowViewModel(EventTypeEnum eventType)
            : base(CommandTypeEnum.Event)
        {
            this.EventType = eventType;
            this.Name = EnumLocalizationHelper.GetLocalizedName(this.EventType);
        }

        public override Task<Result> Validate()
        {
            return Task.FromResult(new Result());
        }

        public override Task<CommandModelBase> CreateNewCommand() { return Task.FromResult<CommandModelBase>(new EventCommandModel(this.EventType)); }

        public override async Task UpdateExistingCommand(CommandModelBase command)
        {
            await base.UpdateExistingCommand(command);
            EventCommandModel eCommand = (EventCommandModel)command;
            eCommand.EventType = this.EventType;
        }

        public override Task SaveCommandToSettings(CommandModelBase command)
        {
            ServiceManager.Get<CommandService>().EventCommands.Remove((EventCommandModel)this.existingCommand);
            ServiceManager.Get<CommandService>().EventCommands.Add((EventCommandModel)command);
            return Task.CompletedTask;
        }

        public override Dictionary<string, string> GetTestSpecialIdentifiers() { return EventCommandModel.GetEventTestSpecialIdentifiers(this.EventType); }
    }
}

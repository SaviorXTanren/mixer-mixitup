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

        public override Task<CommandModelBase> GetCommand() { return Task.FromResult<CommandModelBase>(new EventCommandModel(this.EventType)); }

        public override Task SaveCommandToSettings(CommandModelBase command)
        {
            ChannelSession.EventCommands.Remove((EventCommandModel)this.existingCommand);
            ChannelSession.EventCommands.Add((EventCommandModel)command);
            return Task.FromResult(0);
        }

        public override Dictionary<string, string> GetTestSpecialIdentifiers() { return EventCommandModel.GetEventTestSpecialIdentifiers(this.EventType); }
    }
}

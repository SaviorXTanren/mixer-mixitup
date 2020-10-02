using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Window.Commands
{
    public class EventCommandEditorWindowViewModel : CommandEditorWindowViewModelBase
    {
        public EventTypeEnum EventType { get; private set; }

        public EventCommandEditorWindowViewModel(EventCommandModel existingCommand)
            : base(existingCommand)
        {
            this.Name = EnumLocalizationHelper.GetLocalizedName(this.EventType);
        }

        public EventCommandEditorWindowViewModel(EventTypeEnum eventType)
            : base()
        {
            this.EventType = eventType;
            this.Name = EnumLocalizationHelper.GetLocalizedName(this.EventType);
        }

        public override Task<Result> Validate()
        {
            return Task.FromResult(new Result());
        }

        public override Task<CommandModelBase> GetCommand() { return Task.FromResult<CommandModelBase>(new EventCommandModel(this.EventType)); }
    }
}

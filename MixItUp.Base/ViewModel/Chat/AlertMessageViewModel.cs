using MixItUp.Base.Model;
using MixItUp.Base.ViewModels;
using System;

namespace MixItUp.Base.ViewModel.Chat
{
    public class AlertMessageViewModel : ViewModelBase
    {
        public StreamingPlatformTypeEnum Platform { get; private set; }

        public string Message { get; private set; }

        public string Color { get; private set; }

        public DateTimeOffset Timestamp { get; protected set; } = DateTimeOffset.Now;

        public AlertMessageViewModel(StreamingPlatformTypeEnum platform, string message, string color = null)
        {
            this.Platform = platform;
            this.Message = message;
            this.Color = color;
        }

        public override string ToString()
        {
            return this.Message;
        }
    }
}

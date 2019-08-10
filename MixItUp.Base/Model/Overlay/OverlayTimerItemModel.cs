using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayTimerItemModel : OverlayHTMLTemplateItemModelBase, IDisposable
    {
        public const string TimerCompleteCommandName = "Timer Complete";

        public const string HTMLTemplate = @"<p style=""position: absolute; font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(-50%, -50%);"">{TIME}</p>";

        [DataMember]
        public int TotalLength { get; set; }

        [DataMember]
        public string TextColor { get; set; }
        [DataMember]
        public string TextFont { get; set; }
        [DataMember]
        public int TextSize { get; set; }

        [DataMember]
        public CustomCommand TimerCompleteCommand { get; set; }

        [JsonIgnore]
        private int timeLeft;

        [JsonIgnore]
        private CancellationTokenSource backgroundThreadCancellationTokenSource = new CancellationTokenSource();

        public OverlayTimerItemModel() : base() { }

        public OverlayTimerItemModel(string html, int totalLength, string textColor, string textFont, int textSize, CustomCommand timerCompleteCommand)
            : base(OverlayItemModelTypeEnum.Timer, html)
        {
            this.TotalLength = totalLength;
            this.TextColor = textColor;
            this.TextFont = textFont;
            this.TextSize = textSize;
            this.TimerCompleteCommand = timerCompleteCommand;
        }

        public override async Task Initialize()
        {
            this.timeLeft = this.TotalLength;

            this.backgroundThreadCancellationTokenSource = new CancellationTokenSource();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () => { await this.TimerBackground(); }, this.backgroundThreadCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            await base.Initialize();
        }

        public override async Task Disable()
        {
            this.timeLeft = this.TotalLength;

            if (this.backgroundThreadCancellationTokenSource != null)
            {
                this.backgroundThreadCancellationTokenSource.Cancel();
                this.backgroundThreadCancellationTokenSource = null;
            }
            await base.Disable();
        }

        protected override async Task<Dictionary<string, string>> GetTemplateReplacements(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            Dictionary<string, string> replacementSets = await base.GetTemplateReplacements(user, arguments, extraSpecialIdentifiers);

            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["TEXT_SIZE"] = this.TextSize.ToString();
            replacementSets["TIME"] = TimeSpan.FromSeconds(this.timeLeft).ToString("hh\\:mm\\:ss");

            return replacementSets;
        }

        private async Task TimerBackground()
        {
            try
            {
                while (this.timeLeft > 0)
                {
                    this.SendUpdateRequired();

                    await Task.Delay(1000);

                    this.timeLeft--;
                }

                if (this.IsInitialized && !this.backgroundThreadCancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (this.TimerCompleteCommand != null)
                    {
                        await this.TimerCompleteCommand.Perform();
                    }
                    await this.Disable();
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    this.backgroundThreadCancellationTokenSource.Dispose();
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}

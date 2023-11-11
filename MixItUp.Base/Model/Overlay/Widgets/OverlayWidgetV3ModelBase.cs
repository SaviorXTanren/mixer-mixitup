using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay.Widgets
{
    [DataContract]
    public abstract class OverlayWidgetV3ModelBase
    {
        [DataMember]
        public OverlayItemV3ModelBase Item { get; set; }

        [DataMember]
        public Guid OverlayEndpointID { get; set; }

        [DataMember]
        public int RefreshTime { get; set; }
        [DataMember]
        public bool IsEnabled { get; set; }

        public Guid ID { get { return this.Item.ID; } }
        public string Name { get { return this.Item.Name; } }
        public OverlayItemV3Type Type { get { return this.Item.Type; } }

        public virtual bool IsTestable { get { return true; } }

        private CancellationTokenSource refreshCancellationTokenSource;

        public OverlayWidgetV3ModelBase(OverlayItemV3ModelBase item)
        {
            this.Item = item;
            this.IsEnabled = true;
        }

        [Obsolete]
        public OverlayWidgetV3ModelBase() { }

        public async Task Enable()
        {
            this.IsEnabled = true;

            await this.EnableInternal();

            OverlayEndpointV3Service overlay = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpointService(this.OverlayEndpointID);
            if (overlay != null)
            {
                //await overlay.Add(this.Item, new CommandParametersModel());
            }

            if (this.RefreshTime > 0)
            {
                if (this.refreshCancellationTokenSource != null)
                {
                    this.refreshCancellationTokenSource.Cancel();
                }
                this.refreshCancellationTokenSource = new CancellationTokenSource();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                {
                    do
                    {
                        await Task.Delay(1000 * RefreshTime);

                        //await this.CallFunction("Update", null, null);

                    } while (!cancellationToken.IsCancellationRequested);

                }, this.refreshCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        public async Task Disable()
        {
            await this.DisableInternal();

            if (this.refreshCancellationTokenSource != null)
            {
                this.refreshCancellationTokenSource.Cancel();
            }
            this.refreshCancellationTokenSource = null;

            OverlayEndpointV3Service overlay = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpointService(this.OverlayEndpointID);
            if (overlay != null)
            {
                //await overlay.Remove(this.Item);
            }

            this.IsEnabled = false;
        }

        public async Task Test(CommandParametersModel parameters)
        {
            await this.Enable();
            await this.TestInternal(parameters);
            await this.Disable();
        }

        protected virtual Task EnableInternal() { return Task.CompletedTask; }

        protected virtual Task DisableInternal() { return Task.CompletedTask; }

        protected virtual Task TestInternal(CommandParametersModel parameters) { return Task.CompletedTask; }

        protected async Task CallFunction(string functionName, Dictionary<string, string> data, CommandParametersModel parameters)
        {
            Dictionary<string, string> dataParameters = new Dictionary<string, string>();
            if (data != null)
            {
                foreach (var kvp in data)
                {
                    dataParameters[kvp.Key] = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(kvp.Value, parameters);
                }
            }

            OverlayEndpointV3Service overlay = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpointService(this.OverlayEndpointID);
            if (overlay != null)
            {
                //await overlay.Function(this.Item, functionName, dataParameters);
            }
        }
    }
}

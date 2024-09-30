using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [Obsolete]
    [DataContract]
    public class OverlayWidgetModel
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsEnabled { get; set; }

        [DataMember]
        public string OverlayName { get; set; }

        [DataMember]
        public OverlayItemModelBase Item { get; set; }

        [DataMember]
        public int RefreshTime { get; set; }

        [Obsolete]
        public OverlayWidgetModel() { }

        public OverlayWidgetModel(string name, string overlayName, OverlayItemModelBase item, int refreshTime)
        {
            this.Name = name;
            this.OverlayName = overlayName;
            this.Item = item;
            this.RefreshTime = refreshTime;
            this.IsEnabled = true;
        }

        [JsonIgnore]
        public virtual bool SupportsTestData { get { return (this.Item != null) ? this.Item.SupportsTestData : false; } }

        [JsonIgnore]
        public virtual bool SupportsRefreshUpdating { get { return (this.Item != null) ? this.Item.SupportsRefreshUpdating : false; } }

        public async Task LoadTestData()
        {
            if (this.SupportsTestData)
            {
                await this.Item.LoadTestData();
                await this.UpdateItem();
                await this.Item.Reset();
            }
        }

        public async Task Initialize() { await this.Item.Initialize(); }

        public async Task Enable() { await this.Enable(new CommandParametersModel()); }

        public async Task Enable(CommandParametersModel parameters)
        {
            this.IsEnabled = true;
            if (!this.Item.IsEnabled)
            {
                await this.Item.Enable();
                this.Item.OnChangeState += Item_OnChangeState;
                this.Item.OnSendUpdateRequired += Item_OnSendUpdateRequired;
                this.Item.OnHide += Item_OnHide;
            }
            await this.ShowItem();
        }

        public async Task Disable()
        {
            this.IsEnabled = false;
            if (this.Item.IsEnabled)
            {
                await this.Item.Disable();
                this.Item.OnChangeState -= Item_OnChangeState;
                this.Item.OnSendUpdateRequired -= Item_OnSendUpdateRequired;
                this.Item.OnHide -= Item_OnHide;
            }
            await this.HideItem();
        }

        public async Task LoadCachedData() { await this.Item.LoadCachedData(); }

        public async Task ShowItem() { await this.ShowItem(new CommandParametersModel()); }

        public async Task ShowItem(CommandParametersModel parameters)
        {
            OverlayEndpointService overlay = this.GetOverlay();
            if (overlay != null)
            {
                await overlay.ShowItem(this.Item, parameters);
            }
        }

        public async Task UpdateItem() { await this.UpdateItem(new CommandParametersModel()); }

        public async Task UpdateItem(CommandParametersModel parameters)
        {
            OverlayEndpointService overlay = this.GetOverlay();
            if (overlay != null)
            {
                await overlay.UpdateItem(this.Item, parameters);
            }
        }

        public async Task HideItem()
        {
            OverlayEndpointService overlay = this.GetOverlay();
            if (overlay != null)
            {
                await overlay.HideItem(this.Item);
            }
        }

        private OverlayEndpointService GetOverlay()
        {
            //string overlayName = (string.IsNullOrEmpty(this.OverlayName)) ? ServiceManager.Get<OverlayService>().DefaultOverlayName : this.OverlayName;
            //var overlays = ServiceManager.Get<OverlayService>().GetOverlayNames();
            //if (!overlays.Contains(overlayName))
            //{
            //    this.OverlayName = ServiceManager.Get<OverlayService>().DefaultOverlayName;
            //}
            //return ServiceManager.Get<OverlayService>().GetOverlay(overlayName);

            return null;
        }

        private async void Item_OnChangeState(object sender, bool state)
        {
            if (state)
            {
                await this.Enable();
            }
            else
            {
                await this.Disable();
            }
        }

        private async void Item_OnSendUpdateRequired(object sender, System.EventArgs e)
        {
            await this.UpdateItem();
        }

        private async void Item_OnHide(object sender, System.EventArgs e)
        {
            await this.HideItem();
        }
    }
}

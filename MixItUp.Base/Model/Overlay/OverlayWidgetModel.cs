using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
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
        public bool DontRefresh { get; set; }

        public OverlayWidgetModel()
        {
            this.IsEnabled = true;
        }

        public OverlayWidgetModel(string name, string overlayName, OverlayItemModelBase item, bool dontRefresh)
            : this()
        {
            this.Name = name;
            this.OverlayName = overlayName;
            this.Item = item;
            this.DontRefresh = dontRefresh;

            this.SetUpEventListener();
        }

        [JsonIgnore]
        public virtual bool SupportsTestData { get { return (this.Item != null) ? this.Item.SupportsTestData : false; } }

        public async Task LoadTestData()
        {
            if (this.SupportsTestData)
            {
                await this.Item.LoadTestData();
            }
        }

        public async Task ShowItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            IOverlayService overlay = this.GetOverlay();
            if (overlay != null)
            {
                await overlay.ShowItem(this.Item, user, arguments, extraSpecialIdentifiers);
            }
        }

        public async Task UpdateItem()
        {
            IOverlayService overlay = this.GetOverlay();
            if (overlay != null)
            {
                await overlay.UpdateItem(this.Item, await ChannelSession.GetCurrentUser(), new List<string>(), new Dictionary<string, string>());
            }
        }

        public async Task HideItem()
        {
            IOverlayService overlay = this.GetOverlay();
            if (overlay != null)
            {
                await overlay.HideItem(this.Item);
            }
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            this.SetUpEventListener();
        }

        private void SetUpEventListener()
        {
            this.Item.OnSendUpdateRequired += async (s, e) =>
            {
                await this.UpdateItem();
            };
        }

        private IOverlayService GetOverlay()
        {
            string overlayName = (string.IsNullOrEmpty(this.OverlayName)) ? ChannelSession.Services.OverlayServers.DefaultOverlayName : this.OverlayName;
            return ChannelSession.Services.OverlayServers.GetOverlay(overlayName);
        }
    }
}

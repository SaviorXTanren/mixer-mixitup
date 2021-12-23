using MixItUp.Base.Model.API;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for ChangelogControl.xaml
    /// </summary>
    public partial class ChangelogControl : MainControlBase
    {
        public ChangelogControl()
        {
            InitializeComponent();

            GlobalEvents.OnMainMenuStateChanged += GlobalEvents_OnMainMenuStateChanged;
        }

        protected override async Task InitializeInternal()
        {
            try
            {
                MixItUpUpdateModel update = await ServiceManager.Get<MixItUpService>().GetLatestUpdate();
                if (update != null)
                {
                    using (AdvancedHttpClient client = new AdvancedHttpClient())
                    {
                        string changelogHTML = await client.GetStringAsync(update.ChangelogLink);
                        this.ChangelogWebBrowser.NavigateToString(changelogHTML);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            await base.InitializeInternal();
        }

        private void GlobalEvents_OnMainMenuStateChanged(object sender, bool state)
        {
            this.ChangelogWebBrowser.Visibility = (state) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}

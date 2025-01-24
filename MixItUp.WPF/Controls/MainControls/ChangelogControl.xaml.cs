using MixItUp.Base.Model.API;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Net.Http;
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

            MainMenuControl.OnMainMenuStateChanged += MainMenuControl_OnMainMenuStateChanged;
        }

        protected override async Task InitializeInternal()
        {
            try
            {
                MixItUpUpdateModel update = await ServiceManager.Get<MixItUpService>().GetLatestUpdate();
                if (update != null)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        HttpResponseMessage response = await client.GetAsync(update.ChangelogLink);
                        if (response.IsSuccessStatusCode)
                        {
                            this.ChangelogWebBrowser.NavigateToString(await response.Content.ReadAsStringAsync());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            await base.InitializeInternal();
        }

        private void MainMenuControl_OnMainMenuStateChanged(object sender, bool state)
        {
            this.ChangelogWebBrowser.Visibility = (state) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}

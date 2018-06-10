using MixItUp.Base;
using MixItUp.Base.Model.Store;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Store
{
    /// <summary>
    /// Interaction logic for MainStoreControl.xaml
    /// </summary>
    public partial class MainStoreControl : LoadingControlBase
    {
        private CommandWindow window;

        private StoreDetailListingModel currentListing = null;

        public MainStoreControl(CommandWindow window)
        {
            this.window = window;

            InitializeComponent();
        }

        public async Task StoreListingSelected(StoreListingModel storeListing)
        {
            this.LandingGrid.Visibility = Visibility.Collapsed;
            this.DetailsGrid.Visibility = Visibility.Visible;

            this.RateReviewButton.Visibility = (storeListing.IsCommandOwnedByUser) ? Visibility.Collapsed : Visibility.Visible;
            this.ReportButton.Visibility = (storeListing.IsCommandOwnedByUser) ? Visibility.Collapsed : Visibility.Visible;
            this.RemoveButton.Visibility = (storeListing.IsCommandOwnedByUser) ? Visibility.Visible : Visibility.Collapsed;

            this.DetailsGrid.DataContext = this.currentListing = await ChannelSession.Services.MixItUpService.GetStoreListing(storeListing.ID);
        }

        protected override async Task OnLoaded()
        {
            this.PromotedCommandControl.Content = new LargeCommandLisingControl(this, await ChannelSession.Services.MixItUpService.GetTopRandomStoreListings());

            this.CreateAndAddCategory("Chat", await ChannelSession.Services.MixItUpService.GetTopStoreListingsForTag("Chat"));
            this.CreateAndAddCategory("Donations", await ChannelSession.Services.MixItUpService.GetTopStoreListingsForTag("Donations"));
            this.CreateAndAddCategory("Overlay", await ChannelSession.Services.MixItUpService.GetTopStoreListingsForTag("Overlay"));

            await base.OnLoaded();
        }

        private void CreateAndAddCategory(string categoryName, IEnumerable<StoreListingModel> storeListings)
        {
            List<StoreListingControl> storeListingControls = new List<StoreListingControl>();
            foreach (StoreListingModel storeListing in storeListings)
            {
                storeListingControls.Add(new SmallCommandListingControl(this, storeListing));
            }
            CategoryCommandListingControl category1 = new CategoryCommandListingControl(this, categoryName, storeListingControls);
            this.CategoriesStackPanel.Children.Add(category1);
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            this.window.DownloadCommandFromStore(this.currentListing);
        }

        private void RateReviewButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            await this.window.RunAsyncOperation(async () =>
            {
                if (await MessageBoxHelper.ShowConfirmationDialog("This will remove your command from the Mix It Up store." + Environment.NewLine + "Are you sure you wish to do this?"))
                {
                    await ChannelSession.Services.MixItUpService.DeleteStoreListing(this.currentListing);
                    this.window.Close();
                };
            });
        }
    }
}

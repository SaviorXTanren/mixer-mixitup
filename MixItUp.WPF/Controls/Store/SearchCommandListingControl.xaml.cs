using MixItUp.Base.Model.Store;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Store
{
    /// <summary>
    /// Interaction logic for SearchCommandListingControl.xaml
    /// </summary>
    public partial class SearchCommandListingControl : StoreListingControl
    {
        public SearchCommandListingControl(MainStoreControl mainStoreControl, StoreListingModel listing)
            : base(mainStoreControl, listing)
        {
            InitializeComponent();
        }

        protected override Task OnLoaded()
        {
            return base.OnLoaded();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.listing != null)
            {
                await this.mainStoreControl.StoreListingSelected(this.listing);
            }
        }
    }
}

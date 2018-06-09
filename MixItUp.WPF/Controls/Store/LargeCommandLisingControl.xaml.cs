using MixItUp.Base.Model.Store;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Store
{
    /// <summary>
    /// Interaction logic for LargeCommandLisingControl.xaml
    /// </summary>
    public partial class LargeCommandLisingControl : StoreListingControl
    {
        public LargeCommandLisingControl(MainStoreControl mainStoreControl, StoreListingModel listing)
            : base(mainStoreControl, listing)
        {
            InitializeComponent();
        }

        protected override Task OnLoaded()
        {
            return base.OnLoaded();
        }

        private async void Button_Click(object sender, RoutedEventArgs e) { await this.mainStoreControl.StoreListingSelected(this.listing); }
    }
}

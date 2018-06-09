using System.Threading.Tasks;
using System.Windows;
using MixItUp.Base.Model.Store;

namespace MixItUp.WPF.Controls.Store
{
    /// <summary>
    /// Interaction logic for SmallCommandListingControl.xaml
    /// </summary>
    public partial class SmallCommandListingControl : StoreListingControl
    {
        public SmallCommandListingControl(MainStoreControl mainStoreControl, StoreListingModel listing)
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

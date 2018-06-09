using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Store
{
    /// <summary>
    /// Interaction logic for CategoryCommandListingControl.xaml
    /// </summary>
    public partial class CategoryCommandListingControl : LoadingControlBase
    {
        private MainStoreControl mainStoreControl;
        private string categoryName;
        private IEnumerable<StoreListingControl> storeListing;

        public CategoryCommandListingControl(MainStoreControl mainStoreControl, string categoryName, IEnumerable<StoreListingControl> storeListing)
        {
            this.mainStoreControl = mainStoreControl;
            this.categoryName = categoryName;
            this.storeListing = storeListing;

            InitializeComponent();
        }

        protected override Task OnLoaded()
        {
            this.CategoryNameTextBlock.Text = this.categoryName;

            if (this.storeListing.Count() > 0) { this.Listing1.Content = this.storeListing.ElementAt(0); }
            if (this.storeListing.Count() > 1) { this.Listing2.Content = this.storeListing.ElementAt(1); }
            if (this.storeListing.Count() > 2) { this.Listing3.Content = this.storeListing.ElementAt(2); }
            if (this.storeListing.Count() > 3) { this.Listing4.Content = this.storeListing.ElementAt(3); }
            if (this.storeListing.Count() > 4) { this.Listing5.Content = this.storeListing.ElementAt(4); }

            return base.OnLoaded();
        }
    }
}

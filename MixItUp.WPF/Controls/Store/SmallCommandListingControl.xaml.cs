using System.Threading.Tasks;
using MixItUp.Base.Model.Store;

namespace MixItUp.WPF.Controls.Store
{
    /// <summary>
    /// Interaction logic for SmallCommandListingControl.xaml
    /// </summary>
    public partial class SmallCommandListingControl : StoreListingControl
    {
        public SmallCommandListingControl(StoreListingModel listing)
            : base(listing)
        {
            InitializeComponent();
        }

        protected override Task OnLoaded()
        {
            this.ListingImage.Source = this.DisplayImage;

            return base.OnLoaded();
        }
    }
}

using MixItUp.Base.Model.Store;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Store
{
    /// <summary>
    /// Interaction logic for LargeCommandLisingControl.xaml
    /// </summary>
    public partial class LargeCommandLisingControl : StoreListingControl
    {
        public LargeCommandLisingControl(StoreListingModel listing)
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

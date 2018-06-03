using MixItUp.Base.Model.Store;
using MixItUp.WPF.Controls.Store;
using System.Threading.Tasks;

namespace MixItUp.WPF.Windows.Store
{
    /// <summary>
    /// Interaction logic for StoreWindow.xaml
    /// </summary>
    public partial class StoreWindow : LoadingWindowBase
    {
        public StoreWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override Task OnLoaded()
        {
            this.Blah.Content = new LargeCommandLisingControl(new StoreListingModel() { Name = "Really cool command!", Description = "This is a really cool comamnd that I designed that you are sure to love! It has all the coolest things in the world you could ever want in a command. So why have you not downloaded it already? Seriously, what is wrong with you? What were you even thinking not doing something like that, come on!", AverageRating = 3.8 });

            this.Blah2.Content = new SmallCommandListingControl(new StoreListingModel() { Name = "Really cool command!", Description = "This is a really cool comamnd that I designed that you are sure to love! It has all the coolest things in the world you could ever want in a command. So why have you not downloaded it already? Seriously, what is wrong with you? What were you even thinking not doing something like that, come on!", AverageRating = 3.8 });

            return base.OnLoaded();
        }
    }
}

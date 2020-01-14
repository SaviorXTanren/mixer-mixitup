using MixItUp.WPF.Windows;
using System;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for ServiceContainerControl.xaml
    /// </summary>
    public partial class ServiceContainerControl : LoadingControlBase
    {
        private const int MinimizedGroupBoxHeight = 34;

        public LoadingWindowBase window { get; private set; }
        private ServiceControlBase serviceControl;

        public ServiceContainerControl(LoadingWindowBase window, ServiceControlBase serviceControl)
        {
            this.window = window;
            this.serviceControl = serviceControl;
            this.serviceControl.Initialize(this);
            this.DataContext = this.serviceControl.ViewModel;

            InitializeComponent();

            this.InnerContentControl.Content = serviceControl;
        }

        public void Minimize() { this.GroupBox.Height = MinimizedGroupBoxHeight; }

        public void GroupBoxHeader_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.GroupBox.Height == MinimizedGroupBoxHeight)
            {
                this.GroupBox.Height = Double.NaN;
            }
            else
            {
                this.Minimize();
            }
        }

        protected override Task OnLoaded()
        {
            this.Minimize();
            return base.OnLoaded();
        }
    }
}

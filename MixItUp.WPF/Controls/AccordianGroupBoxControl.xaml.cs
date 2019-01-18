using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MixItUp.WPF.Controls
{
    /// <summary>
    /// Interaction logic for AccordianGroupBoxControl.xaml
    /// </summary>
    public partial class AccordianGroupBoxControl : GroupBox
    {
        private const int MinimizedGroupBoxHeight = 34;

        public event RoutedEventHandler Maximized;
        public event RoutedEventHandler Minimized;

        public AccordianGroupBoxControl()
        {
            InitializeComponent();
        }

        public bool IsMinimized { get { return this.Height == MinimizedGroupBoxHeight; } }

        public void Minimize()
        {
            this.Height = MinimizedGroupBoxHeight;
            this.Minimized?.Invoke(this, new RoutedEventArgs());
        }

        public void Maximize()
        {
            this.Height = Double.NaN;
            this.Maximized?.Invoke(this, new RoutedEventArgs());
        }

        protected override void OnHeaderChanged(object oldHeader, object newHeader)
        {
            base.OnHeaderChanged(oldHeader, newHeader);
            FrameworkElement header = (FrameworkElement)newHeader;
            if (header != null)
            {
                header.MouseLeftButtonUp -= Header_MouseLeftButtonUp;
                header.MouseLeftButtonUp += Header_MouseLeftButtonUp;
            }
        }

        private void Header_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.IsMinimized)
            {
                this.Maximize();
            }
            else
            {
                this.Minimize();
            }
        }
    }
}

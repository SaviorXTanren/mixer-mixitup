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

        // Using a DependencyProperty as the backing store for IsMinimized.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMinimizedProperty =
            DependencyProperty.Register("IsMinimized", typeof(bool), typeof(AccordianGroupBoxControl), new PropertyMetadata(false));

        public AccordianGroupBoxControl()
        {
            InitializeComponent();

            this.Loaded += AccordianGroupBoxControl_Loaded;
        }

        public bool IsMinimized
        {
            get { return (bool)GetValue(IsMinimizedProperty); }
            set { SetValue(IsMinimizedProperty, value); }
        }

        public bool IsUIMinimized { get { return this.Height == MinimizedGroupBoxHeight; } }

        public void Minimize()
        {
            this.Height = MinimizedGroupBoxHeight;
            this.IsMinimized = true;
            this.Minimized?.Invoke(this, new RoutedEventArgs());
        }

        public void Maximize()
        {
            this.Height = Double.NaN;
            this.IsMinimized = false;
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

        private void AccordianGroupBoxControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.IsMinimized)
            {
                this.Minimize();
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

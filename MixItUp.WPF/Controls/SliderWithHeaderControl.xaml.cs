using System;

namespace MixItUp.WPF.Controls
{
    /// <summary>
    /// Interaction logic for SliderWithHeaderControl.xaml
    /// </summary>
    public partial class SliderWithHeaderControl : NotifyPropertyChangedUserControl
    {
        public SliderWithHeaderControl()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Slider.ValueChanged += Slider_ValueChanged;
        }

        public string Text
        {
            get { return this.text; }
            set
            {
                this.text = value;
                this.NotifyPropertyChanged("Text");
            }
        }
        private string text;

        public int Minimum
        {
            get { return this.minimum; }
            set
            {
                this.minimum = value;
                this.NotifyPropertyChanged("Minimum");
            }
        }
        private int minimum;

        public int Maximum
        {
            get { return this.maximum; }
            set
            {
                this.maximum = value;
                this.NotifyPropertyChanged("Maximum");
            }
        }
        private int maximum;

        public int Value
        {
            get { return (int)this.Slider.Value; }
            set { this.Slider.Value = value; }
        }

        public event EventHandler<int> ValueChanged;

        private void Slider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.ValueChanged != null)
            {
                this.ValueChanged(this, this.Value);
            }
        }
    }
}

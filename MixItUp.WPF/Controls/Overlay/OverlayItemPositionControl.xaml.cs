using MixItUp.Base.Model.Overlay;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayItemPositionControl.xaml
    /// </summary>
    public partial class OverlayItemPositionControl : LoadingControlBase
    {
        private OverlayItemPosition position;

        public OverlayItemPositionControl()
        {
            InitializeComponent();
        }

        public OverlayItemPositionControl(OverlayItemPosition position)
            : this()
        {
            this.position = position;
        }

        public void SetPosition(OverlayItemPosition position)
        {
            this.position = position;
            if (this.position != null)
            {
                if (this.position.PositionType == OverlayEffectPositionType.Percentage)
                {
                    if (this.position.Horizontal == 25 && this.position.Vertical == 25) { this.TopLeftPositionButton_Click(this, new RoutedEventArgs()); }
                    else if (this.position.Horizontal == 50 && this.position.Vertical == 25) { this.TopPositionButton_Click(this, new RoutedEventArgs()); }
                    else if (this.position.Horizontal == 75 && this.position.Vertical == 25) { this.TopRightPositionButton_Click(this, new RoutedEventArgs()); }
                    else if (this.position.Horizontal == 25 && this.position.Vertical == 50) { this.LeftPositionButton_Click(this, new RoutedEventArgs()); }
                    else if (this.position.Horizontal == 50 && this.position.Vertical == 50) { this.CenterPositionButton_Click(this, new RoutedEventArgs()); }
                    else if (this.position.Horizontal == 75 && this.position.Vertical == 50) { this.RightPositionButton_Click(this, new RoutedEventArgs()); }
                    else if (this.position.Horizontal == 25 && this.position.Vertical == 75) { this.BottomLeftPositionButton_Click(this, new RoutedEventArgs()); }
                    else if (this.position.Horizontal == 50 && this.position.Vertical == 75) { this.BottomPositionButton_Click(this, new RoutedEventArgs()); }
                    else if (this.position.Horizontal == 75 && this.position.Vertical == 75) { this.BottomRightPositionButton_Click(this, new RoutedEventArgs()); }
                    else
                    {
                        this.PercentagePositionButton_Click(this, new RoutedEventArgs());
                    }

                    this.PercentagePositionHorizontalSlider.Value = this.position.Horizontal;
                    this.PercentagePositionVerticalSlider.Value = this.position.Vertical;
                }
                else
                {
                    this.PixelsPositionButton_Click(this, new RoutedEventArgs());

                    this.PixelPositionHorizontalTextBox.Text = this.position.Horizontal.ToString();
                    this.PixelPositionVerticalTextBox.Text = this.position.Vertical.ToString();
                }
            }
        }

        public OverlayItemPosition GetPosition()
        {
            int horizontal = 0;
            int vertical = 0;
            OverlayEffectPositionType positionType = OverlayEffectPositionType.Percentage;
            if (this.PixelsPositionGrid.Visibility == Visibility.Visible)
            {
                if (string.IsNullOrEmpty(this.PixelPositionHorizontalTextBox.Text) || !int.TryParse(this.PixelPositionHorizontalTextBox.Text, out horizontal) || horizontal < 0 ||
                    string.IsNullOrEmpty(this.PixelPositionVerticalTextBox.Text) || !int.TryParse(this.PixelPositionVerticalTextBox.Text, out vertical) || vertical < 0)
                {
                    return null;
                }
                positionType = OverlayEffectPositionType.Pixel;
            }
            else if (this.PercentagePositionGrid.Visibility == Visibility.Visible)
            {
                horizontal = (int)this.PercentagePositionHorizontalSlider.Value;
                vertical = (int)this.PercentagePositionVerticalSlider.Value;
            }
            else
            {
                if (this.IsSimplePositionButtonSelected(this.TopLeftPositionButton))
                {
                    horizontal = 25;
                    vertical = 25;
                }
                else if (this.IsSimplePositionButtonSelected(this.TopPositionButton))
                {
                    horizontal = 50;
                    vertical = 25;
                }
                else if (this.IsSimplePositionButtonSelected(this.TopRightPositionButton))
                {
                    horizontal = 75;
                    vertical = 25;
                }
                else if (this.IsSimplePositionButtonSelected(this.LeftPositionButton))
                {
                    horizontal = 25;
                    vertical = 50;
                }
                else if (this.IsSimplePositionButtonSelected(this.CenterPositionButton))
                {
                    horizontal = 50;
                    vertical = 50;
                }
                else if (this.IsSimplePositionButtonSelected(this.RightPositionButton))
                {
                    horizontal = 75;
                    vertical = 50;
                }
                else if (this.IsSimplePositionButtonSelected(this.BottomLeftPositionButton))
                {
                    horizontal = 25;
                    vertical = 75;
                }
                else if (this.IsSimplePositionButtonSelected(this.BottomPositionButton))
                {
                    horizontal = 50;
                    vertical = 75;
                }
                else if (this.IsSimplePositionButtonSelected(this.BottomRightPositionButton))
                {
                    horizontal = 75;
                    vertical = 75;
                }
            }
            return new OverlayItemPosition(positionType, horizontal, vertical);
        }

        protected override Task OnLoaded()
        {
            this.SimplePositionButton_Click(this, new RoutedEventArgs());
            this.CenterPositionButton_Click(this, new RoutedEventArgs());

            this.PixelPositionHorizontalTextBox.Text = "0";
            this.PixelPositionVerticalTextBox.Text = "0";

            if (this.position != null)
            {
                this.SetPosition(position);
            }
            return Task.FromResult(0);
        }

        private void SimplePositionButton_Click(object sender, RoutedEventArgs e)
        {
            this.SimplePositionButton.IsEnabled = false;
            this.PercentagePositionButton.IsEnabled = true;
            this.PixelsPositionButton.IsEnabled = true;

            this.SimplePositionGrid.Visibility = Visibility.Visible;
            this.PercentagePositionGrid.Visibility = Visibility.Collapsed;
            this.PixelsPositionGrid.Visibility = Visibility.Collapsed;
        }

        private void PercentagePositionButton_Click(object sender, RoutedEventArgs e)
        {
            this.SimplePositionButton.IsEnabled = true;
            this.PercentagePositionButton.IsEnabled = false;
            this.PixelsPositionButton.IsEnabled = true;

            this.SimplePositionGrid.Visibility = Visibility.Collapsed;
            this.PercentagePositionGrid.Visibility = Visibility.Visible;
            this.PixelsPositionGrid.Visibility = Visibility.Collapsed;
        }

        private void PixelsPositionButton_Click(object sender, RoutedEventArgs e)
        {
            this.SimplePositionButton.IsEnabled = true;
            this.PercentagePositionButton.IsEnabled = true;
            this.PixelsPositionButton.IsEnabled = false;

            this.SimplePositionGrid.Visibility = Visibility.Collapsed;
            this.PercentagePositionGrid.Visibility = Visibility.Collapsed;
            this.PixelsPositionGrid.Visibility = Visibility.Visible;
        }

        private void TopLeftPositionButton_Click(object sender, RoutedEventArgs e) { this.HandleSimplePositionChange(this.TopLeftPositionButton); }

        private void TopPositionButton_Click(object sender, RoutedEventArgs e) { this.HandleSimplePositionChange(this.TopPositionButton); }

        private void TopRightPositionButton_Click(object sender, RoutedEventArgs e) { this.HandleSimplePositionChange(this.TopRightPositionButton); }

        private void LeftPositionButton_Click(object sender, RoutedEventArgs e) { this.HandleSimplePositionChange(this.LeftPositionButton); }

        private void CenterPositionButton_Click(object sender, RoutedEventArgs e) { this.HandleSimplePositionChange(this.CenterPositionButton); }

        private void RightPositionButton_Click(object sender, RoutedEventArgs e) { this.HandleSimplePositionChange(this.RightPositionButton); }

        private void BottomLeftPositionButton_Click(object sender, RoutedEventArgs e) { this.HandleSimplePositionChange(this.BottomLeftPositionButton); }

        private void BottomPositionButton_Click(object sender, RoutedEventArgs e) { this.HandleSimplePositionChange(this.BottomPositionButton); }

        private void BottomRightPositionButton_Click(object sender, RoutedEventArgs e) { this.HandleSimplePositionChange(this.BottomRightPositionButton); }

        private void HandleSimplePositionChange(Button button)
        {
            this.TopLeftPositionButton.Style = (Style)this.FindResource("MaterialDesignRaisedButton");
            this.TopPositionButton.Style = (Style)this.FindResource("MaterialDesignRaisedButton");
            this.TopRightPositionButton.Style = (Style)this.FindResource("MaterialDesignRaisedButton");
            this.LeftPositionButton.Style = (Style)this.FindResource("MaterialDesignRaisedButton");
            this.CenterPositionButton.Style = (Style)this.FindResource("MaterialDesignRaisedButton");
            this.RightPositionButton.Style = (Style)this.FindResource("MaterialDesignRaisedButton");
            this.BottomLeftPositionButton.Style = (Style)this.FindResource("MaterialDesignRaisedButton");
            this.BottomPositionButton.Style = (Style)this.FindResource("MaterialDesignRaisedButton");
            this.BottomRightPositionButton.Style = (Style)this.FindResource("MaterialDesignRaisedButton");

            button.Style = (Style)this.FindResource("MaterialDesignRaisedLightButton");
        }

        private bool IsSimplePositionButtonSelected(Button button) { return button.Style.Equals((Style)this.FindResource("MaterialDesignRaisedLightButton")); }
    }
}

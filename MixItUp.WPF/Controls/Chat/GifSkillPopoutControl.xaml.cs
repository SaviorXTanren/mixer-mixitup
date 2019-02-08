using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for GifSkillPopoutControl.xaml
    /// </summary>
    public partial class GifSkillPopoutControl : UserControl
    {
        public GifSkillPopoutControl()
        {
            InitializeComponent();
        }

        public void SlideInFromRight()
        {
            DoubleAnimation db = new DoubleAnimation();
            db.From = 0;
            db.To = -this.GifImage.ActualWidth;
            db.Duration = TimeSpan.FromSeconds(1);
            BorderAnimation.BeginAnimation(TranslateTransform.YProperty, db);
        }

        public void SlideOutToRight()
        {
            DoubleAnimation db = new DoubleAnimation();
            db.From = 0;
            db.To = this.GifImage.ActualWidth;
            db.Duration = TimeSpan.FromSeconds(1);
            BorderAnimation.BeginAnimation(TranslateTransform.YProperty, db);
        }
    }
}

using MixItUp.Base;
using MixItUp.Base.ViewModel.Chat.Mixer;
using System.Windows.Controls;
using System.Windows;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for GifSkillHoverControl.xaml
    /// </summary>
    public partial class GifSkillHoverControl : UserControl
    {
        public GifSkillHoverControl()
        {
            InitializeComponent();

            this.Loaded += GifSkillHoverControl_Loaded;
            this.DataContextChanged += GifSkillHoverControl_DataContextChanged;
        }

        private void GifSkillHoverControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.GifSkillHoverControl_DataContextChanged(this, new DependencyPropertyChangedEventArgs());
        }

        private void GifSkillHoverControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (this.IsLoaded && this.DataContext != null && this.DataContext is MixerSkillChatMessageViewModel)
            {
                MixerSkillChatMessageViewModel skillMessage = (MixerSkillChatMessageViewModel)this.DataContext;
                this.GifSkillPopup.DataContext = skillMessage.Skill.Image;
                this.GifSkillPopup.Visibility = Visibility.Visible;
                this.GifSkillIcon.Height = this.GifSkillIcon.Width = ChannelSession.Settings.ChatFontSize * 2;
            }
        }
    }
}

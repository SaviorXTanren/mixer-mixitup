using MixItUp.Base.Model.Skill;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

            this.GifImage.SizeChanged += GifImage_SizeChanged;
        }

        public async Task ShowGif(SkillUsageModel skill)
        {
            this.DataContext = skill;

            await this.UserAvatar.SetUserAvatarUrl(skill.User);
            this.UserAvatar.SetSize(16);
           
            while (this.DataContext != null)
            {
                await Task.Delay(1000);
            }
        }

        private async void GifImage_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (this.DataContext != null && this.GifImage.Source.Height > 10)
            {
                this.GroupBox.Visibility = Visibility.Visible;
                await Task.Delay(9000);

                this.GroupBox.Visibility = Visibility.Hidden;
                this.DataContext = null;
            }
        }
    }
}

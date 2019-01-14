using MixItUp.Base;
using MixItUp.Base.Model.Skill;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for SkillControl.xaml
    /// </summary>
    public partial class SkillControl : UserControl
    {
        private static Dictionary<string, BitmapImage> skillBitmapImages = new Dictionary<string, BitmapImage>();

        public SkillInstanceModel Skill { get { return this.DataContext as SkillInstanceModel; } }

        public bool IsEmberSkill { get; private set; }

        public SkillControl()
        {
            this.DataContextChanged += SkillControl_DataContextChanged;

            InitializeComponent();
        }

        public SkillControl(SkillInstanceModel skill)
            : this()
        {
            this.DataContext = skill;
        }

        public SkillControl(SkillInstanceModel skill, bool isEmberSkill)
            : this(skill)
        {
            this.IsEmberSkill = isEmberSkill;
        }

        public void UpdateSizing()
        {
            this.SkillImage.Height = this.SkillImage.Width = ChannelSession.Settings.ChatFontSize * 2;
            this.GifSkillIcon.Height = this.GifSkillIcon.Width = ChannelSession.Settings.ChatFontSize * 2;
            this.SkillNameTextBlock.FontSize = ChannelSession.Settings.ChatFontSize;
            this.EmberIcon.Height = this.EmberIcon.Width = this.SparkIcon.Height = this.SparkIcon.Width = ChannelSession.Settings.ChatFontSize + 2;
            this.SkillCostTextBlock.FontSize = ChannelSession.Settings.ChatFontSize;
        }

        private async void SkillControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (this.Skill != null)
                {
                    if (this.Skill.IsGif)
                    {
                        this.GifSkillPopup.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        this.SkillImage.Visibility = Visibility.Visible;
                        if (!SkillControl.skillBitmapImages.ContainsKey(this.Skill.Skill.attributionIconUrl))
                        {
                            BitmapImage bitmap = new BitmapImage();
                            using (WebClient client = new WebClient())
                            {
                                string url = this.Skill.Skill.attributionIconUrl;
                                var bytes = await Task.Run<byte[]>(async () => { return await client.DownloadDataTaskAsync(url); });

                                bitmap.BeginInit();
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.StreamSource = new MemoryStream(bytes);
                                bitmap.EndInit();
                            }
                            SkillControl.skillBitmapImages[this.Skill.Skill.attributionIconUrl] = bitmap;
                        }

                        this.SkillImage.Source = SkillControl.skillBitmapImages[this.Skill.Skill.attributionIconUrl];
                    }

                    if (this.IsEmberSkill)
                    {
                        this.EmberIcon.Visibility = Visibility.Visible;
                        this.SparkIcon.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        this.EmberIcon.Visibility = Visibility.Collapsed;
                        this.SparkIcon.Visibility = Visibility.Visible;
                    }

                    this.UpdateSizing();
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }
    }
}

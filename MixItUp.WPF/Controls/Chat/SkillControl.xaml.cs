using Mixer.Base.Model.Skills;
using MixItUp.Base;
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

        public SkillModel Skill { get { return this.DataContext as SkillModel; } }

        public SkillControl()
        {
            this.DataContextChanged += SkillControl_DataContextChanged;
            InitializeComponent();
        }

        public SkillControl(SkillModel skill) : this()
        {
            InitializeComponent();
            this.DataContext = skill;
        }

        public void UpdateSizing()
        {
            this.SkillImage.Height = this.SkillImage.Width = ChannelSession.Settings.ChatFontSize * 2;
            this.SkillNameTextBlock.FontSize = ChannelSession.Settings.ChatFontSize;
            this.SparkIcon.Height = this.SparkIcon.Width = ChannelSession.Settings.ChatFontSize + 2;
            this.SkillCostTextBlock.FontSize = ChannelSession.Settings.ChatFontSize;
        }

        private async void SkillControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (this.Skill != null)
                {
                    string uri = Skill.attributionIconUrl;
                    if (!SkillControl.skillBitmapImages.ContainsKey(uri))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        using (WebClient client = new WebClient())
                        {
                            var bytes = await Task.Run<byte[]>(async () => { return await client.DownloadDataTaskAsync(uri); });

                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = new MemoryStream(bytes);
                            bitmap.EndInit();
                        }
                        SkillControl.skillBitmapImages[uri] = bitmap;
                    }

                    this.SkillImage.Source = SkillControl.skillBitmapImages[uri];

                    this.UpdateSizing();
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }
    }
}

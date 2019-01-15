using Mixer.Base.Model.Chat;
using MixItUp.Base;
using MixItUp.Base.MixerAPI.Models;
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
    /// Interaction logic for StickerControl.xaml
    /// </summary>
    public partial class StickerControl : UserControl
    {
        private static Dictionary<string, BitmapImage> stickerBitmapImages = new Dictionary<string, BitmapImage>();

        public ChatSkillModelWrapper Skill { get { return this.DataContext as ChatSkillModelWrapper; } }

        public StickerControl(ChatSkillModel skill)
        {
            this.DataContextChanged += StickerControl_DataContextChanged;

            InitializeComponent();

            this.DataContext = new ChatSkillModelWrapper(skill);
        }

        public void UpdateSizing()
        {
            this.StickerImage.Height = this.StickerImage.Width = ChannelSession.Settings.ChatFontSize * 3;
            this.EmberIcon.Height = this.EmberIcon.Width = this.SparkIcon.Height = this.SparkIcon.Width = ChannelSession.Settings.ChatFontSize + 2;
            this.StickerCostTextBlock.FontSize = ChannelSession.Settings.ChatFontSize;
        }

        private async void StickerControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (this.Skill != null)
                {
                    string uri = this.Skill.Skill.icon_url;
                    if (!StickerControl.stickerBitmapImages.ContainsKey(uri))
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
                        StickerControl.stickerBitmapImages[uri] = bitmap;
                    }

                    this.StickerImage.Source = StickerControl.stickerBitmapImages[uri];

                    this.UpdateSizing();
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }
    }
}

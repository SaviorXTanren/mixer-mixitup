using Mixer.Base.Model.Chat;
using MixItUp.Base;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.WPF.Properties;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatMessageControl.xaml
    /// </summary>
    public partial class ChatMessageControl : UserControl
    {
        private static Dictionary<string, BitmapImage> emoticonBitmapImages = new Dictionary<string, BitmapImage>();

        public ChatMessageViewModel Message { get; private set; }

        private ChatMessageHeaderControl messageHeader;
        private List<TextBlock> textBlocks = new List<TextBlock>();

        public ChatMessageControl(ChatMessageViewModel message)
        {
            this.Loaded += ChatMessageControl_Loaded;

            InitializeComponent();

            this.DataContext = this.Message = message;
            this.messageHeader = new ChatMessageHeaderControl(this.Message);
        }

        private void ChatMessageControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.MessageWrapPanel.Children.Clear();

            if (!this.Message.IsAlertMessage)
            {
                this.MessageWrapPanel.Children.Add(this.messageHeader);
            }

            foreach (ChatMessageDataModel messageData in this.Message.MessageComponents)
            {
                if (messageData.type.Equals("emoticon") && ChatMessageViewModel.EmoticonImages.ContainsKey(messageData.text))
                {
                    if (!ChatMessageControl.emoticonBitmapImages.ContainsKey(messageData.text))
                    {
                        ChatMessageControl.emoticonBitmapImages[messageData.text] = new BitmapImage(new Uri(ChatMessageViewModel.EmoticonImages[messageData.text].FilePath));
                    }

                    CoordinatesModel coords = ChatMessageViewModel.EmoticonImages[messageData.text].Coordinates;
                    CroppedBitmap bitmap = new CroppedBitmap(ChatMessageControl.emoticonBitmapImages[messageData.text], new Int32Rect((int)coords.x, (int)coords.y, (int)coords.width, (int)coords.height));

                    Image image = new Image();
                    image.Source = bitmap;
                    image.VerticalAlignment = VerticalAlignment.Center;
                    this.MessageWrapPanel.Children.Add(image);
                }
                else
                {
                    foreach (string word in messageData.text.Split(new string[] { " " }, StringSplitOptions.None))
                    {
                        TextBlock textBlock = new TextBlock();
                        textBlock.Text = word + " ";
                        textBlock.VerticalAlignment = VerticalAlignment.Center;
                        if (this.Message.IsAlertMessage)
                        {
                            textBlock.FontWeight = FontWeights.Bold;
                            textBlock.Foreground = (App.AppSettings.DarkTheme) ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Black);
                        }
                        this.textBlocks.Add(textBlock);
                        this.MessageWrapPanel.Children.Add(textBlock);
                    }
                }
            }

            this.UpdateSizing();
        }

        public void DeleteMessage(string reason = null)
        {
            this.messageHeader.DeleteMessage();
            if (!string.IsNullOrEmpty(reason))
            {
                this.Message.AddToMessage(" (Auto-Moderated: " + reason + ")");
                this.DataContext = null;
                this.DataContext = this.Message;
            }
            this.Message.IsDeleted = true;
            foreach (TextBlock textBlock in this.textBlocks)
            {
                textBlock.TextDecorations = TextDecorations.Strikethrough;
            }
        }

        public void UpdateSizing()
        {
            this.messageHeader.UpdateSizing();
            foreach (var item in this.MessageWrapPanel.Children)
            {
                if (item is TextBlock)
                {
                    TextBlock textBlock = (TextBlock)item;
                    textBlock.FontSize = ChannelSession.Settings.ChatFontSize;
                }
                else if (item is Image)
                {
                    Image image = (Image)item;
                    image.Height = image.Width = ChannelSession.Settings.ChatFontSize + 2;
                }
            }
        }
    }
}

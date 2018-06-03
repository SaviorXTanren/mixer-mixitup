using MixItUp.Base;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.WPF.Controls.MainControls;
using MixItUp.WPF.Controls.Settings;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatMessageHeaderControl.xaml
    /// </summary>
    public partial class ChatMessageHeaderControl : UserControl
    {
        public ChatMessageViewModel Message { get; private set; }

        public ChatMessageHeaderControl(ChatMessageViewModel message)
        {
            this.Loaded += ChatMessageHeaderControl_Loaded;

            InitializeComponent();

            this.DataContext = this.Message = message;
        }

        private void ChatMessageHeaderControl_Loaded(object sender, RoutedEventArgs e)
        {
            WhispererNumber.Visibility = ChannelSession.Settings.TrackWhispererNumber ? Visibility.Visible : Visibility.Collapsed;
            if (!this.Message.IsAlertMessage)
            {
                this.UserTextBlock.Foreground = Application.Current.FindResource(this.Message.User.PrimaryRoleColorName) as SolidColorBrush;

                if (!string.IsNullOrEmpty(this.Message.User.AvatarLink))
                {
                    this.UserAvatar.SetImageUrl(this.Message.User.AvatarLink);
                }

                if (ChatControl.SubscriberBadgeBitmap != null && this.Message.User.IsSubscriber)
                {
                    this.SubscriberImage.Visibility = Visibility.Visible;
                    this.SubscriberImage.Source = ChatControl.SubscriberBadgeBitmap;
                }
            }
            this.UpdateSizing();
        }

        public void DeleteMessage()
        {
            this.UserTextBlock.TextDecorations = TextDecorations.Strikethrough;
            this.TargetUserTextBlock.TextDecorations = TextDecorations.Strikethrough;
        }

        public void UpdateSizing()
        {
            int fontSize = this.GetChatFontSize();
            this.UserAvatar.SetSize(fontSize + 2);
            this.SubscriberImage.Height = this.SubscriberImage.Width = fontSize + 2;
            this.UserTextBlock.FontSize = fontSize;
            this.TargetUserTextBlock.FontSize = fontSize;
        }

        private int GetChatFontSize() { return Math.Max(ChannelSession.Settings.ChatFontSize, ChatSettingsControl.ChatDefaultFontSize); }
    }
}

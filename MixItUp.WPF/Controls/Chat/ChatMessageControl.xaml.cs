using MixItUp.Base;
using MixItUp.Base.Model.Chat;
using MixItUp.Base.Model.Chat.Mixer;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Mixer;
using StreamingClient.Base.Util;
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
        public ChatMessageViewModel Message { get; private set; }

        private List<TextBlock> textBlocks = new List<TextBlock>();

        public ChatMessageControl()
        {
            InitializeComponent();

            this.Loaded += ChatMessageControl_Loaded;
            this.DataContextChanged += ChatMessageControl_DataContextChanged;

            GlobalEvents.OnChatFontSizeChanged += GlobalEvents_OnChatFontSizeChanged;
        }

        private void GlobalEvents_OnChatFontSizeChanged(object sender, EventArgs e)
        {
            if (this.Message != null)
            {
                this.Message.OnDeleted -= Message_OnDeleted;
            }
            this.Message = null;
            this.MessageWrapPanel.Children.Clear();
            this.ChatMessageControl_DataContextChanged(this, new DependencyPropertyChangedEventArgs());
        }

        private void ChatMessageControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.ChatMessageControl_DataContextChanged(sender, new System.Windows.DependencyPropertyChangedEventArgs());
        }

        private void ChatMessageControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is ChatMessageViewModel && this.Message == null)
            {
                this.Message = (ChatMessageViewModel)this.DataContext;
                this.Message.OnDeleted += Message_OnDeleted;
                if (this.DataContext is AlertChatMessageViewModel)
                {
                    AlertChatMessageViewModel alert = (AlertChatMessageViewModel)this.DataContext;
                    SolidColorBrush foreground = null;
                    if (!string.IsNullOrEmpty(alert.Color) && !alert.Color.Equals(ColorSchemes.DefaultColorScheme))
                    {
                        try
                        {
                            foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(alert.Color));
                        }
                        catch (Exception ex)
                        {
                            Logger.Log("Bad Alert Color: " + alert.Color);
                            Logger.Log(ex);
                        }
                    }
                    this.AddStringMessage(alert.PlainTextMessage, foreground: foreground);
                }
                else
                {
                    ChatMessageHeaderControl header = new ChatMessageHeaderControl();
                    header.DataContext = this.DataContext;
                    this.MessageWrapPanel.Children.Add(header);

                    bool showMessage = true;
                    if (this.DataContext is MixerSkillChatMessageViewModel)
                    {
                        MixerSkillChatMessageViewModel skillMessage = (MixerSkillChatMessageViewModel)this.DataContext;
                        if (skillMessage.Skill.Type == MixerSkillTypeEnum.Gif)
                        {
                            this.GifSkillPopup.DataContext = skillMessage.Skill.Image;
                            this.GifSkillPopup.Visibility = Visibility.Visible;
                            this.GifSkillIcon.Height = this.GifSkillIcon.Width = ChannelSession.Settings.ChatFontSize * 2;
                        }
                        else
                        {
                            this.AddImage(new Uri(skillMessage.Skill.Image), ChannelSession.Settings.ChatFontSize * 3, skillMessage.Skill.Name);
                        }

                        if (skillMessage.Skill.Type == MixerSkillTypeEnum.Other)
                        {
                            this.AddStringMessage(skillMessage.Skill.Name);
                        }

                        if (skillMessage.Skill.IsEmbersSkill)
                        {
                            this.AddImage(new Uri("/Assets/Images/Embers.png", UriKind.Relative), ChannelSession.Settings.ChatFontSize + 2, MixerSkillModel.EmbersCurrencyName);
                        }
                        else
                        {
                            this.AddImage(new Uri("/Assets/Images/Sparks.png", UriKind.Relative), ChannelSession.Settings.ChatFontSize + 2, MixerSkillModel.SparksCurrencyName);
                            showMessage = false;
                        }

                        this.AddStringMessage(" " + skillMessage.Skill.Cost.ToString());
                    }

                    if (showMessage)
                    {
                        foreach (object messagePart in this.Message.MessageParts)
                        {
                            if (messagePart is string)
                            {
                                string messagePartString = (string)messagePart;

                                bool isWhisperToStreamer = this.Message.IsWhisper && ChannelSession.MixerStreamerUser.username.Equals(this.Message.TargetUsername, StringComparison.InvariantCultureIgnoreCase);
                                bool isStreamerTagged = messagePartString.Contains("@" + ChannelSession.MixerStreamerUser.username);

                                this.AddStringMessage(messagePartString, isHighlighted: (isWhisperToStreamer || isStreamerTagged));
                            }
                            else if (messagePart is MixerChatEmoteModel)
                            {
                                this.MessageWrapPanel.Children.Add(new ChatEmoteControl((MixerChatEmoteModel)messagePart));
                            }
                        }
                    }
                }
            }
        }

        private void AddStringMessage(string text, bool isHighlighted = false, SolidColorBrush foreground = null)
        {
            foreach (string word in text.Split(new string[] { " " }, StringSplitOptions.None))
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = word + " ";
                textBlock.FontSize = ChannelSession.Settings.ChatFontSize;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                if (foreground != null)
                {
                    textBlock.FontWeight = FontWeights.Bold;
                    textBlock.Foreground = foreground;
                }

                if (isHighlighted)
                {
                    textBlock.Background = (Brush)FindResource("PrimaryHueLightBrush");
                    textBlock.Foreground = (Brush)FindResource("PrimaryHueLightForegroundBrush");
                }

                this.textBlocks.Add(textBlock);
                this.MessageWrapPanel.Children.Add(textBlock);
            }
        }

        private void AddImage(Uri uri, int size, string tooltip = "")
        {
            Image image = new Image();
            image.Source = new BitmapImage(uri);
            image.Width = size;
            image.Height = size;
            image.ToolTip = tooltip;
            image.VerticalAlignment = VerticalAlignment.Center;
            image.HorizontalAlignment = HorizontalAlignment.Center;
            image.Margin = new Thickness(5, 0, 5, 0);
            this.MessageWrapPanel.Children.Add(image);
        }

        private void Message_OnDeleted(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                foreach (TextBlock tb in this.textBlocks)
                {
                    tb.TextDecorations = TextDecorations.Strikethrough;
                }

                if (!string.IsNullOrEmpty(this.Message.DeletedBy))
                {
                    if (!string.IsNullOrEmpty(this.Message.ModerationReason))
                    {
                        this.AddStringMessage(" (" + this.Message.ModerationReason + " By: " + this.Message.DeletedBy + ")");
                    }
                    else
                    {
                        this.AddStringMessage(" (Deleted By: " + this.Message.DeletedBy + ")");
                    }
                }
                else if (!string.IsNullOrEmpty(this.Message.ModerationReason))
                {
                    this.AddStringMessage(" (Auto-Moderated: " + this.Message.ModerationReason + ")");
                }
                else
                {
                    this.AddStringMessage(" (Manual Deletion)");
                }
            });
        }
    }
}

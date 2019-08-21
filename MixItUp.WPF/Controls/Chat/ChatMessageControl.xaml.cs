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
        }

        private void ChatMessageControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.ChatMessageControl_DataContextChanged(sender, new System.Windows.DependencyPropertyChangedEventArgs());
        }

        private void ChatMessageControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null)
            {
                if (this.DataContext is ChatMessageViewModel && this.Message == null)
                {
                    this.Message = (ChatMessageViewModel)this.DataContext;
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
                else if (this.DataContext is AlertMessageViewModel)
                {
                    AlertMessageViewModel alert = (AlertMessageViewModel)this.DataContext;
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
                    this.AddStringMessage(alert.Message, foreground: foreground);
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

        //private void xChatMessageControl_Loaded(object sender, RoutedEventArgs e)
        //{
        //    this.MessageWrapPanel.Children.Clear();

        //    if (this.Message.IsSkill)
        //    {
        //        SkillControl skillControl = new SkillControl(this.Message.Skill);
        //        this.MessageWrapPanel.Children.Add(skillControl);
        //    }

        //    foreach (ChatMessageDataModel messageData in this.Message.MessageComponents)
        //    {
        //        MixerChatEmoteModel emoticon = MixerChatEmoteModel.GetEmoteForMessageData(messageData);
        //        if (emoticon != null)
        //        {
        //            ChatEmoteControl emoticonControl = new ChatEmoteControl(emoticon);
        //            this.MessageWrapPanel.Children.Add(emoticonControl);
        //        }
        //        else if (messageData.type.Equals("image"))
        //        {
        //            StickerControl stickerControl = new StickerControl(this.Message.ChatSkill);
        //            this.MessageWrapPanel.Children.Add(stickerControl);
        //        }
        //        else
        //        {
        //            foreach (string word in messageData.text.Split(new string[] { " " }, StringSplitOptions.None))
        //            {
        //                TextBlock textBlock = new TextBlock();
        //                textBlock.Text = word + " ";
        //                textBlock.VerticalAlignment = VerticalAlignment.Center;
        //                if (this.Message.IsAlert)
        //                {
        //                    textBlock.FontWeight = FontWeights.Bold;
        //                    if (!string.IsNullOrEmpty(this.Message.AlertMessageBrush) && !this.Message.AlertMessageBrush.Equals(ColorSchemes.DefaultColorScheme))
        //                    {
        //                        textBlock.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(this.Message.AlertMessageBrush));
        //                    }
        //                    else
        //                    {
        //                        textBlock.Foreground = (App.AppSettings.IsDarkBackground) ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Black);
        //                    }
        //                }

        //                bool isWhisperToStreamer = this.Message.IsWhisper && ChannelSession.MixerStreamerUser.username.Equals(this.Message.TargetUsername, StringComparison.InvariantCultureIgnoreCase);
        //                bool isStreamerTagged = messageData.type == "tag" && word.Equals("@" + ChannelSession.MixerStreamerUser.username, StringComparison.InvariantCultureIgnoreCase);
        //                if (isWhisperToStreamer || isStreamerTagged)
        //                {
        //                    textBlock.Background = (Brush)FindResource("PrimaryHueLightBrush");
        //                    textBlock.Foreground = (Brush)FindResource("PrimaryHueLightForegroundBrush");
        //                }

        //                this.textBlocks.Add(textBlock);
        //                this.MessageWrapPanel.Children.Add(textBlock);
        //            }
        //        }
        //    }

        //    this.UpdateSizing();

        //    if (!string.IsNullOrEmpty(this.Message.ModerationReason))
        //    {
        //        this.DeleteMessage();
        //    }
        //}

        //public void DeleteMessage(string deletedBy = null)
        //{
        //    this.Dispatcher.Invoke(() =>
        //    {
        //        TextBlock textBlock = new TextBlock();
        //        textBlock.VerticalAlignment = VerticalAlignment.Center;

        //        if (!string.IsNullOrEmpty(deletedBy))
        //        {
        //            string text = " (Deleted By: " + deletedBy + ")";
        //            textBlock.Text += text;
        //            this.Message.AddToMessage(text);
        //            this.Message.DeletedBy = deletedBy;
        //        }

        //        if (this.Message.IsAlert)
        //        {
        //            textBlock.FontWeight = FontWeights.Bold;
        //            if (!string.IsNullOrEmpty(this.Message.AlertMessageBrush) && !this.Message.AlertMessageBrush.Equals(ColorSchemes.DefaultColorScheme))
        //            {
        //                textBlock.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(this.Message.AlertMessageBrush));
        //            }
        //            else
        //            {
        //                textBlock.Foreground = (App.AppSettings.IsDarkBackground) ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Black);
        //            }
        //        }

        //        this.Message.IsDeleted = true;
        //        foreach (TextBlock tb in this.textBlocks)
        //        {
        //            tb.TextDecorations = TextDecorations.Strikethrough;
        //        }

        //        this.textBlocks.Add(textBlock);
        //        this.MessageWrapPanel.Children.Add(textBlock);

        //        if (!string.IsNullOrEmpty(this.Message.ModerationReason))
        //        {
        //            textBlock = new TextBlock();
        //            textBlock.VerticalAlignment = VerticalAlignment.Center;

        //            string text = " (Auto-Moderated: " + this.Message.ModerationReason + ")";
        //            textBlock.Text += text;
        //            this.Message.AddToMessage(text);

        //            this.textBlocks.Add(textBlock);
        //            this.MessageWrapPanel.Children.Add(textBlock);
        //        }
        //    });
        //}

        //public void UpdateSizing()
        //{
        //    foreach (var item in this.MessageWrapPanel.Children)
        //    {
        //        if (item is TextBlock)
        //        {
        //            TextBlock textBlock = (TextBlock)item;
        //            textBlock.FontSize = ChannelSession.Settings.ChatFontSize;
        //        }
        //        else if (item is MixerChatEmoteModel)
        //        {
        //            MixerChatEmoteModel emoticon = (MixerChatEmoteModel)item;
        //            emoticon.Height = emoticon.Width = (uint)ChannelSession.Settings.ChatFontSize + 2;
        //        }
        //        else if (item is StickerControl)
        //        {
        //            StickerControl sticker = (StickerControl)item;
        //            sticker.UpdateSizing();
        //        }
        //        else if (item is SkillControl)
        //        {
        //            SkillControl skill = (SkillControl)item;
        //            skill.UpdateSizing();
        //        }
        //    }
        //}
    }
}

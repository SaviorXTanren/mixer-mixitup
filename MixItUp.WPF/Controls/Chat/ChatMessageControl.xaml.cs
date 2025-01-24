using MixItUp.Base;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Twitch;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatMessageControl.xaml
    /// </summary>
    public partial class ChatMessageControl : UserControl
    {
        private static StrikethroughConverter strikethroughConverter = new StrikethroughConverter();

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
            if (this.IsLoaded && this.DataContext != null && this.DataContext is ChatMessageViewModel && this.Message == null)
            {
                this.Loaded -= ChatMessageControl_Loaded;
                this.DataContextChanged -= ChatMessageControl_DataContextChanged;

                this.Message = (ChatMessageViewModel)this.DataContext;
                bool italics = false;
                bool highlighted = false;

                this.Separator.Visibility = (ChannelSession.Settings.AddSeparatorsBetweenMessages) ? Visibility.Visible : Visibility.Collapsed;

                if (this.DataContext is AlertChatMessageViewModel)
                {
                    AlertChatMessageViewModel alert = (AlertChatMessageViewModel)this.DataContext;
                    SolidColorBrush foreground = null;
                    if (!string.IsNullOrEmpty(alert.Color) && !alert.Color.Equals(ColorSchemes.DefaultColorScheme))
                    {
                        string color = alert.Color;
                        try
                        {
                            if (ColorSchemes.MaterialDesignColors.TryGetValue(color.Replace(" ", string.Empty), out var colorOverride))
                            {
                                color = colorOverride;
                            }
                            foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(color));
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(LogLevel.Error, "Bad Alert Color: " + color);
                            Logger.Log(ex);
                        }
                    }
                    this.AddStringMessage(alert.PlainTextMessage, foreground: foreground);
                    this.MessageWrapPanel.HorizontalAlignment = HorizontalAlignment.Center;
                }
                else
                {
                    ChatMessageHeaderControl header = new ChatMessageHeaderControl();
                    header.DataContext = this.DataContext;
                    this.MessageWrapPanel.Children.Add(header);

                    bool showMessage = true;

                    if (this.DataContext is ChatMessageViewModel)
                    {
                        ChatMessageViewModel message = (ChatMessageViewModel)this.DataContext;
                        highlighted = highlighted || message.IsStreamerTagged;
                    }

                    if (this.DataContext is TwitchChatMessageViewModel)
                    {
                        TwitchChatMessageViewModel twitchMessage = (TwitchChatMessageViewModel)this.DataContext;
                        italics = twitchMessage.IsSlashMe;
                        highlighted = highlighted || twitchMessage.IsHighlightedMessage;
                    }

                    if (showMessage)
                    {
                        foreach (object messagePart in this.Message.MessageParts)
                        {
                            if (messagePart is string)
                            {
                                string messagePartString = (string)messagePart;
                                this.AddStringMessage(messagePartString, isHighlighted: highlighted, isItalicized: italics);
                            }
                            else if (messagePart is ChatEmoteViewModelBase)
                            {
                                ChatEmoteViewModelBase emote = (ChatEmoteViewModelBase)messagePart;
                                Image image = new Image();
                                ImageHelper.SetImageSource(image, emote.ImageURL, ChannelSession.Settings.ChatFontSize * 2, ChannelSession.Settings.ChatFontSize * 2, emote.Name);
                                this.MessageWrapPanel.Children.Add(image);

                                if (emote is TwitchBitsCheerViewModel)
                                {
                                    TwitchBitsCheerViewModel bitsCheer = (TwitchBitsCheerViewModel)emote;
                                    this.AddStringMessage(bitsCheer.Amount.ToString());
                                }
                            }
                        }
                    }
                }

                if (this.Message.IsDeleted)
                {
                    foreach (TextBlock textBlock in this.textBlocks)
                    {
                        textBlock.TextDecorations = TextDecorations.Strikethrough;
                    }
                    this.AddStringMessage(this.Message.DeletedInformation);
                }
            }
        }

        private void AddStringMessage(string text, bool isHighlighted = false, bool isItalicized = false, SolidColorBrush foreground = null)
        {
            foreach (string word in text.Split(new string[] { " " }, StringSplitOptions.None))
            {
                TextBlock textBlock = new TextBlock();
                textBlock.DataContext = this.Message;
                textBlock.Text = word + " ";
                textBlock.FontSize = ChannelSession.Settings.ChatFontSize;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                textBlock.TextWrapping = TextWrapping.Wrap;

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

                if (isItalicized)
                {
                    foreach (var run in textBlock.Inlines)
                    {
                        run.FontStyle = FontStyles.Italic;
                    }
                }

                this.textBlocks.Add(textBlock);
                this.MessageWrapPanel.Children.Add(textBlock);
            }
        }
    }
}

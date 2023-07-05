using MixItUp.Base;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Twitch;
using StreamingClient.Base.Util;
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
        public ChatMessageViewModel Message { get; private set; }

        private List<TextBlock> textBlocks = new List<TextBlock>();

        public ChatMessageControl()
        {
            InitializeComponent();

            this.Loaded += ChatMessageControl_Loaded;
            this.DataContextChanged += ChatMessageControl_DataContextChanged;

            GlobalEvents.OnChatVisualSettingsChanged += GlobalEvents_OnChatVisualSettingsChanged;
        }

        private void GlobalEvents_OnChatVisualSettingsChanged(object sender, EventArgs e)
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
            if (this.IsLoaded && this.DataContext != null && this.DataContext is ChatMessageViewModel && this.Message == null)
            {
                this.Message = (ChatMessageViewModel)this.DataContext;
                this.Message.OnDeleted += Message_OnDeleted;
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
                            if (ColorSchemes.HTMLColorSchemeDictionary.TryGetValue(color.Replace(" ", string.Empty), out var colorOverride))
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
                                this.MessageWrapPanel.Children.Add(new ChatImageControl((ChatEmoteViewModelBase)messagePart));
                            }
                        }
                    }
                }

                if (this.Message.IsDeleted)
                {
                    this.Message_OnDeleted(this, new EventArgs());
                }
            }
        }

        private void AddStringMessage(string text, bool isHighlighted = false, bool isItalicized = false, SolidColorBrush foreground = null)
        {
            foreach (string word in text.Split(new string[] { " " }, StringSplitOptions.None))
            {
                TextBlock textBlock = new TextBlock();
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
                        this.AddStringMessage($" ({this.Message.ModerationReason} {MixItUp.Base.Resources.By}: {this.Message.DeletedBy})");
                    }
                    else
                    {
                        this.AddStringMessage($" ({MixItUp.Base.Resources.DeletedBy}: {this.Message.DeletedBy})");
                    }
                }
                else if (!string.IsNullOrEmpty(this.Message.ModerationReason))
                {
                    this.AddStringMessage($" ({MixItUp.Base.Resources.AutoModerated}: {this.Message.ModerationReason})");
                }
                else
                {
                    this.AddStringMessage($" ({MixItUp.Base.Resources.ManualDeletion})");
                }
            });
        }
    }
}

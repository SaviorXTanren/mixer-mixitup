using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Actions
{
    public class JSONToSpecialIdentifierPair
    {
        public string JSONParameterName { get; set; }
        public string SpecialIdentifierName { get; set; }
    }

    /// <summary>
    /// Interaction logic for WebRequestAction.xaml
    /// </summary>
    public partial class WebRequestActionControl : ActionControlBase
    {
        private WebRequestAction action;

        private ObservableCollection<JSONToSpecialIdentifierPair> jsonToSpecialIdentifierPairs = new ObservableCollection<JSONToSpecialIdentifierPair>();

        public WebRequestActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public WebRequestActionControl(ActionContainerControl containerControl, WebRequestAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.ResponseActionComboBox.ItemsSource = EnumHelper.GetEnumNames<WebRequestResponseActionTypeEnum>();           
            this.CommandResponseComboBox.ItemsSource = ChannelSession.Settings.ChatCommands.OrderBy(c => c.Name);

            this.JSONToSpecialIdentifiersItemsControl.ItemsSource = this.jsonToSpecialIdentifierPairs;
            this.jsonToSpecialIdentifierPairs.Add(new JSONToSpecialIdentifierPair());

            if (this.action != null)
            {
                this.WebRequestURLTextBox.Text = this.action.Url;
                this.ResponseActionComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.ResponseAction);
                if (this.action.ResponseAction == WebRequestResponseActionTypeEnum.Chat)
                {
                    this.ChatResponseTextBox.Text = this.action.ResponseChatText;
                }
                else if (this.action.ResponseAction == WebRequestResponseActionTypeEnum.Command)
                {
                    this.CommandResponseComboBox.SelectedItem = ChannelSession.AllEnabledCommands.FirstOrDefault(c => c.ID.Equals(this.action.ResponseCommandID));
                    this.CommandResponseArgumentsTextBox.Text = this.action.ResponseCommandArgumentsText;
                }
                else if (this.action.ResponseAction == WebRequestResponseActionTypeEnum.SpecialIdentifier)
                {
                    this.SpecialIdentifierResponseTextBox.Text = this.action.SpecialIdentifierName;
                }
                else if (this.action.ResponseAction == WebRequestResponseActionTypeEnum.JSONToSpecialIdentifiers)
                {
                    this.jsonToSpecialIdentifierPairs.Clear();
                    foreach (var kvp in this.action.JSONToSpecialIdentifiers)
                    {
                        this.jsonToSpecialIdentifierPairs.Add(new JSONToSpecialIdentifierPair() { JSONParameterName = kvp.Key, SpecialIdentifierName = kvp.Value });
                    }
                }
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (!string.IsNullOrEmpty(this.WebRequestURLTextBox.Text))
            {
                WebRequestResponseActionTypeEnum responseType = EnumHelper.GetEnumValueFromString<WebRequestResponseActionTypeEnum>((string)this.ResponseActionComboBox.SelectedItem);
                if (responseType == WebRequestResponseActionTypeEnum.Chat)
                {
                    if (!string.IsNullOrEmpty(this.ChatResponseTextBox.Text))
                    {
                        return WebRequestAction.CreateForChat(this.WebRequestURLTextBox.Text, this.ChatResponseTextBox.Text);
                    }
                }
                else if (responseType == WebRequestResponseActionTypeEnum.Command)
                {
                    if (this.CommandResponseComboBox.SelectedIndex >= 0)
                    {
                        return WebRequestAction.CreateForCommand(this.WebRequestURLTextBox.Text, (CommandBase)this.CommandResponseComboBox.SelectedItem, this.CommandResponseArgumentsTextBox.Text);
                    }
                }
                else if (responseType == WebRequestResponseActionTypeEnum.SpecialIdentifier)
                {
                    if (!string.IsNullOrEmpty(this.SpecialIdentifierResponseTextBox.Text) && SpecialIdentifierStringBuilder.IsValidSpecialIdentifier(this.SpecialIdentifierResponseTextBox.Text))
                    {
                        return WebRequestAction.CreateForSpecialIdentifier(this.WebRequestURLTextBox.Text, this.SpecialIdentifierResponseTextBox.Text);
                    }
                }
                else if (responseType == WebRequestResponseActionTypeEnum.JSONToSpecialIdentifiers)
                {
                    if (this.jsonToSpecialIdentifierPairs.Count > 0)
                    {
                        foreach (JSONToSpecialIdentifierPair pairs in this.jsonToSpecialIdentifierPairs)
                        {
                            if (string.IsNullOrEmpty(pairs.JSONParameterName) || !SpecialIdentifierStringBuilder.IsValidSpecialIdentifier(pairs.SpecialIdentifierName))
                            {
                                return null;
                            }
                        }
                        return WebRequestAction.CreateForJSONToSpecialIdentifiers(this.WebRequestURLTextBox.Text,
                            this.jsonToSpecialIdentifierPairs.ToDictionary(p => p.JSONParameterName, p => p.SpecialIdentifierName));
                    }
                }
                else
                {
                    return new WebRequestAction(this.WebRequestURLTextBox.Text, WebRequestResponseActionTypeEnum.None);
                }
            }
            return null;
        }

        private void ResponseActionComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.ResponseActionComboBox.SelectedIndex >= 0)
            {
                WebRequestResponseActionTypeEnum responseType = EnumHelper.GetEnumValueFromString<WebRequestResponseActionTypeEnum>((string)this.ResponseActionComboBox.SelectedItem);

                this.ChatResponseActionGrid.Visibility = (responseType == WebRequestResponseActionTypeEnum.Chat) ? Visibility.Visible : Visibility.Collapsed;
                this.CommandResponseActionGrid.Visibility = (responseType == WebRequestResponseActionTypeEnum.Command) ? Visibility.Visible : Visibility.Collapsed;
                this.SpecialIdentifierResponseActionGrid.Visibility = (responseType == WebRequestResponseActionTypeEnum.SpecialIdentifier) ? Visibility.Visible : Visibility.Collapsed;
                this.JSONToSpecialIdentifiersResponseActionGrid.Visibility = (responseType == WebRequestResponseActionTypeEnum.JSONToSpecialIdentifiers) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void DeleteJSONToSpecialIdentifierButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            JSONToSpecialIdentifierPair pair = (JSONToSpecialIdentifierPair)button.DataContext;
            this.jsonToSpecialIdentifierPairs.Remove(pair);
        }

        private void AddJSONToSpecialIdentifierButton_Click(object sender, RoutedEventArgs e)
        {
            this.jsonToSpecialIdentifierPairs.Add(new JSONToSpecialIdentifierPair());
        }
    }
}

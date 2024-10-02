using MixItUp.Base;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for EditTestCommandParametersDialogControl.xaml
    /// </summary>
    public partial class EditTestCommandParametersDialogControl : UserControl
    {
        private CommandParametersModel parameters;

        private readonly ObservableCollection<DataWrapper> values = new ObservableCollection<DataWrapper>();

        public EditTestCommandParametersDialogControl(CommandParametersModel parameters)
        {
            InitializeComponent();

            this.parameters = parameters;

            this.PlatformComboBox.ItemsSource = StreamingPlatforms.SelectablePlatforms;
            this.PlatformComboBox.SelectedItem = StreamingPlatformTypeEnum.All;

            this.UserTextBox.Text = ChannelSession.User.Username;

            this.ArgumentsTextBox.Text = string.Join(" ", this.parameters.Arguments);

            foreach (var kvp in this.parameters.SpecialIdentifiers)
            {
                this.values.Add(new DataWrapper { SpecialIdentifier = kvp.Key, Value = kvp.Value });
            }

            this.SpecialIdentifiersList.ItemsSource = values;

            if (ChannelSession.Settings.AlwaysUseCommandLocksWhenTestingCommands)
            {
                this.UseCommandLocks.IsChecked = true;
            }
        }

        public async Task<CommandParametersModel> GetCommandParameters()
        {
            this.parameters.Platform = (StreamingPlatformTypeEnum)this.PlatformComboBox.SelectedItem;

            if (!string.Equals(this.UserTextBox.Text, ChannelSession.User.Username, System.StringComparison.OrdinalIgnoreCase))
            {
                UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatform(this.parameters.Platform, platformUsername: this.UserTextBox.Text, performPlatformSearch: true);
                if (user != null)
                {
                    this.parameters.User = user;
                }
            }

            this.parameters.SetArguments(CommandParametersModel.GenerateArguments(this.ArgumentsTextBox.Text));

            this.parameters.SpecialIdentifiers.Clear();
            foreach (var value in this.values)
            {
                this.parameters.SpecialIdentifiers.Add(value.SpecialIdentifier, value.Value);
            }

            this.parameters.UseCommandLocks = this.UseCommandLocks.IsChecked.GetValueOrDefault();

            return this.parameters;
        }

        private class DataWrapper
        {
            public string SpecialIdentifier { get; set; }
            public string Value { get; set; }
        }
    }
}

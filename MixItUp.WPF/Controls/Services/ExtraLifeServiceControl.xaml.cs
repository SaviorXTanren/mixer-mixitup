using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for ExtraLifeServiceControl.xaml
    /// </summary>
    public partial class ExtraLifeServiceControl : ServicesControlBase
    {
        private ObservableCollection<ExtraLifeTeamParticipant> participants = new ObservableCollection<ExtraLifeTeamParticipant>();

        public ExtraLifeServiceControl()
        {
            InitializeComponent();
        }

        protected override Task OnLoaded()
        {
            this.SetHeaderText("Extra Life");

            this.TeamParticipantComboBox.ItemsSource = this.participants;

            if (ChannelSession.Settings.ExtraLifeTeamID > 0)
            {
                this.ExistingAccountGrid.Visibility = Visibility.Visible;
                this.IncludeTeamDonationsGrid.Visibility = Visibility.Collapsed;
                this.SetCompletedIcon(visible: true);
            }
            else
            {
                this.NewLoginGrid.Visibility = Visibility.Visible;
            }

            return base.OnLoaded();
        }

        private async void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            this.LogInButton.IsEnabled = false;
            this.TeamIDTextBox.IsEnabled = false;
            this.TeamParticipantComboBox.IsEnabled = false;

            if (string.IsNullOrEmpty(this.TeamIDTextBox.Text) || !int.TryParse(this.TeamIDTextBox.Text, out int teamID))
            {
                await MessageBoxHelper.ShowMessageDialog("Please enter a valid Extra Life team ID");
            }
            else if (this.TeamParticipantComboBox.SelectedIndex < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("Please select yourself from the Extra Life team participants");
            }
            else
            {
                ExtraLifeTeamParticipant participant = (ExtraLifeTeamParticipant)this.TeamParticipantComboBox.SelectedItem;

                await this.groupBoxControl.window.RunAsyncOperation(async () =>
                {
                    ChannelSession.Settings.ExtraLifeTeamID = teamID;
                    ChannelSession.Settings.ExtraLifeParticipantID = participant.participantID;
                    ChannelSession.Settings.ExtraLifeIncludeTeamDonations = this.IncludeTeamDonationsToggleButton.IsChecked.GetValueOrDefault();

                    if (!await ChannelSession.Services.InitializeExtraLife())
                    {
                        await MessageBoxHelper.ShowMessageDialog("Unable to authenticate with Extra Life. Please ensure you correctly input your Extra Life team & participant.");
                    }
                    else
                    {
                        this.NewLoginGrid.Visibility = Visibility.Collapsed;
                        this.IncludeTeamDonationsGrid.Visibility = Visibility.Collapsed;
                        this.ExistingAccountGrid.Visibility = Visibility.Visible;

                        this.SetCompletedIcon(visible: true);
                    }
                });
            }

            this.LogInButton.IsEnabled = true;
            this.TeamIDTextBox.IsEnabled = true;
            this.TeamParticipantComboBox.IsEnabled = true;
        }

        private async void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                await ChannelSession.Services.DisconnectExtraLife();
            });

            this.ExistingAccountGrid.Visibility = Visibility.Collapsed;
            this.NewLoginGrid.Visibility = Visibility.Visible;
            this.IncludeTeamDonationsGrid.Visibility = Visibility.Visible;

            this.SetCompletedIcon(visible: false);
        }

        private async void SearchTeamButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                if (string.IsNullOrEmpty(this.TeamIDTextBox.Text) || !int.TryParse(this.TeamIDTextBox.Text, out int teamID))
                {
                    await MessageBoxHelper.ShowMessageDialog("Please enter a valid Extra Life team ID");
                }
                else
                {
                    this.participants.Clear();

                    ExtraLifeTeam team = await ChannelSession.Services.ExtraLife.GetTeam(teamID);
                    if (team != null)
                    {
                        IEnumerable<ExtraLifeTeamParticipant> ps = await ChannelSession.Services.ExtraLife.GetTeamParticipants(teamID);
                        foreach (ExtraLifeTeamParticipant participant in ps.OrderBy(p => p.displayName))
                        {
                            this.participants.Add(participant);
                        }
                    }
                    else
                    {
                        await MessageBoxHelper.ShowMessageDialog("The Extra Life team ID you entered could not be found");
                    }
                }
            });
        }
    }
}

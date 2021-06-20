using MixItUp.Base.Model.Store;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.CommunityCommands;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class CommunityCommandsMainControlViewModel : WindowControlViewModelBase
    {
        public HashSet<Guid> DownloadedCommandsCache = new HashSet<Guid>();

        public ICommand BackCommand { get; set; }

        public bool ShowHome
        {
            get { return this.showHome; }
            set
            {
                this.showHome = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool showHome = true;

        public ThreadSafeObservableCollection<CommunityCommandCategoryViewModel> Categories { get; set; } = new ThreadSafeObservableCollection<CommunityCommandCategoryViewModel>();

        public string SearchText
        {
            get { return this.searchText; }
            set
            {
                this.searchText = value;
                this.NotifyPropertyChanged();
            }
        }
        private string searchText;

        public ICommand SearchCommand { get; set; }

        public bool ShowSearch
        {
            get { return this.showSearch; }
            set
            {
                this.showSearch = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool showSearch;

        public ThreadSafeObservableCollection<CommunityCommandViewModel> SearchResults { get; set; } = new ThreadSafeObservableCollection<CommunityCommandViewModel>();

        public ICommand GetCommandDetailsCommand { get; set; }

        public bool ShowCommandDetails
        {
            get { return this.showCommandDetails; }
            set
            {
                this.showCommandDetails = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool showCommandDetails;

        public CommunityCommandDetailsViewModel CommandDetails
        {
            get { return this.commandDetails; }
            set
            {
                this.commandDetails = value;
                this.NotifyPropertyChanged();
            }
        }
        private CommunityCommandDetailsViewModel commandDetails;

        public ICommand GetUserCommandsCommand { get; set; }

        public bool ShowUserCommands
        {
            get { return this.showUserCommands; }
            set
            {
                this.showUserCommands = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool showUserCommands;

        public ThreadSafeObservableCollection<CommunityCommandViewModel> UserCommands { get; set; } = new ThreadSafeObservableCollection<CommunityCommandViewModel>();

        public ICommand GetMyCommandsCommand { get; set; }

        public bool ShowMyCommands
        {
            get { return this.showMyCommands; }
            set
            {
                this.showMyCommands = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool showMyCommands;

        public ThreadSafeObservableCollection<CommunityCommandViewModel> MyCommands { get; set; } = new ThreadSafeObservableCollection<CommunityCommandViewModel>();

        public ICommand EditMyCommandCommand { get; set; }

        public ICommand DeleteMyCommandCommand { get; set; }

        private bool firstLoadCompleted = false;
        private DateTimeOffset lastCategoryRefresh = DateTimeOffset.MinValue;

        public CommunityCommandsMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            this.BackCommand = this.CreateCommand(async () =>
            {
                if (this.ShowMyCommands)
                {
                    await this.NavigateToCategories();
                }
                else if (this.ShowUserCommands)
                {
                    await this.NavigateToCategories();
                }
                else if (this.ShowSearch)
                {
                    await this.NavigateToCategories();
                }
                else if (this.ShowCommandDetails)
                {
                    if (this.MyCommands.Count > 0)
                    {
                        this.ClearAllShows();
                        this.ShowMyCommands = true;
                    }
                    else if (this.UserCommands.Count > 0)
                    {
                        this.ClearAllShows();
                        this.ShowUserCommands = true;
                    }
                    else if (this.SearchResults.Count > 0)
                    {
                        this.ClearAllShows();
                        this.ShowSearch = true;
                    }
                    else
                    {
                        await this.NavigateToCategories();
                    }
                }
            });

            this.SearchCommand = this.CreateCommand(async () =>
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(this.SearchText))
                    {
                        this.SearchResults.Clear();
                        foreach (CommunityCommandModel command in await ChannelSession.Services.CommunityCommandsService.SearchCommands(this.SearchText))
                        {
                            this.SearchResults.Add(new CommunityCommandViewModel(command));
                        }

                        this.ClearAllShows();
                        this.ShowSearch = true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            });

            this.GetCommandDetailsCommand = this.CreateCommand(async (id) =>
            {
                try
                {
                    CommunityCommandDetailsModel commandDetails = await ChannelSession.Services.CommunityCommandsService.GetCommandDetails((Guid)id);
                    if (commandDetails != null)
                    {
                        this.CommandDetails = new CommunityCommandDetailsViewModel(commandDetails);

                        this.ClearAllShows();
                        this.ShowCommandDetails = true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            });

            this.GetUserCommandsCommand = this.CreateCommand(async () =>
            {
                try
                {
                    this.UserCommands.Clear();
                    foreach (CommunityCommandModel command in await ChannelSession.Services.CommunityCommandsService.GetCommandsByUser(this.CommandDetails.UserID))
                    {
                        this.UserCommands.Add(new CommunityCommandViewModel(command));
                    }

                    this.ClearAllShows();
                    this.ShowUserCommands = true;
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            });

            this.GetMyCommandsCommand = this.CreateCommand(async () =>
            {
                try
                {
                    this.MyCommands.Clear();
                    foreach (CommunityCommandModel command in await ChannelSession.Services.CommunityCommandsService.GetMyCommands())
                    {
                        this.MyCommands.Add(new CommunityCommandViewModel(command));
                    }

                    this.ClearAllShows();
                    this.ShowMyCommands = true;
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            });

            this.EditMyCommandCommand = this.CreateCommand(async (id) =>
            {
                try
                {
                    CommunityCommandDetailsModel commandDetails = await ChannelSession.Services.CommunityCommandsService.GetCommandDetails((Guid)id);
                    if (commandDetails != null)
                    {
                        this.CommandDetails = new MyCommunityCommandDetailsViewModel(commandDetails);

                        this.ClearAllShows();
                        this.ShowCommandDetails = true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            });

            this.DeleteMyCommandCommand = this.CreateCommand(async () =>
            {
                try
                {
                    if (this.CommandDetails.IsMyCommand && await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.CommunityCommandsDeleteMyCommandConfirmation))
                    {
                        await ChannelSession.Services.CommunityCommandsService.DeleteCommand(this.CommandDetails.ID);

                        this.GetMyCommandsCommand.Execute(null);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            });
        }

        public async Task ReviewCommand(int rating, string review)
        {
            await ChannelSession.Services.CommunityCommandsService.AddReview(new CommunityCommandReviewModel()
            {
                CommandID = this.CommandDetails.ID,
                Rating = rating,
                Review = review,
            });

            this.GetCommandDetailsCommand.Execute(this.CommandDetails.ID);
        }

        public async Task ReportCommand(string report)
        {
            await ChannelSession.Services.CommunityCommandsService.ReportCommand(new CommunityCommandReportModel()
            {
                CommandID = this.CommandDetails.ID,
                Report = report
            });
        }

        protected override async Task OnVisibleInternal()
        {
            if (this.firstLoadCompleted)
            {
                if (this.ShowHome)
                {
                    await this.NavigateToCategories();
                }
            }
            this.firstLoadCompleted = true;
            await base.OnVisibleInternal();
        }

        private async Task NavigateToCategories()
        {
            if (this.lastCategoryRefresh.TotalMinutesFromNow() > 1)
            {
                try
                {
                    this.Categories.Clear();
                    foreach (CommunityCommandCategoryModel category in await ChannelSession.Services.CommunityCommandsService.GetHomeCategories())
                    {
                        this.Categories.Add(new CommunityCommandCategoryViewModel(category));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }

                this.lastCategoryRefresh = DateTimeOffset.Now;
            }

            this.ClearAllShows();
            this.ShowHome = true;

            this.SearchResults.Clear();
            this.UserCommands.Clear();
            this.MyCommands.Clear();
        }

        private void ClearAllShows()
        {
            this.ShowHome = false;
            this.ShowSearch = false;
            this.ShowCommandDetails = false;
            this.ShowUserCommands = false;
            this.ShowMyCommands = false;
        }
    }
}

using MixItUp.Base.Model.Store;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.CommunityCommands;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class CommunityCommandsMainControlViewModel : WindowControlViewModelBase
    {
        private const int SearchResultsPageSize = 25;

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

        public ObservableCollection<CommunityCommandCategoryViewModel> Categories { get; set; } = new ObservableCollection<CommunityCommandCategoryViewModel>();

        public ICommand CategorySeeMoreCommand { get; set; }

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

        public ObservableCollection<CommunityCommandViewModel> SearchResults { get; set; } = new ObservableCollection<CommunityCommandViewModel>();

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

        public ICommand WebsiteLinkCommand { get; set; }

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

        public ObservableCollection<CommunityCommandViewModel> UserCommands { get; set; } = new ObservableCollection<CommunityCommandViewModel>();

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

        public ObservableCollection<CommunityCommandViewModel> MyCommands { get; set; } = new ObservableCollection<CommunityCommandViewModel>();

        public ICommand EditMyCommandCommand { get; set; }

        public ICommand DeleteMyCommandCommand { get; set; }

        public bool ShowNextResults
        {
            get { return this.showNextResults; }
            set
            {
                this.showNextResults = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("HasNextOrPreviousResults");
            }
        }
        private bool showNextResults;
        public ICommand NextResultsCommand { get; set; }

        public bool ShowPreviousResults
        {
            get { return this.showPreviousResults; }
            set
            {
                this.showPreviousResults = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("HasNextOrPreviousResults");
            }
        }
        private bool showPreviousResults;
        public ICommand PreviousResultsCommand { get; set; }

        public int CurrentResultsPage
        {
            get { return this.currentResultsPage; }
            set
            {
                this.currentResultsPage = value;
                this.NotifyPropertyChanged();
            }
        }
        private int currentResultsPage;

        public int TotaResultsPages
        {
            get { return this.totaResultsPages; }
            set
            {
                this.totaResultsPages = value;
                this.NotifyPropertyChanged();
            }
        }
        private int totaResultsPages;

        public bool HasNextOrPreviousResults { get { return this.ShowNextResults || this.ShowPreviousResults; } }

        private int GetResultsPageSkip { get { return (Math.Max(this.CurrentResultsPage, 1) - 1) * SearchResultsPageSize; } }

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
                    this.CurrentResultsPage = 0;
                    await this.PerformSearch();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            });

            this.CategorySeeMoreCommand = this.CreateCommand(async (searchTag) =>
            {
                try
                {
                    this.CurrentResultsPage = 0;
                    this.SearchText = searchTag.ToString();
                    await this.PerformSearch();
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
                    CommunityCommandDetailsModel commandDetails = await ServiceManager.Get<MixItUpService>().GetCommandDetails((Guid)id);
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

            this.WebsiteLinkCommand = this.CreateCommand(() =>
            {
                ServiceManager.Get<IProcessService>().LaunchLink(this.CommandDetails?.WebsiteURL);
            });

            this.GetUserCommandsCommand = this.CreateCommand(async () =>
            {
                try
                {
                    this.CurrentResultsPage = 0;
                    await this.PerformUserCommands();
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
                    this.CurrentResultsPage = 0;
                    await this.PerformMyCommands();
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
                    CommunityCommandDetailsModel commandDetails = await ServiceManager.Get<MixItUpService>().GetCommandDetails((Guid)id);
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
                        await ServiceManager.Get<MixItUpService>().DeleteCommand(this.CommandDetails.ID);

                        this.GetMyCommandsCommand.Execute(null);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            });

            this.PreviousResultsCommand = this.CreateCommand(async () =>
            {
                this.CurrentResultsPage--;
                if (this.ShowMyCommands)
                {
                    await this.PerformMyCommands();
                }
                else if (this.ShowUserCommands)
                {
                    await this.PerformUserCommands();
                }
                else if (this.ShowSearch)
                {
                    await this.PerformSearch();
                }
            });

            this.NextResultsCommand = this.CreateCommand(async () =>
            {
                this.CurrentResultsPage++;
                if (this.ShowMyCommands)
                {
                    await this.PerformMyCommands();
                }
                else if (this.ShowUserCommands)
                {
                    await this.PerformUserCommands();
                }
                else if (this.ShowSearch)
                {
                    await this.PerformSearch();
                }
            });
        }

        public async Task ReviewCommand(int rating, string review)
        {
            await ServiceManager.Get<MixItUpService>().AddReview(new CommunityCommandReviewModel()
            {
                CommandID = this.CommandDetails.ID,
                Rating = rating,
                Review = review,
            });

            this.GetCommandDetailsCommand.Execute(this.CommandDetails.ID);
        }

        public async Task ReportCommand(string report)
        {
            await ServiceManager.Get<MixItUpService>().ReportCommand(new CommunityCommandReportModel()
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
                    foreach (CommunityCommandCategoryModel category in await ServiceManager.Get<MixItUpService>().GetHomeCategories())
                    {
                        if (category.Commands.Count > 0)
                        {
                            this.Categories.Add(new CommunityCommandCategoryViewModel(category));
                        }
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

        private async Task PerformSearch()
        {
            if (!string.IsNullOrWhiteSpace(this.SearchText))
            {
                this.SearchResults.Clear();

                CommunityCommandsSearchResult results = await ServiceManager.Get<MixItUpService>().SearchCommands(this.SearchText, this.GetResultsPageSkip, SearchResultsPageSize);
                foreach (CommunityCommandModel command in results.Results)
                {
                    this.SearchResults.Add(new CommunityCommandViewModel(command));
                }
                this.SetSearchResultProperties(results);

                this.ClearAllShows();
                this.ShowSearch = true;
            }
        }

        private async Task PerformUserCommands()
        {
            this.UserCommands.Clear();

            CommunityCommandsSearchResult results = await ServiceManager.Get<MixItUpService>().GetCommandsByUser(this.CommandDetails.UserID, this.GetResultsPageSkip, SearchResultsPageSize);
            foreach (CommunityCommandModel command in results.Results)
            {
                this.UserCommands.Add(new CommunityCommandViewModel(command));
            }
            this.SetSearchResultProperties(results);

            this.ClearAllShows();
            this.ShowUserCommands = true;
        }

        private async Task PerformMyCommands()
        {
            this.MyCommands.Clear();

            CommunityCommandsSearchResult results = await ServiceManager.Get<MixItUpService>().GetMyCommands(this.GetResultsPageSkip, SearchResultsPageSize);
            foreach (CommunityCommandModel command in results.Results)
            {
                this.MyCommands.Add(new CommunityCommandViewModel(command));
            }
            this.SetSearchResultProperties(results);

            this.ClearAllShows();
            this.ShowMyCommands = true;
        }

        private void SetSearchResultProperties(CommunityCommandsSearchResult results)
        {
            this.ShowPreviousResults = results.HasPreviousResults;
            this.ShowNextResults = results.HasNextResults;

            this.CurrentResultsPage = results.PageNumber;
            this.TotaResultsPages = results.TotalPages;
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

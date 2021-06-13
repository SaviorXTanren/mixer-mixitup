using MixItUp.Base.Model.Store;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.CommunityCommands;
using StreamingClient.Base.Util;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class CommunityCommandsMainControlViewModel : WindowControlViewModelBase
    {
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

        public ICommand DetailsCommand { get; set; }

        public bool ShowDetails
        {
            get { return this.showDetails; }
            set
            {
                this.showDetails = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool showDetails;

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

        public ICommand DownloadCommand { get; set; }
        public ICommand ReviewCommand { get; set; }
        public ICommand ReportCommand { get; set; }

        private bool firstLoadCompleted = false;
        private DateTimeOffset lastCategoryRefresh = DateTimeOffset.MinValue;

        public CommunityCommandsMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            this.BackCommand = this.CreateCommand(async () =>
            {
                if (this.ShowSearch)
                {
                    await this.NavigateToCategories();
                }
                else if (this.ShowDetails)
                {
                    if (this.SearchResults.Count == 0)
                    {
                        await this.NavigateToCategories();
                    }
                    else
                    {
                        this.ClearAllShows();
                        this.ShowSearch = true;
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
                        foreach (CommunityCommandModel command in await ServiceManager.Get<CommunityCommandsService>().SearchCommands(this.SearchText))
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

            this.DetailsCommand = this.CreateCommand(async (id) =>
            {
                try
                {
                    CommunityCommandDetailsModel commandDetails = await ServiceManager.Get<CommunityCommandsService>().GetCommandDetails((Guid)id);
                    if (commandDetails != null)
                    {
                        this.CommandDetails = new CommunityCommandDetailsViewModel(commandDetails);

                        this.ClearAllShows();
                        this.ShowDetails = true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            });

            this.DownloadCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<CommunityCommandsService>().DownloadCommand(this.CommandDetails.ID);
            });

            this.ReviewCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<CommunityCommandsService>().AddReview(new CommunityCommandReviewModel()
                {
                    CommandID = this.CommandDetails.ID,
                    Rating = 5,
                    Review = "Here's a brand new review!!",
                });
            });

            this.ReportCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<CommunityCommandsService>().ReportCommand(this.CommandDetails.ID, "This command has bad stuff in it!");
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
                    foreach (CommunityCommandCategoryModel category in await ServiceManager.Get<CommunityCommandsService>().GetHomeCategories())
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
        }

        private void ClearAllShows()
        {
            this.ShowHome = false;
            this.ShowSearch = false;
            this.ShowDetails = false;
        }
    }
}

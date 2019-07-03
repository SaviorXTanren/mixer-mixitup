using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayGameQueueListItemModel : OverlayListItemModelBase
    {
        public const string HTMLTemplate =
        @"<div style=""position: relative; border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR}; width: {WIDTH}px; height: {HEIGHT}px"">
          <p style=""position: absolute; top: 50%; left: 5%; float: left; text-align: left; font-family: '{TEXT_FONT}'; font-size: {TEXT_HEIGHT}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(0, -50%);"">#{POSITION} {USERNAME}</p>
        </div>";

        private List<UserViewModel> lastUsers = new List<UserViewModel>();

        public OverlayGameQueueListItemModel() : base() { }

        public OverlayGameQueueListItemModel(string htmlText, int totalToShow, string textFont, int width, int height,
            string borderColor, string backgroundColor, string textColor, OverlayItemEffectEntranceAnimationTypeEnum addEventAnimation, OverlayItemEffectExitAnimationTypeEnum removeEventAnimation)
            : base(OverlayItemModelTypeEnum.GameQueue, htmlText, totalToShow, textFont, width, height, borderColor, backgroundColor, textColor, addEventAnimation, removeEventAnimation)
        { }

        public override async Task LoadTestData()
        {
            UserViewModel user = await ChannelSession.GetCurrentUser();

            List<UserViewModel> users = new List<UserViewModel>();
            for (int i = 0; i < this.TotalToShow; i++)
            {
                users.Add(user);
            }
            await this.AddGameQueueUsers(users);
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnGameQueueUpdated += GlobalEvents_OnGameQueueUpdated;

            await base.Initialize();
        }

        public override async Task Disable()
        {
            GlobalEvents.OnGameQueueUpdated -= GlobalEvents_OnGameQueueUpdated;

            await base.Disable();
        }

        private async void GlobalEvents_OnGameQueueUpdated(object sender, System.EventArgs e)
        {
            await this.AddGameQueueUsers(ChannelSession.Services.GameQueueService.Queue);
        }

        private async Task AddGameQueueUsers(IEnumerable<UserViewModel> users)
        {
            await this.listSemaphore.WaitAndRelease(() =>
            {
                foreach (UserViewModel user in this.lastUsers)
                {
                    if (!users.Contains(user))
                    {
                        this.Items.Add(OverlayListIndividualItemModel.CreateRemoveItem(user.ID.ToString()));
                    }
                }

                for (int i = 0; i < users.Count(); i++)
                {
                    UserViewModel user = users.ElementAt(i);

                    OverlayListIndividualItemModel item = null;

                    int lastIndex = this.lastUsers.IndexOf(user);
                    if (lastIndex < 0 || lastIndex != i)
                    {
                        item = OverlayListIndividualItemModel.CreateAddItem(user.ID.ToString(), user, i + 1, this.HTML);

                        item.TemplateReplacements.Add("USERNAME", user.UserName);
                        item.TemplateReplacements.Add("POSITION", (i + 1).ToString());

                        this.Items.Add(item);
                    }
                }

                this.lastUsers = new List<UserViewModel>(users);

                this.SendUpdateRequired();
                return Task.FromResult(0);
            });
        }
    }
}

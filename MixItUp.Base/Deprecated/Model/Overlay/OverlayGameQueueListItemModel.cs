using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [Obsolete]
    [DataContract]
    public class OverlayGameQueueListItemModel : OverlayListItemModelBase
    {
        public const string HTMLTemplate =
        @"<div style=""position: relative; border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR}; width: {WIDTH}px; height: {HEIGHT}px"">
          <p style=""position: absolute; top: 50%; left: 5%; float: left; text-align: left; font-family: '{TEXT_FONT}'; font-size: {TEXT_HEIGHT}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(0, -50%);"">#{POSITION} {USERNAME}</p>
        </div>";

        private List<UserV2ViewModel> lastUsers = new List<UserV2ViewModel>();

        public OverlayGameQueueListItemModel() : base() { }

        public OverlayGameQueueListItemModel(string htmlText, int totalToShow, string textFont, int width, int height, string borderColor, string backgroundColor, string textColor,
            OverlayListItemAlignmentTypeEnum alignment, OverlayItemEffectEntranceAnimationTypeEnum addEventAnimation, OverlayItemEffectExitAnimationTypeEnum removeEventAnimation)
            : base(OverlayItemModelTypeEnum.GameQueue, htmlText, totalToShow, 0, textFont, width, height, borderColor, backgroundColor, textColor, alignment, addEventAnimation, removeEventAnimation)
        { }

        public override async Task LoadTestData()
        {
            UserV2ViewModel user = ChannelSession.User;

            List<CommandParametersModel> parameters = new List<CommandParametersModel>();
            for (int i = 0; i < this.TotalToShow; i++)
            {
                parameters.Add(new CommandParametersModel(user));
            }
            await this.AddGameQueueUsers(parameters);
        }

        public override async Task Enable()
        {
            GameQueueService.OnGameQueueUpdated += GameQueueService_OnGameQueueUpdated;

            await base.Enable();
        }

        public override async Task Disable()
        {
            this.lastUsers.Clear();

            GameQueueService.OnGameQueueUpdated -= GameQueueService_OnGameQueueUpdated;

            await base.Disable();
        }

        private async void GameQueueService_OnGameQueueUpdated(object sender, System.EventArgs e)
        {
            await this.AddGameQueueUsers(ServiceManager.Get<GameQueueService>().Queue);
        }

        private async Task AddGameQueueUsers(IEnumerable<CommandParametersModel> parameters)
        {
            await this.listSemaphore.WaitAsync();

            foreach (UserV2ViewModel user in this.lastUsers)
            {
                if (!parameters.Select(p => p.User).Contains(user))
                {
                    this.Items.Add(OverlayListIndividualItemModel.CreateRemoveItem(user.ID.ToString()));
                }
            }

            for (int i = 0; i < parameters.Count() && i < this.TotalToShow; i++)
            {
                CommandParametersModel p = parameters.ElementAt(i);

                OverlayListIndividualItemModel item = OverlayListIndividualItemModel.CreateAddItem(p.User.ID.ToString(), p.User, i + 1, this.HTML);
                item.TemplateReplacements.Add("USERNAME", (string)p.User.DisplayName);
                item.TemplateReplacements.Add("POSITION", (i + 1).ToString());

                this.Items.Add(item);
            }

            this.lastUsers = new List<UserV2ViewModel>(parameters.Select(p => p.User));

            this.SendUpdateRequired();

            this.listSemaphore.Release();
        }
    }
}

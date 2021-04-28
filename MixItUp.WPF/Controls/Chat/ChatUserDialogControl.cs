using MixItUp.Base;
using MixItUp.Base.Model;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Dialogs;
using MixItUp.WPF.Windows.Users;
using StreamingClient.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Chat
{
    public static class ChatUserDialogControl
    {
        public static async Task ShowUserDialog(UserViewModel user)
        {
            if (user != null && !user.IsAnonymous)
            {
                object result = await DialogHelper.ShowCustom(new UserDialogControl(user));
                if (result != null)
                {
                    UserDialogResult dialogResult = EnumHelper.GetEnumValueFromString<UserDialogResult>(result.ToString());
                    switch (dialogResult)
                    {
                        case UserDialogResult.Purge:
                            await ServiceManager.Get<ChatService>().PurgeUser(user);
                            break;
                        case UserDialogResult.Timeout1:
                            await ServiceManager.Get<ChatService>().TimeoutUser(user, 60);
                            break;
                        case UserDialogResult.Timeout5:
                            await ServiceManager.Get<ChatService>().TimeoutUser(user, 300);
                            break;
                        case UserDialogResult.Ban:
                            if (await DialogHelper.ShowConfirmation(string.Format(Resources.BanUserPrompt, user.DisplayName)))
                            {
                                await ServiceManager.Get<ChatService>().BanUser(user);
                            }
                            break;
                        case UserDialogResult.Unban:
                            await ServiceManager.Get<ChatService>().UnbanUser(user);
                            break;
                        case UserDialogResult.Follow:
                            if (user.Platform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSessionService>().IsConnected)
                            {
                                await ServiceManager.Get<TwitchSessionService>().UserConnection.FollowUser(ServiceManager.Get<TwitchSessionService>().UserNewAPI, user.GetTwitchNewAPIUserModel());
                            }
                            break;
                        case UserDialogResult.Unfollow:
                            if (user.Platform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSessionService>().IsConnected)
                            {
                                await ServiceManager.Get<TwitchSessionService>().UserConnection.UnfollowUser(ServiceManager.Get<TwitchSessionService>().UserNewAPI, user.GetTwitchNewAPIUserModel());
                            }
                            break;
                        case UserDialogResult.PromoteToMod:
                            if (await DialogHelper.ShowConfirmation(string.Format(Resources.PromoteUserPrompt, user.DisplayName)))
                            {
                                await ServiceManager.Get<ChatService>().ModUser(user);
                            }
                            break;
                        case UserDialogResult.DemoteFromMod:
                            if (await DialogHelper.ShowConfirmation(string.Format(Resources.DemoteUserPrompt, user.DisplayName)))
                            {
                                await ServiceManager.Get<ChatService>().UnmodUser(user);
                            }
                            break;
                        case UserDialogResult.ChannelPage:
                            ProcessHelper.LaunchLink(user.ChannelLink);
                            break;
                        case UserDialogResult.EditUser:
                            UserDataModel userData = ChannelSession.Settings.GetUserData(user.ID);
                            if (userData != null)
                            {
                                UserDataEditorWindow window = new UserDataEditorWindow(userData);
                                await Task.Delay(100);
                                window.Show();
                                await Task.Delay(100);
                                window.Focus();
                            }
                            break;
                        case UserDialogResult.Close:
                        default:
                            // Just close
                            break;
                    }
                }
            }
        }
    }
}

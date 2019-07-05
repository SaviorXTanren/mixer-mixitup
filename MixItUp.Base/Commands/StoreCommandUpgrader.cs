using MixItUp.Base.Actions;
using MixItUp.Base.Model.Overlay;
using System;
using System.Collections.Generic;

namespace MixItUp.Base.Commands
{
    public static class StoreCommandUpgrader
    {
        public static void PerformUpgrades(List<ActionBase> actions, string version)
        {
            if (StoreCommandUpgrader.IsVersionLessThan(version, "0.5.1.501"))
            {
                StoreCommandUpgrader.RestructureNewerOverlayActions(actions);
            }
        }

        internal static void RestructureNewerOverlayActions(List<ActionBase> actions)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                ActionBase action = actions[i];
                if (action is OverlayAction)
                {
                    OverlayAction oAction = (OverlayAction)action;
#pragma warning disable CS0612 // Type or member is obsolete
                    if (oAction.Item != null)
                    {
                        oAction.OverlayItem = ConvertOverlayItem(oAction.Item);
                        oAction.OverlayItem.Position = new OverlayItemPositionModel((OverlayItemPositionType)oAction.Position.PositionType, oAction.Position.Horizontal, oAction.Position.Vertical, 0);
                        oAction.OverlayItem.Effects = new OverlayItemEffectsModel((OverlayItemEffectEntranceAnimationTypeEnum)oAction.Effects.EntranceAnimation, (OverlayItemEffectVisibleAnimationTypeEnum)oAction.Effects.VisibleAnimation,
                            (OverlayItemEffectExitAnimationTypeEnum)oAction.Effects.ExitAnimation, oAction.Effects.Duration);

                        oAction.Item = null;
                        oAction.Position = null;
                        oAction.Effects = null;
                        oAction.Effect = null;
                    }
#pragma warning restore CS0612 // Type or member is obsolete
                }
            }
        }

#pragma warning disable CS0612 // Type or member is obsolete
        public static OverlayItemModelBase ConvertOverlayItem(OverlayItemBase item)
        {
            if (item is OverlayTextItem)
            {
                OverlayTextItem oItem = (OverlayTextItem)item;
                return new OverlayTextItemModel(oItem.Text, oItem.Color, oItem.Size, oItem.Font, oItem.Bold, oItem.Italic, oItem.Underline, oItem.ShadowColor);
            }
            else if (item is OverlayImageItem)
            {
                OverlayImageItem oItem = (OverlayImageItem)item;
                return new OverlayImageItemModel(oItem.FilePath, oItem.Width, oItem.Height);
            }
            else if (item is OverlayVideoItem)
            {
                OverlayVideoItem oItem = (OverlayVideoItem)item;
                return new OverlayVideoItemModel(oItem.FilePath, oItem.Width, oItem.Height, oItem.Volume);
            }
            else if (item is OverlayYouTubeItem)
            {
                OverlayYouTubeItem oItem = (OverlayYouTubeItem)item;
                return new OverlayYouTubeItemModel(oItem.VideoID, oItem.Width, oItem.Height, oItem.StartTime, oItem.Volume);
            }
            else if (item is OverlayWebPageItem)
            {
                OverlayWebPageItem oItem = (OverlayWebPageItem)item;
                return new OverlayWebPageItemModel(oItem.URL, oItem.Width, oItem.Height);
            }
            else if (item is OverlayHTMLItem)
            {
                OverlayHTMLItem oItem = (OverlayHTMLItem)item;
                return new OverlayHTMLItemModel(oItem.HTMLText);
            }
            else if (item is OverlayChatMessages)
            {
                OverlayChatMessages oItem = (OverlayChatMessages)item;
                return new OverlayChatMessagesListItemModel(oItem.HTMLText, oItem.TotalToShow, oItem.TextFont, oItem.Width, oItem.TextSize, oItem.BorderColor, oItem.BackgroundColor,
                    oItem.TextColor, (OverlayItemEffectEntranceAnimationTypeEnum)oItem.AddEventAnimation, OverlayItemEffectExitAnimationTypeEnum.None);
            }
            else if (item is OverlayEventList)
            {
                OverlayEventList oItem = (OverlayEventList)item;
                List<OverlayEventListItemTypeEnum> types = new List<OverlayEventListItemTypeEnum>();
                foreach (EventListItemTypeEnum type in oItem.ItemTypes)
                {
                    types.Add((OverlayEventListItemTypeEnum)type);
                }
                return new OverlayEventListItemModel(oItem.HTMLText, types, oItem.TotalToShow, oItem.TextFont, oItem.Width, oItem.Height, oItem.BorderColor, oItem.BackgroundColor,
                    oItem.TextColor, (OverlayItemEffectEntranceAnimationTypeEnum)oItem.AddEventAnimation, (OverlayItemEffectExitAnimationTypeEnum)oItem.RemoveEventAnimation);
            }
            else if (item is OverlayGameQueue)
            {
                OverlayGameQueue oItem = (OverlayGameQueue)item;
                return new OverlayGameQueueListItemModel(oItem.HTMLText, oItem.TotalToShow, oItem.TextFont, oItem.Width, oItem.Height, oItem.BorderColor, oItem.BackgroundColor,
                    oItem.TextColor, (OverlayItemEffectEntranceAnimationTypeEnum)oItem.AddEventAnimation, (OverlayItemEffectExitAnimationTypeEnum)oItem.RemoveEventAnimation);
            }
            else if (item is OverlayLeaderboard)
            {
                OverlayLeaderboard oItem = (OverlayLeaderboard)item;
                OverlayLeaderboardListItemModel result = new OverlayLeaderboardListItemModel(oItem.HTMLText, (OverlayLeaderboardListItemTypeEnum)oItem.LeaderboardType, oItem.TotalToShow,
                    oItem.TextFont, oItem.Width, oItem.Height, oItem.BorderColor, oItem.BackgroundColor, oItem.TextColor, (OverlayItemEffectEntranceAnimationTypeEnum)oItem.AddEventAnimation,
                    (OverlayItemEffectExitAnimationTypeEnum)oItem.RemoveEventAnimation);
                result.CurrencyID = oItem.CurrencyID;
                result.LeaderboardDateRange = (OverlayLeaderboardListItemDateRangeEnum)oItem.DateRange;
                return result;
            }
            else if (item is OverlayProgressBar)
            {
                OverlayProgressBar oItem = (OverlayProgressBar)item;
                OverlayProgressBarItemModel result = new OverlayProgressBarItemModel(oItem.HTMLText, (OverlayProgressBarItemTypeEnum)oItem.ProgressBarType, oItem.ResetAfterDays,
                    oItem.ProgressColor, oItem.BackgroundColor, oItem.TextColor, oItem.TextFont, oItem.Width, oItem.Height, oItem.GoalReachedCommand);
                result.StartAmount = result.CurrentAmount = oItem.CurrentAmountNumber;
                result.GoalAmount = oItem.GoalAmountNumber;
                result.CurrentAmountCustom = oItem.CurrentAmountCustom;
                result.GoalAmountCustom = oItem.GoalAmountCustom;
                return result;
            }
            else if (item is OverlaySongRequests)
            {
                OverlaySongRequests oItem = (OverlaySongRequests)item;
                return new OverlaySongRequestsListItemModel(oItem.HTMLText, oItem.TotalToShow, oItem.TextFont, oItem.Width, oItem.Height, oItem.BorderColor, oItem.BackgroundColor,
                    oItem.TextColor, (OverlayItemEffectEntranceAnimationTypeEnum)oItem.AddEventAnimation, (OverlayItemEffectExitAnimationTypeEnum)oItem.RemoveEventAnimation);
            }
            else if (item is OverlayStreamBoss)
            {
                OverlayStreamBoss oItem = (OverlayStreamBoss)item;
                return new OverlayStreamBossItemModel(oItem.HTMLText, oItem.StartingHealth, oItem.Width, oItem.Height, oItem.TextColor, oItem.TextFont, oItem.BorderColor, oItem.BackgroundColor,
                    oItem.ProgressColor, oItem.FollowBonus, oItem.HostBonus, oItem.SubscriberBonus, oItem.DonationBonus, oItem.SparkBonus, oItem.EmberBonus, 1.0, 1.0,
                    (OverlayItemEffectVisibleAnimationTypeEnum)oItem.DamageAnimation, (OverlayItemEffectVisibleAnimationTypeEnum)oItem.NewBossAnimation, oItem.NewStreamBossCommand);
            }
            else if (item is OverlayMixerClip)
            {
                OverlayMixerClip oItem = (OverlayMixerClip)item;
                return new OverlayStreamClipItemModel(oItem.Width, oItem.Height, oItem.Volume, (OverlayItemEffectEntranceAnimationTypeEnum)oItem.EntranceAnimation, (OverlayItemEffectExitAnimationTypeEnum)oItem.ExitAnimation);
            }
            else if (item is OverlayTimer)
            {
                OverlayTimer oItem = (OverlayTimer)item;
                return new OverlayTimerItemModel(oItem.HTMLText, oItem.TotalLength, oItem.TextColor, oItem.TextFont, oItem.TextSize, oItem.TimerCompleteCommand);
            }
            else if (item is OverlayTimerTrain)
            {
                OverlayTimerTrain oItem = (OverlayTimerTrain)item;
                return new OverlayTimerTrainItemModel(oItem.HTMLText, oItem.MinimumSecondsToShow, oItem.TextColor, oItem.TextFont, oItem.TextSize, oItem.FollowBonus, oItem.HostBonus, oItem.SubscriberBonus, oItem.DonationBonus, oItem.SparkBonus, oItem.EmberBonus);
            }
            return null;
        }
#pragma warning restore CS0612 // Type or member is obsolete

        private static bool IsVersionLessThan(string currentVersion, string checkVersion)
        {
            Version current = Version.Parse(currentVersion.Trim());
            Version check = Version.Parse(checkVersion);
            return current.CompareTo(check) < 0;
        }
    }
}

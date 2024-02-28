﻿using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.Services.YouTube;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.ObjectModel;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayEventTrackingYouTubeMembershipViewModel : UIViewModelBase
    {
        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                this.NotifyPropertyChanged();
            }
        }
        private string name;

        public int Amount
        {
            get { return this.damageAmount; }
            set
            {
                this.damageAmount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int damageAmount;

        public OverlayEventTrackingYouTubeMembershipViewModel(string name, int amount)
        {
            this.Name = name;
            this.Amount = amount;
        }
    }

    public abstract class OverlayEventTrackingV3ViewModelBase : OverlayVisualTextV3ViewModelBase
    {
        private const double SampleIntegerAmount = 123;
        private const double SampleDecimalAmount = 12.34;

        public abstract string EquationUnits { get; }

        public int FollowAmount
        {
            get { return this.followAmount; }
            set
            {
                this.followAmount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int followAmount;

        public int RaidAmount
        {
            get { return this.raidAmount; }
            set
            {
                this.raidAmount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.RaidEquation));
            }
        }
        private int raidAmount;

        public double RaidPerViewAmount
        {
            get { return this.raidPerViewAmount; }
            set
            {
                this.raidPerViewAmount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.RaidEquation));
            }
        }
        private double raidPerViewAmount;

        public string RaidEquation
        {
            get
            {
                int total = (int)Math.Round(this.RaidAmount + (this.RaidPerViewAmount * SampleIntegerAmount));
                return $"{this.RaidAmount} + ({this.RaidPerViewAmount} * {SampleIntegerAmount} {Resources.Viewers}) = {total} {this.EquationUnits}";
            }
        }

        public int TwitchSubscriptionTier1Amount
        {
            get { return this.twitchSubscriptionTier1Amount; }
            set
            {
                this.twitchSubscriptionTier1Amount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int twitchSubscriptionTier1Amount;

        public int TwitchSubscriptionTier2Amount
        {
            get { return this.twitchSubscriptionTier2Amount; }
            set
            {
                this.twitchSubscriptionTier2Amount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int twitchSubscriptionTier2Amount;

        public int TwitchSubscriptionTier3Amount
        {
            get { return this.twitchSubscriptionTier3Amount; }
            set
            {
                this.twitchSubscriptionTier3Amount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int twitchSubscriptionTier3Amount;

        public ObservableCollection<OverlayEventTrackingYouTubeMembershipViewModel> YouTubeMemberships { get; set; } = new ObservableCollection<OverlayEventTrackingYouTubeMembershipViewModel>();

        public int TrovoSubscriptionTier1Amount
        {
            get { return this.trovoSubscriptionTier1Amount; }
            set
            {
                this.trovoSubscriptionTier1Amount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int trovoSubscriptionTier1Amount;

        public int TrovoSubscriptionTier2Amount
        {
            get { return this.trovoSubscriptionTier2Amount; }
            set
            {
                this.trovoSubscriptionTier2Amount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int trovoSubscriptionTier2Amount;

        public int TrovoSubscriptionTier3Amount
        {
            get { return this.trovoSubscriptionTier3Amount; }
            set
            {
                this.trovoSubscriptionTier3Amount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int trovoSubscriptionTier3Amount;

        public double TwitchBitsAmount
        {
            get { return this.twitchBitsAmount; }
            set
            {
                this.twitchBitsAmount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.TwitchBitsEquation));
            }
        }
        private double twitchBitsAmount;

        public string TwitchBitsEquation
        {
            get
            {
                int total = (int)Math.Round(this.TwitchBitsAmount * SampleIntegerAmount);
                return $"{this.TwitchBitsAmount} * {SampleIntegerAmount} {Resources.Bits} = {total} {this.EquationUnits}";
            }
        }

        public double YouTubeSuperChatAmount
        {
            get { return this.youTubeSuperChatAmount; }
            set
            {
                this.youTubeSuperChatAmount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.YouTubeSuperChatEquation));
            }
        }
        private double youTubeSuperChatAmount;

        public string YouTubeSuperChatEquation
        {
            get
            {
                int total = (int)Math.Round(this.YouTubeSuperChatAmount * SampleDecimalAmount);
                return $"{this.YouTubeSuperChatAmount} * {CurrencyHelper.ToCurrencyString(SampleDecimalAmount)} = {total} {this.EquationUnits}";
            }
        }

        public double TrovoElixirSpellAmount
        {
            get { return this.trovoElixirSpellAmount; }
            set
            {
                this.trovoElixirSpellAmount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.TrovoElixirSpellEquation));
            }
        }
        private double trovoElixirSpellAmount;

        public string TrovoElixirSpellEquation
        {
            get
            {
                int total = (int)Math.Round(this.TrovoElixirSpellAmount * SampleIntegerAmount);
                return $"{this.TrovoElixirSpellAmount} * {SampleIntegerAmount} {Resources.Elixir} = {total} {this.EquationUnits}";
            }
        }

        public double DonationAmount
        {
            get { return this.donationAmount; }
            set
            {
                this.donationAmount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(DonationEquation));
            }
        }
        private double donationAmount;

        public string DonationEquation
        {
            get
            {
                int total = (int)Math.Round(this.DonationAmount * SampleDecimalAmount);
                return $"{this.DonationAmount} * {CurrencyHelper.ToCurrencyString(SampleDecimalAmount)} = {total} {this.EquationUnits}";
            }
        }

        public OverlayEventTrackingV3ViewModelBase(OverlayItemV3Type type)
            : base(type)
        {
            if (ServiceManager.Get<YouTubeSessionService>().IsConnected)
            {
                foreach (MembershipsLevel membershipsLevel in ServiceManager.Get<YouTubeSessionService>().MembershipLevels)
                {
                    this.YouTubeMemberships.Add(new OverlayEventTrackingYouTubeMembershipViewModel(membershipsLevel.Snippet.LevelDetails.DisplayName, 0));
                }
            }
        }

        public OverlayEventTrackingV3ViewModelBase(OverlayEventCountingV3ModelBase item)
            : base(item)
        {
            this.FollowAmount = item.FollowAmount;

            this.RaidAmount = item.RaidAmount;
            this.RaidPerViewAmount = item.RaidPerViewAmount;

            this.TwitchSubscriptionTier1Amount = item.TwitchSubscriptionsAmount[1];
            this.TwitchSubscriptionTier2Amount = item.TwitchSubscriptionsAmount[2];
            this.TwitchSubscriptionTier3Amount = item.TwitchSubscriptionsAmount[3];
            this.TwitchBitsAmount = item.TwitchBitsAmount;

            if (ServiceManager.Get<YouTubeSessionService>().IsConnected)
            {
                foreach (MembershipsLevel membershipsLevel in ServiceManager.Get<YouTubeSessionService>().MembershipLevels)
                {
                    if (item.YouTubeMembershipsAmount.TryGetValue(membershipsLevel.Snippet.LevelDetails.DisplayName, out int damageAmount))
                    {
                        this.YouTubeMemberships.Add(new OverlayEventTrackingYouTubeMembershipViewModel(membershipsLevel.Snippet.LevelDetails.DisplayName, damageAmount));
                    }
                    else
                    {
                        this.YouTubeMemberships.Add(new OverlayEventTrackingYouTubeMembershipViewModel(membershipsLevel.Snippet.LevelDetails.DisplayName, 0));
                    }
                }
            }
            this.YouTubeSuperChatAmount = item.YouTubeSuperChatAmount;

            this.TrovoSubscriptionTier1Amount = item.TrovoSubscriptionsAmount[1];
            this.TrovoSubscriptionTier2Amount = item.TrovoSubscriptionsAmount[2];
            this.TrovoSubscriptionTier3Amount = item.TrovoSubscriptionsAmount[3];
            this.TrovoElixirSpellAmount = item.TrovoElixirSpellAmount;

            this.DonationAmount = item.DonationAmount;
        }

        public override Result Validate()
        {
            return new Result();
        }

        protected void AssignProperties(OverlayEventCountingV3ModelBase result)
        {
            base.AssignProperties(result);

            result.FollowAmount = this.FollowAmount;

            result.RaidAmount = this.RaidAmount;
            result.RaidPerViewAmount = this.RaidPerViewAmount;

            result.TwitchSubscriptionsAmount[1] = this.TwitchSubscriptionTier1Amount;
            result.TwitchSubscriptionsAmount[2] = this.TwitchSubscriptionTier2Amount;
            result.TwitchSubscriptionsAmount[3] = this.TwitchSubscriptionTier3Amount;
            result.TwitchBitsAmount = this.TwitchBitsAmount;

            result.YouTubeMembershipsAmount.Clear();
            foreach (OverlayEventTrackingYouTubeMembershipViewModel membership in this.YouTubeMemberships)
            {
                result.YouTubeMembershipsAmount[membership.Name] = membership.Amount;
            }
            result.YouTubeSuperChatAmount = this.YouTubeSuperChatAmount;

            result.TrovoSubscriptionsAmount[1] = this.TrovoSubscriptionTier1Amount;
            result.TrovoSubscriptionsAmount[2] = this.TrovoSubscriptionTier2Amount;
            result.TrovoSubscriptionsAmount[3] = this.TrovoSubscriptionTier3Amount;
            result.TrovoElixirSpellAmount = this.TrovoElixirSpellAmount;

            result.DonationAmount = this.DonationAmount;
        }
    }
}
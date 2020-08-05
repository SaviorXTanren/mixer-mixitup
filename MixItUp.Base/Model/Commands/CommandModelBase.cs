using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Commands
{
    public enum CommandTypeEnum
    {
        Custom = 0,
        Chat = 1,
        Event = 2,
        Timer = 3,
        ActionGroup = 4,
        Game = 5,
        Remote = 6,
        TwitchChannelPoints = 7,
    }

    [DataContract]
    public abstract class CommandModelBase
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public CommandTypeEnum Type { get; set; }

        [DataMember]
        public bool IsEnabled { get; set; }

        [DataMember]
        public bool Unlocked { get; set; }

        [DataMember]
        public RequirementsSetModel Requirements { get; set; }

        [DataMember]
        public List<ActionModelBase> Actions { get; set; } = new List<ActionModelBase>();

        public CommandModelBase(string name, CommandTypeEnum type)
        {
            this.ID = Guid.NewGuid();
            this.IsEnabled = true;
            this.Name = name;
            this.Type = type;
        }

        [JsonIgnore]
        protected abstract SemaphoreSlim CommandLockSemaphore { get; }

        protected bool IsUnlocked { get { return this.Unlocked || ChannelSession.Settings.UnlockAllCommands; } }

        public void PerformBackground() { this.PerformBackground(ChannelSession.GetCurrentUser()); }

        public void PerformBackground(UserViewModel user) { this.PerformBackground(user, StreamingPlatformTypeEnum.None, null, null); }

        public void PerformBackground(UserViewModel user, IEnumerable<string> arguments) { this.PerformBackground(user, StreamingPlatformTypeEnum.None, arguments, null); }

        public void PerformBackground(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers) { this.PerformBackground(user, StreamingPlatformTypeEnum.None, arguments, specialIdentifiers); }

        public void PerformBackground(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            Task.Run(() => this.Perform(user, platform, arguments, specialIdentifiers));
        }

        public async Task Perform() { await this.Perform(ChannelSession.GetCurrentUser()); }

        public async Task Perform(UserViewModel user) { await this.Perform(user, StreamingPlatformTypeEnum.None, null, null); }

        public async Task Perform(UserViewModel user, IEnumerable<string> arguments) { await this.Perform(user, StreamingPlatformTypeEnum.None, arguments, null); }

        public async Task Perform(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers) { await this.Perform(user, StreamingPlatformTypeEnum.None, arguments, specialIdentifiers); }

        public async Task Perform(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            try
            {
                if (this.IsEnabled && this.DoesCommandHaveWork)
                {
                    Logger.Log(LogLevel.Debug, $"Starting command performing: {this}");

                    ChannelSession.Services.Telemetry.TrackCommand(this.Type);

                    if (user == null)
                    {
                        user = ChannelSession.GetCurrentUser();
                    }

                    if (arguments == null)
                    {
                        arguments = new List<string>();
                    }

                    if (specialIdentifiers == null)
                    {
                        specialIdentifiers = new Dictionary<string, string>();
                    }

                    if (platform == StreamingPlatformTypeEnum.None)
                    {
                        platform = user.Platform;
                    }

                    await this.CommandLockSemaphore.WaitAsync();

                    List<UserViewModel> users = new List<UserViewModel>() { user };
                    if (this.Requirements != null)
                    {
                        if (!await this.Requirements.Validate(user))
                        {
                            return;
                        }
                        await this.Requirements.Perform(user);

                        users = new List<UserViewModel>(this.Requirements.GetPerformingUsers(user));
                    }

                    if (this.IsUnlocked)
                    {
                        this.CommandLockSemaphore.Release();
                    }

                    this.TrackTelemetry();

                    foreach (UserViewModel u in users)
                    {
                        u.Data.TotalCommandsRun++;
                        await this.PerformInternal(u, platform, arguments, specialIdentifiers);
                    }

                    if (!this.IsUnlocked)
                    {
                        this.CommandLockSemaphore.Release();
                    }
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex) { Logger.Log(ex); }
            finally
            {
                if (this.CommandLockSemaphore.CurrentCount == 0)
                {
                    this.CommandLockSemaphore.Release();
                }
            }
        }

        public override string ToString() { return string.Format("{0} - {1}", this.ID, this.Name); }

        public int CompareTo(object obj)
        {
            if (obj is CommandModelBase)
            {
                return this.CompareTo((CommandModelBase)obj);
            }
            return 0;
        }

        public int CompareTo(CommandModelBase other) { return this.Name.CompareTo(other.Name); }

        public override bool Equals(object obj)
        {
            if (obj is CommandModelBase)
            {
                return this.Equals((CommandModelBase)obj);
            }
            return false;
        }

        public bool Equals(CommandModelBase other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }

        public virtual bool DoesCommandHaveWork { get { return this.Actions.Count > 0; } }

        protected virtual async Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            List<ActionModelBase> actionsToRun = new List<ActionModelBase>(this.Actions);
            for (int i = 0; i < actionsToRun.Count; i++)
            {
                ActionModelBase action = actionsToRun[i];
                //if (action is OverlayAction && ChannelSession.Services.Overlay.IsConnected)
                //{
                //    ChannelSession.Services.Overlay.StartBatching();
                //}

                await action.Perform(user, platform, arguments, specialIdentifiers);

                //if (action is OverlayAction && ChannelSession.Services.Overlay.IsConnected)
                //{
                //    if (i == (actionsToRun.Count - 1) || !(actionsToRun[i + 1] is OverlayAction))
                //    {
                //        await ChannelSession.Services.Overlay.EndBatching();
                //    }
                //}
            }
        }

        protected virtual void TrackTelemetry() { ChannelSession.Services.Telemetry.TrackCommand(this.Type); }
    }
}

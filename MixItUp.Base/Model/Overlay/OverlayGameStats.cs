using Mixer.Base.Util;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum GameStatsPlatformTypeEnum
    {
        PC,
        Xbox,
        Playstation
    }

    [DataContract]
    public class GameStatsSetupBase
    {
        public const string DefaultHTMLTemplate =
        @"<table cellpadding=""10"" style=""border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR};"">
            <tbody>
            <tr>
                <td colspan=""3"" style=""padding: 10px;"">
                    <div style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; color: {TEXT_COLOR}; text-align: center;"">TOTAL</div>
                </td>
            </tr>
            <tr>
                <td style=""padding: 10px;"">
                    <span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; color: {TEXT_COLOR};"">Kills:</span>
                </td>
                <td style=""padding: 10px;"">
                    <span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; color: {TEXT_COLOR}; float: right"">{TOTAL_KILLS}</span>
                </td>
                <td style=""padding: 10px;"">
                    <span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; color: {TEXT_COLOR}; float: right"">{TOTAL_KD}</span>
                </td>
            </tr>
            <tr>
                <td style=""padding: 10px;"">
                    <span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; color: {TEXT_COLOR};"">Wins:</span>
                </td>
                <td style=""padding: 10px;"">
                    <span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; color: {TEXT_COLOR}; float: right"">{TOTAL_WINS}</span>
                </td>
                <td style=""padding: 10px;"">
                    <span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; color: {TEXT_COLOR}; float: right"">{TOTAL_WL}</span>
                </td>
            </tr>
            <tr>
                <td colspan=""3"" style=""padding: 10px;"">
                    <div style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; color: {TEXT_COLOR}; text-align: center;"">SESSION</div>
                </td>
            </tr>
            <tr>
                <td style=""padding: 10px;"">
                    <span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; color: {TEXT_COLOR};"">Kills:</span>
                </td>
                <td style=""padding: 10px;"">
                    <span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; color: {TEXT_COLOR}; float: right"">{SESSION_KILLS}</span>
                </td>
                <td style=""padding: 10px;"">
                    <span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; color: {TEXT_COLOR}; float: right"">{SESSION_KD}</span>
                </td>
            </tr>
            <tr>
                <td style=""padding: 10px;"">
                    <span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; color: {TEXT_COLOR};"">Wins:</span>
                </td>
                <td style=""padding: 10px;"">
                    <span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; color: {TEXT_COLOR}; float: right"">{SESSION_WINS}</span>
                </td>
                <td style=""padding: 10px;"">
                    <span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; color: {TEXT_COLOR}; float: right"">{SESSION_WL}</span>
                </td>
            </tr>
            </tbody>
        </table>";

        public virtual string Name { get { return string.Empty; } }

        public string Username { get; set; }

        public GameStatsPlatformTypeEnum Platform { get; set; }

        public string Category { get; set; }

        public GameStatsSetupBase() { }

        protected GameStatsSetupBase(string username, GameStatsPlatformTypeEnum platform)
        {
            this.Username = username;
            this.Platform = platform;
        }

        protected GameStatsSetupBase(string username, GameStatsPlatformTypeEnum platform, string category) : this(username, platform) { this.Category = category; }

        public virtual Task Initialize() { return Task.FromResult(0); }

        public virtual Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            return Task.FromResult(new Dictionary<string, string>());
        }

        protected async Task<string> GetAsync(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    return await client.GetStringAsync(url);
                }
            }
            catch (Exception ex) { Util.Logger.Log(ex); }
            return null;
        }
    }

    [DataContract]
    public class RainboxSixSiegeGameStatsSetup : GameStatsSetupBase
    {
        public const string GameName = "Rainbox Six Siege";

        private ScoutUser user;

        private int initialKills = 0;
        private int initialDeaths = 0;
        private int initialWins = 0;
        private int initialLoses = 0;

        public override string Name { get { return GameName; } }

        public RainboxSixSiegeGameStatsSetup() { }

        public RainboxSixSiegeGameStatsSetup(string username, GameStatsPlatformTypeEnum platform) : base(username, platform) { }

        public override async Task Initialize()
        {
            string platformString = "";
            switch (this.Platform)
            {
                case GameStatsPlatformTypeEnum.PC: platformString = "uplay"; break;
                case GameStatsPlatformTypeEnum.Xbox: platformString = "xbl"; break;
                case GameStatsPlatformTypeEnum.Playstation: platformString = "psn"; break;
            }

            this.user = await ChannelSession.Services.Scout.GetUser("r6siege", this.Username, new Dictionary<string, string>()
            {
                { "platform", platformString },
            });
        }

        public override async Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            if (this.user != null)
            {
                Dictionary<string, ScoutStat> stats = await ChannelSession.Services.Scout.GetStats("r6siege", this.user);

                int kills = stats["kills"].ValueInt;
                int deaths = stats["deaths"].ValueInt;
                double kd = (deaths > 0) ? (((double)kills) / ((double)deaths)) : 1.0;
                int wins = stats["matchesWon"].ValueInt;
                int loses = stats["matchesLost"].ValueInt;
                int matches = wins + loses;
                double wl = (matches > 0) ? ((((double)wins) / ((double)matches)) * 100.0) : 1.0;

                replacementSets["TOTAL_KILLS"] = kills.ToString();
                replacementSets["TOTAL_KD"] = Math.Round(kd, 2).ToString("F2");
                replacementSets["TOTAL_WINS"] = wins.ToString();
                replacementSets["TOTAL_WL"] = Math.Round(wl, 2).ToString("F1") + "%";

                if (this.initialWins == 0 && this.initialLoses == 0)
                {
                    this.initialKills = kills;
                    this.initialDeaths = deaths;
                    this.initialWins = wins;
                    this.initialLoses = loses;
                }

                int sessionKills = kills - this.initialKills;
                int sessionDeaths = deaths - this.initialDeaths;
                double sessionkd = (sessionDeaths > 0) ? (((double)sessionKills) / ((double)sessionDeaths)) : 1.0;
                int sessionWins = wins - this.initialWins;
                int sessionLoses = loses - this.initialLoses;
                int sessionmatches = sessionWins + sessionLoses;
                double sessionwl = (sessionmatches > 0) ? ((((double)sessionWins) / ((double)sessionmatches)) * 100.0) : 1.0;

                replacementSets["SESSION_KILLS"] = sessionKills.ToString();
                replacementSets["SESSION_KD"] = Math.Round(sessionkd, 2).ToString("F2");
                replacementSets["SESSION_WINS"] = sessionWins.ToString();
                replacementSets["SESSION_WL"] = Math.Round(sessionwl, 2).ToString("F1") + "%";
            }

            return replacementSets;
        }
    }

    [DataContract]
    public class FortniteGameStatsSetup : GameStatsSetupBase
    {
        private enum FortniteGameStatsCategoryTypeEnum
        {
            Lifetime,
            [Name("Solo Lifetime")]
            SoloLifetime,
            [Name("Duo Lifetime")]
            DuoLifetime,
            [Name("Squad Lifetime")]
            SquadLifetime,
            [Name("Solo Season")]
            SoloSeason,
            [Name("Duo Season")]
            DuoSeason,
            [Name("Squad Season")]
            SquadSeason,
        }

        public const string GameName = "Fortnite";

        public static readonly List<string> Categories = EnumHelper.GetEnumNames<FortniteGameStatsCategoryTypeEnum>().ToList();

        private ScoutUser user;

        private FortniteGameStatsCategoryTypeEnum categoryEnum;

        private int initialKills = 0;
        private int initialWins = 0;
        private int initialMatches = 0;

        public override string Name { get { return GameName; } }

        public FortniteGameStatsSetup() { }

        public FortniteGameStatsSetup(string username, GameStatsPlatformTypeEnum platform, string category) : base(username, platform)
        {
            this.Category = category;
        }

        public override async Task Initialize()
        {
            this.categoryEnum = EnumHelper.GetEnumValueFromString<FortniteGameStatsCategoryTypeEnum>(this.Category);

            string platformString = "";
            string consoleString = null;
            switch (this.Platform)
            {
                case GameStatsPlatformTypeEnum.PC: platformString = "epic"; break;
                case GameStatsPlatformTypeEnum.Xbox: platformString = "xbl"; consoleString = "xb1"; break;
                case GameStatsPlatformTypeEnum.Playstation: platformString = "psn"; consoleString = "ps4"; break;
            }

            this.user = await ChannelSession.Services.Scout.GetUser("fortnite", this.Username, new Dictionary<string, string>()
            {
                { "platform", platformString },
                { "console", consoleString },
            });
        }

        public override async Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            if (this.user != null)
            {
                string segment = null;
                switch (this.categoryEnum)
                {
                    case FortniteGameStatsCategoryTypeEnum.SoloLifetime: segment = "p2.br.m0.alltime"; break;
                    case FortniteGameStatsCategoryTypeEnum.DuoLifetime: segment = "p10.br.m0.alltime"; break;
                    case FortniteGameStatsCategoryTypeEnum.SquadLifetime: segment = "p9.br.m0.alltime"; break;
                    case FortniteGameStatsCategoryTypeEnum.SoloSeason: segment = "p2.br.m0.weekly"; break;
                    case FortniteGameStatsCategoryTypeEnum.DuoSeason: segment = "p10.br.m0.weekly"; break;
                    case FortniteGameStatsCategoryTypeEnum.SquadSeason: segment = "p9.br.m0.weekly"; break;
                }

                Dictionary<string, ScoutStat> stats = await ChannelSession.Services.Scout.GetStats("fortnite", this.user, segment);

                int wins = stats["placeTop1"].ValueInt;
                int matches = stats["matchesPlayed"].ValueInt;
                int kills = stats["kills"].ValueInt;
                int deaths = matches - wins;
                double kd = (deaths > 0) ? (((double)kills) / ((double)deaths)) : 1.0;
                double wl = (matches > 0) ? ((((double)wins) / ((double)matches)) * 100.0) : 1.0;

                replacementSets["TOTAL_KILLS"] = kills.ToString();
                replacementSets["TOTAL_KD"] = Math.Round(kd, 2).ToString("F2");
                replacementSets["TOTAL_WINS"] = wins.ToString();
                replacementSets["TOTAL_WL"] = Math.Round(wl, 2).ToString("F1") + "%";

                if (this.initialWins == 0 && this.initialMatches == 0)
                {
                    this.initialKills = kills;
                    this.initialWins = wins;
                    this.initialMatches = matches;
                }

                int sessionKills = kills - this.initialKills;
                int sessionDeaths = deaths - (this.initialMatches - this.initialWins);
                double sessionkd = (sessionDeaths > 0) ? (((double)sessionKills) / ((double)sessionDeaths)) : 1.0;
                int sessionWins = wins - this.initialWins;
                int sessionLoses = deaths - (this.initialMatches - this.initialWins);
                int sessionmatches = sessionWins + sessionLoses;
                double sessionwl = (sessionmatches > 0) ? ((((double)sessionWins) / ((double)sessionmatches)) * 100.0) : 1.0;

                replacementSets["SESSION_KILLS"] = sessionKills.ToString();
                replacementSets["SESSION_KD"] = Math.Round(sessionkd, 2).ToString("F2");
                replacementSets["SESSION_WINS"] = sessionWins.ToString();
                replacementSets["SESSION_WL"] = Math.Round(sessionwl, 2).ToString("F1") + "%";
            }

            return replacementSets;
        }
    }

    [DataContract]
    public class CallOfDutyBlackOps4GameStatsSetup : GameStatsSetupBase
    {
        private enum CallOfDutyBlackOps4GameStatsCategoryTypeEnum
        {
            [Name("All Modes Lifetime")]
            AllModesLifetime,
            [Name("Blackout Lifetime")]
            BlackoutLifetime,
            [Name("Blackout Solo Lifetime")]
            BlackoutSoloLifetime,
            [Name("Blackout Duo Lifetime")]
            BlackoutDuoLifetime,
            [Name("Blackout Squad Lifetime")]
            BlackoutSquadLifetime,
        }

        public const string GameName = "Call of Duty: Black Ops 4";

        public static readonly List<string> Categories = EnumHelper.GetEnumNames<CallOfDutyBlackOps4GameStatsCategoryTypeEnum>().ToList();

        private ScoutUser user;

        private CallOfDutyBlackOps4GameStatsCategoryTypeEnum categoryEnum;

        private int initialKills = 0;
        private int initialDeaths = 0;
        private int initialWins = 0;
        private int initialMatches = 0;

        public override string Name { get { return GameName; } }

        public CallOfDutyBlackOps4GameStatsSetup() { }

        public CallOfDutyBlackOps4GameStatsSetup(string username, GameStatsPlatformTypeEnum platform, string category) : base(username, platform)
        {
            this.Category = category;
        }

        public override async Task Initialize()
        {
            this.categoryEnum = EnumHelper.GetEnumValueFromString<CallOfDutyBlackOps4GameStatsCategoryTypeEnum>(this.Category);

            string platformString = "";
            switch (this.Platform)
            {
                case GameStatsPlatformTypeEnum.PC: platformString = "battlenet"; break;
                case GameStatsPlatformTypeEnum.Xbox: platformString = "xbl"; break;
                case GameStatsPlatformTypeEnum.Playstation: platformString = "psn"; break;
            }

            this.user = await ChannelSession.Services.Scout.GetUser("codbo4", this.Username, new Dictionary<string, string>()
            {
                { "platform", platformString },
            });
        }

        public override async Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            if (this.user != null)
            {
                string segment = null;
                switch (this.categoryEnum)
                {
                    case CallOfDutyBlackOps4GameStatsCategoryTypeEnum.BlackoutLifetime: segment = "blackout.lifetime.all"; break;
                    case CallOfDutyBlackOps4GameStatsCategoryTypeEnum.BlackoutSoloLifetime: segment = "blackout.lifetime.solo"; break;
                    case CallOfDutyBlackOps4GameStatsCategoryTypeEnum.BlackoutDuoLifetime: segment = "blackout.lifetime.duo"; break;
                    case CallOfDutyBlackOps4GameStatsCategoryTypeEnum.BlackoutSquadLifetime: segment = "lackout.lifetime.quad"; break;
                }

                Dictionary<string, ScoutStat> stats = await ChannelSession.Services.Scout.GetStats("codbo4", this.user, segment);

                int wins = stats["wins"].ValueInt;
                int matches = stats["totalGamesPlayed"].ValueInt;
                int kills = stats["kills"].ValueInt;
                int deaths = stats["deaths"].ValueInt;
                double kd = (deaths > 0) ? (((double)kills) / ((double)deaths)) : 1.0;
                double wl = (matches > 0) ? ((((double)wins) / ((double)matches)) * 100.0) : 1.0;

                replacementSets["TOTAL_KILLS"] = kills.ToString();
                replacementSets["TOTAL_KD"] = Math.Round(kd, 2).ToString("F2");
                replacementSets["TOTAL_WINS"] = wins.ToString();
                replacementSets["TOTAL_WL"] = Math.Round(wl, 2).ToString("F1") + "%";

                if (this.initialWins == 0 && this.initialMatches == 0)
                {
                    this.initialKills = kills;
                    this.initialDeaths = deaths;
                    this.initialWins = wins;
                    this.initialMatches = matches;
                }

                int sessionKills = kills - this.initialKills;
                int sessionDeaths = deaths - this.initialDeaths;
                double sessionkd = (sessionDeaths > 0) ? (((double)sessionKills) / ((double)sessionDeaths)) : 1.0;
                int sessionWins = wins - this.initialWins;
                int sessionLoses = deaths - (this.initialMatches - this.initialWins);
                int sessionmatches = sessionWins + sessionLoses;
                double sessionwl = (sessionmatches > 0) ? ((((double)sessionWins) / ((double)sessionmatches)) * 100.0) : 1.0;

                replacementSets["SESSION_KILLS"] = sessionKills.ToString();
                replacementSets["SESSION_KD"] = Math.Round(sessionkd, 2).ToString("F2");
                replacementSets["SESSION_WINS"] = sessionWins.ToString();
                replacementSets["SESSION_WL"] = Math.Round(sessionwl, 2).ToString("F1") + "%";
            }

            return replacementSets;
        }
    }

    [DataContract]
    public class OverlayGameStats : OverlayCustomHTMLItem
    {
        public const string GameStatsItemType = "gamestats";

        [DataMember]
        public GameStatsSetupBase Setup { get; set; }

        [DataMember]
        public string BorderColor { get; set; }
        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string TextColor { get; set; }
        [DataMember]
        public string TextFont { get; set; }
        [DataMember]
        public int TextSize { get; set; }

        private DateTimeOffset lastUpdate = DateTimeOffset.MinValue;

        public OverlayGameStats() : base(GameStatsItemType, string.Empty) { }

        public OverlayGameStats(string htmlTemplate, GameStatsSetupBase setup, string borderColor, string backgroundColor, string textColor, string textFont, int textSize)
            : base(GameStatsItemType, htmlTemplate)
        {
            this.Setup = setup;
            this.BorderColor = borderColor;
            this.BackgroundColor = backgroundColor;
            this.TextColor = textColor;
            this.TextFont = textFont;
            this.TextSize = textSize;
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            await this.Setup.Initialize();
        }

        public override OverlayCustomHTMLItem GetCopy() { return this.Copy<OverlayGameStats>(); }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            if (this.lastUpdate.TotalMinutesFromNow() >= 1)
            {
                this.lastUpdate = DateTimeOffset.Now;
                return await base.GetProcessedItem(user, arguments, extraSpecialIdentifiers);
            }
            return null;
        }

        protected override async Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            Dictionary<string, string> replacementSets = await this.Setup.GetReplacementSets(user, arguments, extraSpecialIdentifiers);

            replacementSets["BACKGROUND_COLOR"] = this.BackgroundColor;
            replacementSets["BORDER_COLOR"] = this.BorderColor;
            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["TEXT_SIZE"] = this.TextSize.ToString();

            return replacementSets;
        }
    }
}

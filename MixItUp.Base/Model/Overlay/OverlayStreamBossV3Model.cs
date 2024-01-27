using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayStreamBossV3Model : OverlayEventTrackingV3ModelBase
    {
        public const string BossImageProperty = "BossImage";
        public const string BossNameProperty = "BossName";
        public const string BossHealthProperty = "BossHealth";
        public const string BossMaxHealthProperty = "BossMaxHealth";
        public const string BossHealthBarRemainingProperty = "BossHealthBarRemaining";

        public static readonly string DefaultHTML = OverlayResources.OverlayStreamBossDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayTextDefaultCSS + "\n\n" + OverlayResources.OverlayStreamBossDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayStreamBossDefaultJavascript;

        [DataMember]
        public Guid CurrentBoss { get; set; }
        [DataMember]
        public int CurrentHealth { get; set; }
        [DataMember]
        public int CurrentMaxHealth { get; set; }

        [DataMember]
        public string BorderColor { get; set; }
        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string HealthColor { get; set; }
        [DataMember]
        public string DamageColor { get; set; }

        [DataMember]
        public int BaseHealth { get; set; }
        [DataMember]
        public int KillBonusHealth { get; set; }
        [DataMember]
        public double OverkillBonusHealthMultiplier { get; set; }
        [DataMember]
        public double SelfHealingMultiplier { get; set; }

        [DataMember]
        public OverlayAnimationV3Model DamageAnimation { get; set; } = new OverlayAnimationV3Model();
        [DataMember]
        public OverlayAnimationV3Model HealingAnimation { get; set; } = new OverlayAnimationV3Model();
        [DataMember]
        public OverlayAnimationV3Model NewBossAnimation { get; set; } = new OverlayAnimationV3Model();

        [DataMember]
        public Guid DamageOccurredCommandID { get; set; }
        [DataMember]
        public Guid HealingOccurredCommandID { get; set; }
        [DataMember]
        public Guid NewBossCommandID { get; set; }

        [JsonIgnore]
        public int HealthRemainingPercentage { get { return Math.Max(Math.Min((int)Math.Round(((double)this.CurrentHealth / this.CurrentMaxHealth) * 100.0), 100), 0); } }

        [JsonIgnore]
        public override bool IsTestable { get { return true; } }
        [JsonIgnore]
        public override bool IsResettable { get { return true; } }

        public OverlayStreamBossV3Model() : base(OverlayItemV3Type.StreamBoss) { }

        public async Task ProcessEvent(UserV2ViewModel user, double amount, bool forceDamage = false)
        {
            if (amount > 0)
            {
                int damage = (int)Math.Round(amount);

                if (!forceDamage && this.CurrentBoss == user.ID && this.SelfHealingMultiplier > 0)
                {
                    this.CurrentHealth = Math.Min(damage + this.CurrentHealth, this.CurrentMaxHealth);
                    await this.Heal();
                    await ServiceManager.Get<CommandService>().Queue(this.HealingOccurredCommandID, new CommandParametersModel(user));
                }
                else
                {
                    this.CurrentHealth -= damage;
                    if (this.CurrentHealth > 0)
                    {
                        await this.Damage();
                        await ServiceManager.Get<CommandService>().Queue(this.DamageOccurredCommandID, new CommandParametersModel(user));
                    }
                    else
                    {
                        this.CurrentBoss = user.ID;
                        this.CurrentMaxHealth += this.KillBonusHealth;
                        this.CurrentMaxHealth += (int)Math.Round(Math.Abs(this.CurrentHealth) * this.OverkillBonusHealthMultiplier);
                        this.CurrentHealth = this.CurrentMaxHealth;

                        await this.NewBoss(user);
                        await ServiceManager.Get<CommandService>().Queue(this.NewBossCommandID, new CommandParametersModel(user));
                    }
                }
            }
        }

        public async Task Damage()
        {
            await this.CallFunction("damage", this.GetDataProperties());
        }

        public async Task Heal()
        {
            await this.CallFunction("heal", this.GetDataProperties());
        }

        public async Task NewBoss(UserV2ViewModel user)
        {
            Dictionary<string, object> properties = this.GetDataProperties();
            properties[BossImageProperty] = user.AvatarLink;
            properties[BossNameProperty] = user.DisplayName;
            properties[BossMaxHealthProperty] = this.CurrentMaxHealth.ToString();
            await this.CallFunction("killboss", properties);
        }

        public override async Task ProcessEvent(UserV2ViewModel user, double amount)
        {
            await this.ProcessEvent(user, amount, forceDamage: false);
        }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();

            foreach (var kvp in this.GetDataProperties())
            {
                properties[kvp.Key] = kvp.Value;
            }

            properties[nameof(this.BorderColor)] = this.BorderColor;
            properties[nameof(this.BackgroundColor)] = this.BackgroundColor;
            properties[nameof(this.HealthColor)] = this.HealthColor;
            properties[nameof(this.DamageColor)] = this.DamageColor;

            properties[nameof(this.DamageAnimation)] = this.DamageAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElementID);
            properties[nameof(this.HealingAnimation)] = this.HealingAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElementID);
            properties[nameof(this.NewBossAnimation)] = this.NewBossAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElementID);

            return properties;
        }

        public override async Task ProcessGenerationProperties(Dictionary<string, object> properties, CommandParametersModel parameters)
        {
            await base.ProcessGenerationProperties(properties, parameters);

            UserV2ViewModel boss = await ServiceManager.Get<UserService>().GetUserByID(this.CurrentBoss);
            if (boss == null)
            {
                boss = ChannelSession.User;
            }

            properties[BossImageProperty] = boss.AvatarLink;
            properties[BossNameProperty] = boss.DisplayName;
        }

        protected override async Task WidgetEnableInternal()
        {
            await base.WidgetEnableInternal();

            if (this.CurrentBoss == Guid.Empty)
            {
                this.CurrentBoss = ChannelSession.User.ID;
                this.CurrentHealth = this.CurrentMaxHealth = this.BaseHealth;
            }
        }

        private Dictionary<string, object> GetDataProperties()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data[BossHealthProperty] = this.CurrentHealth;
            data[BossMaxHealthProperty] = this.CurrentMaxHealth;
            data[BossHealthBarRemainingProperty] = this.HealthRemainingPercentage;
            return data;
        }
    }
}

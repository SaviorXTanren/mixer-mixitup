namespace MixItUp.Base.Services.Trovo
{
    public interface ITrovoPlatformService
    { 
    }

    public class TrovoPlatformService : StreamingPlatformServiceBase, ITrovoPlatformService
    {
        public const string clientID = "8FMjuk785AX4FMyrwPTU3B8vYvgHWN33";

        public override string Name { get { return "Trovo Connection"; } }
    }
}

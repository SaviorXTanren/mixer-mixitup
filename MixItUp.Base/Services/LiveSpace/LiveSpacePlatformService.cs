namespace MixItUp.Base.Services.LiveSpace
{
    /// <summary>
    /// https://apidocs.live.space
    /// </summary>
    public class LiveSpacePlatformService
    {
        public LiveSpacePlatformService()
        {
            ServiceManager.Get<SecretsService>().GetSecret("LiveSpaceAPIKey");
        }
    }
}

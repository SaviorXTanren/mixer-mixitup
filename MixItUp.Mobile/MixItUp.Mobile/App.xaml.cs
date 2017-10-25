using MixItUp.Base.Util;
using MixItUp.Mobile.Services;

using Xamarin.Forms;

namespace MixItUp.Mobile
{
    public partial class App : Application
	{
		public App ()
		{
			InitializeComponent();

            Logger.Initialize(new AndroidFileService());
            SerializerHelper.Initialize(new AndroidFileService());

            ChannelSession.Initialize(new MobileServicesHandler());

			MainPage = new MixItUp.Mobile.MainPage();
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}

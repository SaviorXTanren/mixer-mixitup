using MixItUp.Base.ViewModel.Services;
using MixItUp.Base;

public class LoupeDeckServiceControlViewModel : ServiceControlViewModelBase
{
    public override string WikiPageName { get { return "loupe-deck"; } }

    public LoupeDeckServiceControlViewModel() : base(Resources.LoupeDeck) { }
}
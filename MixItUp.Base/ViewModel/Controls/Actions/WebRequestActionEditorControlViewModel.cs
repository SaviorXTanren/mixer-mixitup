using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Actions
{
    public class WebRequestActionJSONToSpecialIdentifierViewModel : UIViewModelBase
    {
        public string JSONParameterName { get; set; }
        public string SpecialIdentifierName { get; set; }

        public ICommand DeleteJSONParameterCommand { get; private set; }

        private WebRequestActionEditorControlViewModel viewModel;

        public WebRequestActionJSONToSpecialIdentifierViewModel(string jsonParameterName, string specialIdentifierName, WebRequestActionEditorControlViewModel viewModel)
            : this(viewModel)
        {
            this.JSONParameterName = jsonParameterName;
            this.SpecialIdentifierName = specialIdentifierName;
        }

        public WebRequestActionJSONToSpecialIdentifierViewModel(WebRequestActionEditorControlViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.DeleteJSONParameterCommand = this.CreateCommand((parameter) =>
            {
                this.viewModel.JSONParameters.Remove(this);
                return Task.FromResult(0);
            });
        }
    }

    public class WebRequestActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.WebRequest; } }

        public string RequestURL
        {
            get { return this.requestURL; }
            set
            {
                this.requestURL = value;
                this.NotifyPropertyChanged();
            }
        }
        private string requestURL;

        public IEnumerable<WebRequestResponseParseTypeEnum> ResponseParseTypes { get { return EnumHelper.GetEnumList<WebRequestResponseParseTypeEnum>(); } }

        public WebRequestResponseParseTypeEnum SelectedResponseParseType
        {
            get { return this.selectedResponseParseType; }
            set
            {
                this.selectedResponseParseType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowJSONGrid");
            }
        }
        public WebRequestResponseParseTypeEnum selectedResponseParseType;

        public bool ShowJSONGrid { get { return this.SelectedResponseParseType == WebRequestResponseParseTypeEnum.JSONToSpecialIdentifiers; } }

        public ICommand AddJSONParameterCommand { get; private set; }

        public ObservableCollection<WebRequestActionJSONToSpecialIdentifierViewModel> JSONParameters { get; set; } = new ObservableCollection<WebRequestActionJSONToSpecialIdentifierViewModel>();

        public WebRequestActionEditorControlViewModel(WebRequestActionModel action)
            : this()
        {
            this.RequestURL = action.Url;
            this.SelectedResponseParseType = action.ResponseType;
            if (this.ShowJSONGrid)
            {
                foreach (var kvp in action.JSONToSpecialIdentifiers)
                {
                    this.JSONParameters.Add(new WebRequestActionJSONToSpecialIdentifierViewModel(kvp.Key, kvp.Value, this));
                }
            }
        }

        public WebRequestActionEditorControlViewModel()
        {
            this.AddJSONParameterCommand = this.CreateCommand((parameter) =>
            {
                this.JSONParameters.Add(new WebRequestActionJSONToSpecialIdentifierViewModel(this));
                return Task.FromResult(0);
            });
        }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrEmpty(this.RequestURL))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.WebRequestActionMissingURL));
            }

            if (this.ShowJSONGrid)
            {
                foreach (WebRequestActionJSONToSpecialIdentifierViewModel jsonParameter in this.JSONParameters)
                {
                    if (string.IsNullOrEmpty(jsonParameter.JSONParameterName) || string.IsNullOrEmpty(jsonParameter.SpecialIdentifierName))
                    {
                        return Task.FromResult(new Result(MixItUp.Base.Resources.WebRequestActionMissingJSONParameter));
                    }

                    jsonParameter.SpecialIdentifierName = jsonParameter.SpecialIdentifierName.Replace("$", "");
                    if (!SpecialIdentifierStringBuilder.IsValidSpecialIdentifier(jsonParameter.SpecialIdentifierName))
                    {
                        return Task.FromResult(new Result(MixItUp.Base.Resources.WebRequestActionInvalidJSONSpecialIdentifier));
                    }
                }
            }

            return Task.FromResult(new Result());
        }

        public override Task<ActionModelBase> GetAction()
        {
            if (this.ShowJSONGrid)
            {
                return Task.FromResult<ActionModelBase>(new WebRequestActionModel(this.RequestURL, this.JSONParameters.ToDictionary(j => j.JSONParameterName, j => j.SpecialIdentifierName)));
            }
            else
            {
                return Task.FromResult<ActionModelBase>(new WebRequestActionModel(this.RequestURL, this.SelectedResponseParseType));
            }
        }
    }
}

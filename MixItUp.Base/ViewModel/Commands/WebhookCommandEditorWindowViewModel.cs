using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Webhooks;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Commands
{
    public class WebhookJSONParameterViewModel : UIViewModelBase
    {
        public string JSONParameterName { get; set; }
        public string SpecialIdentifierName { get; set; }
        public ICommand DeleteJSONParameterCommand { get; set; }

        private WebhookCommandEditorWindowViewModel viewModel;

        public WebhookJSONParameterViewModel(WebhookCommandEditorWindowViewModel viewModel)
        {
            this.viewModel = viewModel;

            this.DeleteJSONParameterCommand = this.CreateCommand(() =>
            {
                this.viewModel.JSONParameters.Remove(this);
            });
        }
    }

    public class WebhookCommandEditorWindowViewModel : CommandEditorWindowViewModelBase
    {
        private Guid webhookID;

        // Prevent uploading
        public override bool IsExistingCommand => false;

        public ICommand AddJSONParameterCommand { get; private set; }
        public ObservableCollection<WebhookJSONParameterViewModel> JSONParameters { get; set; } = new ObservableCollection<WebhookJSONParameterViewModel>();

        public WebhookCommandEditorWindowViewModel(WebhookCommandModel existingCommand) : base(existingCommand)
        {
            this.webhookID = existingCommand.ID;
            JSONParameters.Clear();
            foreach (var param in existingCommand.JSONParameters)
            {
                JSONParameters.Add(new WebhookJSONParameterViewModel(this) { JSONParameterName = param.JSONParameterName, SpecialIdentifierName = param.SpecialIdentifierName });
            }
        }

        public WebhookCommandEditorWindowViewModel(Webhook webhook) : base(CommandTypeEnum.Webhook)
        {
            this.webhookID = webhook.Id;
        }

        public override Dictionary<string, string> GetTestSpecialIdentifiers()
        {
            return JSONParameters.ToDictionary(j => j.SpecialIdentifierName, j => "Test Value");
        }

        protected override async Task OnOpenInternal()
        {
            this.AddJSONParameterCommand = this.CreateCommand(() =>
            {
                this.JSONParameters.Add(new WebhookJSONParameterViewModel(this));
            });
            await base.OnOpenInternal();
        }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ACommandNameMustBeSpecified));
            }

            if (this.JSONParameters.Any(j => string.IsNullOrWhiteSpace(j.JSONParameterName)))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.AJSONParameterNameMustBeSpecified));
            }

            if (this.JSONParameters.Any(j => string.IsNullOrWhiteSpace(j.SpecialIdentifierName)))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ASpecialIdentifierNameMustBeSpecified));
            }

            return Task.FromResult(new Result());
        }

        public override Task<CommandModelBase> CreateNewCommand()
        {
            var command = new WebhookCommandModel(this.Name);

            // Link the cloud webhook with the local command ID
            command.ID = this.webhookID;

            return Task.FromResult<CommandModelBase>(command);
        }

        public override async Task UpdateExistingCommand(CommandModelBase command)
        {
            await base.UpdateExistingCommand(command);
        }

        public override Task SaveCommandToSettings(CommandModelBase command)
        {
            // Save JSON Params here
            var webhookCommand = (WebhookCommandModel)command;

            webhookCommand.JSONParameters.Clear();
            foreach (var param in this.JSONParameters)
            {
                webhookCommand.JSONParameters.Add(new WebhookJSONParameter { JSONParameterName = param.JSONParameterName, SpecialIdentifierName = param.SpecialIdentifierName });
            }

            ServiceManager.Get<CommandService>().WebhookCommands.Remove((WebhookCommandModel)this.existingCommand);
            ServiceManager.Get<CommandService>().WebhookCommands.Add(webhookCommand);
            return Task.FromResult(0);
        }
    }
}

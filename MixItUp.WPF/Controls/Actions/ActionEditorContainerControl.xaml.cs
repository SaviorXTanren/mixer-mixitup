using MixItUp.Base.Model.Actions;
using MixItUp.Base.ViewModel.Controls.Actions;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for ActionEditorContainerControl.xaml
    /// </summary>
    public partial class ActionEditorContainerControl : LoadingControlBase
    {
        public ActionEditorControlViewModelBase ViewModel { get; private set; }

        public ActionEditorControlBase Control { get; private set; }

        public ActionEditorContainerControl()
        {
            InitializeComponent();

            this.DataContextChanged += ActionEditorContainerControl_DataContextChanged;
        }

        protected override async Task OnLoaded()
        {
            this.ActionContentControl.Content = this.Control;
            await base.OnLoaded();
        }

        private void ActionEditorContainerControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is ActionEditorControlViewModelBase)
            {
                this.ViewModel = (ActionEditorControlViewModelBase)this.DataContext;
                switch (this.ViewModel.Type)
                {
                    case ActionTypeEnum.Chat: this.Control = new ChatActionEditorControl(); break;
                    case ActionTypeEnum.Command: this.Control = new CommandActionEditorControl(); break;
                }

                if (this.IsLoaded)
                {
                    this.ActionContentControl.Content = this.Control;
                }
            }
        }
    }
}

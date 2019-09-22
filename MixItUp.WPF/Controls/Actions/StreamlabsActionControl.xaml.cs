using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Services;
using StreamingClient.Base.Util;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for StreamlabsActionControl.xaml
    /// </summary>
    public partial class StreamlabsActionControl : ActionControlBase
    {
        private StreamlabsAction action;

        public StreamlabsActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public StreamlabsActionControl(ActionContainerControl containerControl, StreamlabsAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            if (ChannelSession.Services.Streamlabs == null)
            {
                this.StreamlabsNotEnabledWarningTextBlock.Visibility = System.Windows.Visibility.Visible;
            }

            this.StreamlabsActionTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<StreamlabsActionTypeEnum>();
            if (this.action != null)
            {
                this.StreamlabsActionTypeComboBox.SelectedItem = EnumHelper.GetEnumName(action.StreamlabType);
            }

            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.StreamlabsActionTypeComboBox.SelectedIndex >= 0)
            {
                StreamlabsActionTypeEnum actionType = EnumHelper.GetEnumValueFromString<StreamlabsActionTypeEnum>((string)this.StreamlabsActionTypeComboBox.SelectedItem);
                switch (actionType)
                {
                    case StreamlabsActionTypeEnum.SpinWheel:
                        return StreamlabsAction.CreateForSpinWheel();
                    case StreamlabsActionTypeEnum.EmptyJar:
                        return StreamlabsAction.CreateForEmptyJar();
                    case StreamlabsActionTypeEnum.RollCredits:
                        return StreamlabsAction.CreateForRollCredits();
                }
            }
            return null;
        }
    }
}

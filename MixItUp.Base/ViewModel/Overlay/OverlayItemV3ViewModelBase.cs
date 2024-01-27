using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Overlay
{
    public abstract class OverlayItemV3ViewModelBase : UIViewModelBase
    {
        public OverlayItemV3Type Type
        {
            get { return this.type; }
            set
            {
                this.type = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayItemV3Type type;

        public abstract string DefaultHTML { get; }
        public abstract string DefaultCSS { get; }
        public abstract string DefaultJavascript { get; }

        public ObservableCollection<OverlayAnimationV3ViewModel> Animations { get; set; } = new ObservableCollection<OverlayAnimationV3ViewModel>();

        public virtual bool IsTestable { get { return false; } }

        public OverlayItemV3ViewModelBase(OverlayItemV3Type type)
        {
            this.Type = type;
        }

        public OverlayItemV3ViewModelBase(OverlayItemV3ModelBase item)
        {
            this.Type = item.Type;
        }

        public virtual Result Validate()
        {
            return new Result();
        }

        public OverlayItemV3ModelBase GetItem() { return this.GetItemInternal(); }

        public virtual Task TestWidget(OverlayWidgetV3Model widget) { return Task.CompletedTask; }

        protected abstract OverlayItemV3ModelBase GetItemInternal();

        protected CustomCommandModel CreateEmbeddedCommand(string name)
        {
            return new CustomCommandModel(name)
            {
                IsEmbedded = true
            };
        }

        protected CustomCommandModel GetEmbeddedCommand(Guid id, string name)
        {
            if (ChannelSession.Settings.Commands.TryGetValue(id, out CommandModelBase command))
            {
                return (CustomCommandModel)command;
            }
            return this.CreateEmbeddedCommand(name);
        }
    }
}

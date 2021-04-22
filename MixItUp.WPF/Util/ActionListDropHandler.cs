using GongSolutions.Wpf.DragDrop;
using MixItUp.Base.ViewModel.Actions;

namespace MixItUp.WPF.Util
{
    public class ActionListDropHandler : IDropTarget
    {
        public static ActionListDropHandler Instance { get; } = new ActionListDropHandler();

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is ActionEditorControlViewModelBase)
            {
                DragDrop.DefaultDropHandler.DragOver(dropInfo);
            }
            else
            {
                dropInfo.Effects = System.Windows.DragDropEffects.None;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            DragDrop.DefaultDropHandler.Drop(dropInfo);
        }
    }
}

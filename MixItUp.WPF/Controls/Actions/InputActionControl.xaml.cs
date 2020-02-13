using Mixer.Base.Util;
using MixItUp.Base.Actions;
using MixItUp.Base.Services;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for InputActionControl.xaml
    /// </summary>
    public partial class InputActionControl : ActionControlBase
    {
        private InputAction action;

        public InputActionControl() : base() { InitializeComponent(); }

        public InputActionControl(InputAction action) : this() { this.action = action; }

        public override Task OnLoaded()
        {
            this.InputTypeComboBox.ItemsSource = new List<string>() { MixItUp.Base.Resources.Keyboard, MixItUp.Base.Resources.Mouse };
            this.KeyboardKeyComboBox.ItemsSource = EnumHelper.GetEnumNames<InputKeyEnum>().OrderBy(s => s);
            this.MouseButtonComboBox.ItemsSource = Enum.GetValues(typeof(SimpleInputMouseEnum));
            this.KeyButtonActionComboBox.ItemsSource = Enum.GetValues(typeof(InputActionTypeEnum));

            if (this.action != null)
            {
                if (this.action.Key != null)
                {
                    this.InputTypeComboBox.SelectedIndex = 0;
                    this.KeyboardKeyComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.Key.GetValueOrDefault());
                }
                else if (this.action.Mouse != null)
                {
                    this.InputTypeComboBox.SelectedIndex = 1;
                    this.MouseButtonComboBox.SelectedItem = this.action.Mouse.GetValueOrDefault();
                }
                this.KeyButtonActionComboBox.SelectedItem = this.action.ActionType;
                this.ShiftCheckBox.IsChecked = this.action.Shift;
                this.ControlCheckBox.IsChecked = this.action.Control;
                this.AltCheckBox.IsChecked = this.action.Alt;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.KeyButtonActionComboBox.SelectedIndex >= 0)
            {
                InputActionTypeEnum actionType = (InputActionTypeEnum)this.KeyButtonActionComboBox.SelectedItem;
                if (this.InputTypeComboBox.SelectedIndex == 0 && this.KeyboardKeyComboBox.SelectedIndex >= 0)
                {
                    return new InputAction(EnumHelper.GetEnumValueFromString<InputKeyEnum>((string)this.KeyboardKeyComboBox.SelectedItem), actionType,
                        this.ShiftCheckBox.IsChecked.GetValueOrDefault(), this.ControlCheckBox.IsChecked.GetValueOrDefault(), this.AltCheckBox.IsChecked.GetValueOrDefault());
                }
                else if (this.InputTypeComboBox.SelectedIndex == 1 && this.MouseButtonComboBox.SelectedIndex >= 0)
                {
                    return new InputAction((SimpleInputMouseEnum)this.MouseButtonComboBox.SelectedItem, actionType,
                        this.ShiftCheckBox.IsChecked.GetValueOrDefault(), this.ControlCheckBox.IsChecked.GetValueOrDefault(), this.AltCheckBox.IsChecked.GetValueOrDefault());
                }
            }
            return null;
        }

        private void SimpleInputTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.KeyboardKeyComboBox.Visibility = (this.InputTypeComboBox.SelectedIndex == 0) ? Visibility.Visible : Visibility.Collapsed;
            this.MouseButtonComboBox.Visibility = (this.InputTypeComboBox.SelectedIndex == 1) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}

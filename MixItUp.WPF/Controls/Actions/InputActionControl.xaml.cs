using Mixer.Base.Util;
using MixItUp.Base.Actions;
using MixItUp.Base.Services;
using System.Collections.Generic;
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

        private SimpleInputAction simpleAction;

        public InputActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public InputActionControl(ActionContainerControl containerControl, InputAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.SimpleInputTypeComboBox.ItemsSource = new List<string>() { "Keyboard", "Mouse" };
            this.SimpleKeyboardKeyComboBox.ItemsSource = EnumHelper.GetEnumNames<InputKeyEnum>();
            this.SimpleMouseButtonComboBox.ItemsSource = EnumHelper.GetEnumNames<SimpleInputMouseEnum>();

            this.SimpleInputTypeComboBox.SelectedIndex = 0;

            if (this.action != null)
            {
                if (this.action is SimpleInputAction)
                {
                    this.simpleAction = (SimpleInputAction)this.action;
                    if (this.simpleAction.Key != null)
                    {
                        this.SimpleInputTypeComboBox.SelectedIndex = 0;
                        this.SimpleKeyboardKeyComboBox.SelectedItem = EnumHelper.GetEnumName(this.simpleAction.Key.GetValueOrDefault());
                    }
                    else if (this.simpleAction.Mouse != null)
                    {
                        this.SimpleInputTypeComboBox.SelectedIndex = 1;
                        this.SimpleMouseButtonComboBox.SelectedItem = EnumHelper.GetEnumName(this.simpleAction.Mouse.GetValueOrDefault());
                    }
                    this.SimpleShiftCheckBox.IsChecked = this.simpleAction.Shift;
                    this.SimpleControlCheckBox.IsChecked = this.simpleAction.Control;
                    this.SimpleAltCheckBox.IsChecked = this.simpleAction.Alt;
                }
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.InputModeToggleButton.IsChecked.GetValueOrDefault())
            {

            }
            else
            {
                if (this.SimpleInputTypeComboBox.SelectedIndex == 0 && this.SimpleKeyboardKeyComboBox.SelectedIndex >= 0)
                {
                    return new SimpleInputAction(EnumHelper.GetEnumValueFromString<InputKeyEnum>((string)this.SimpleKeyboardKeyComboBox.SelectedItem),
                        this.SimpleShiftCheckBox.IsChecked.GetValueOrDefault(), this.SimpleControlCheckBox.IsChecked.GetValueOrDefault(), this.SimpleAltCheckBox.IsChecked.GetValueOrDefault());
                }
                else if (this.SimpleInputTypeComboBox.SelectedIndex == 1 && this.SimpleMouseButtonComboBox.SelectedIndex >= 0)
                {
                    return new SimpleInputAction(EnumHelper.GetEnumValueFromString<SimpleInputMouseEnum>((string)this.SimpleMouseButtonComboBox.SelectedItem),
                        this.SimpleShiftCheckBox.IsChecked.GetValueOrDefault(), this.SimpleControlCheckBox.IsChecked.GetValueOrDefault(), this.SimpleAltCheckBox.IsChecked.GetValueOrDefault());
                }
            }
            return null;
        }

        private void InputModeToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            this.SimpleModeGrid.Visibility = (this.InputModeToggleButton.IsChecked.GetValueOrDefault()) ? Visibility.Collapsed : Visibility.Visible;
            this.AdvancedModeGrid.Visibility = (this.InputModeToggleButton.IsChecked.GetValueOrDefault()) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SimpleInputTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.SimpleKeyboardKeyComboBox.Visibility = (this.SimpleInputTypeComboBox.SelectedIndex == 0) ? Visibility.Visible : Visibility.Collapsed;
            this.SimpleMouseButtonComboBox.Visibility = (this.SimpleInputTypeComboBox.SelectedIndex == 1) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}

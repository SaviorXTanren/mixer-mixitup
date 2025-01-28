using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MixItUp.WPF.Controls
{
    /// <summary>
    /// Interaction logic for OnOffToggleDropDownControl.xaml
    /// </summary>
    public partial class OnOffToggleDropDownControl : UserControl
    {
        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register(
                "DisplayName",
                typeof(string),
                typeof(OnOffToggleDropDownControl),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
                )
                {
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(bool?),
                typeof(OnOffToggleDropDownControl),
                new FrameworkPropertyMetadata(
                    true,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnValueChanged)
                )
                {
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        public string DisplayName
        {
            get { return (string)GetValue(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }

        public bool? Value
        {
            get { return (bool?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private bool? InternalValue
        {
            get
            {
                if (this.DropDown.SelectedIndex == 0)
                {
                    return true;
                }
                else if (this.DropDown.SelectedIndex == 1)
                {
                    return false;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value == true)
                {
                    this.DropDown.SelectedIndex = 0;
                }
                else if (value == false)
                {
                    this.DropDown.SelectedIndex = 1;
                }
                else
                {
                    this.DropDown.SelectedIndex = 2;
                }
            }
        }

        public List<string> Options { get; set; } = new List<string>()
        {
            MixItUp.Base.Resources.On,
            MixItUp.Base.Resources.Off,
            MixItUp.Base.Resources.Toggle
        };

        public OnOffToggleDropDownControl()
        {
            InitializeComponent();

            this.Display.DataContext = this;
            this.DropDown.DataContext = this;

            this.Loaded += OnOffToggleDropDownControl_Loaded;
        }

        private void OnOffToggleDropDownControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.InternalValue = this.Value;
            this.DropDown.SelectionChanged += OnOffToggleDropDownControl_SelectionChanged;
        }

        private void OnOffToggleDropDownControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Value != this.InternalValue)
            {
                this.Value = this.InternalValue;
            }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OnOffToggleDropDownControl dropDown = (OnOffToggleDropDownControl)d;
            if (dropDown != null && dropDown.Value != dropDown.InternalValue)
            {
                dropDown.InternalValue = dropDown.Value;
            }
        }
    }
}

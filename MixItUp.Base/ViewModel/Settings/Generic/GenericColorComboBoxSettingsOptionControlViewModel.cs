using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.Settings.Generic
{
    public class ColorOptionViewModel : IEquatable<ColorOptionViewModel>
    {
        public string Name { get; set; }
        public string ColorCode { get; set; }

        public bool HasColor { get { return !string.IsNullOrEmpty(this.ColorCode); } }

        public ColorOptionViewModel() { }

        public ColorOptionViewModel(string name) : this(name, string.Empty) { }

        public ColorOptionViewModel(string name, string colorCode)
        {
            this.Name = name;
            this.ColorCode = colorCode;
        }

        public override bool Equals(object obj)
        {
            if (obj is ColorOptionViewModel)
            {
                return this.Equals((ColorOptionViewModel)obj);
            }
            return false;
        }

        public bool Equals(ColorOptionViewModel other) { return this.Name.Equals(other.Name); }

        public override int GetHashCode() { return this.Name.GetHashCode(); }
    }

    public class GenericColorComboBoxSettingsOptionControlViewModel : GenericComboBoxSettingsOptionControlViewModel<ColorOptionViewModel>
    {
        public const string NoneOption = "None";

        private static IEnumerable<ColorOptionViewModel> AvailableColors = ColorSchemes.MaterialDesignColors.Select(c => new ColorOptionViewModel(c.Key, c.Value));

        public GenericColorComboBoxSettingsOptionControlViewModel(string name, string initialValue, Action<string> valueSetter, string tooltip = null)
            : base(name, AvailableColors, null, (value) => { valueSetter(value?.Name); }, tooltip)
        {
            this.Value = this.Values.FirstOrDefault(c => c.Name.Equals(initialValue));
        }

        public void RemoveNonThemes()
        {
            this.Values.Remove(new ColorOptionViewModel("Black"));
            this.Values.Remove(new ColorOptionViewModel("White"));
            this.Values.Remove(new ColorOptionViewModel("Transparent"));
        }

        public void AddNoneOption()
        {
            this.Values.Insert(0, new ColorOptionViewModel(NoneOption));
            if (this.Value == null)
            {
                this.Value = this.Values[0];
            }
        }
    }
}

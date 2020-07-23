using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.Controls.Settings.Generic
{
    public class ColorOptionViewModel : IEquatable<ColorOptionViewModel>
    {
        public string Name { get; set; }
        public string ColorCode { get; set; }

        public ColorOptionViewModel() { }

        public ColorOptionViewModel(string name)
        {
            this.Name = name;
        }

        public ColorOptionViewModel(string name, string colorCode)
            : this(name)
        {
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
        private static IEnumerable<ColorOptionViewModel> AvailableColors = ColorSchemes.HTMLColorSchemeDictionary.Select(c => new ColorOptionViewModel(c.Key, c.Value));

        public GenericColorComboBoxSettingsOptionControlViewModel(string name, string initialValue, Action<ColorOptionViewModel> valueSetter, string tooltip = null)
            : base(name, AvailableColors, null, valueSetter, tooltip)
        {
            this.Value = this.Values.FirstOrDefault(c => c.Name.Equals(initialValue));
        }

        public void RemoveNonThemes()
        {
            this.Values.Remove(new ColorOptionViewModel("Black"));
            this.Values.Remove(new ColorOptionViewModel("White"));
        }
    }
}

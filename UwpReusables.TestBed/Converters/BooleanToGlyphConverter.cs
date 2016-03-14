using System;
using Windows.UI.Xaml.Data;

namespace UwpReusables.TestBed.Converters
{
    public class BooleanToGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool input = (bool)value;
            return input ? "✓" : "X";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ClipFFmpeg
{
    class BooleanToInvisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
            {
                Console.Write("BooleanToInvisibilityConverter value not bool");
                return Visibility.Visible;
            }

            bool val = (bool)value;
            return ((bool)val) ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Visibility))
            {
                Console.Write("BooleanToInvisibilityConverter value not Visibility");
                return Visibility.Visible;
            }

            return ((Visibility)value) == Visibility.Hidden; 
        }
    }
}

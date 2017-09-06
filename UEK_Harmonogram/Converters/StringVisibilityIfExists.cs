using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace UEK_Harmonogram.Converters
{
    class StringVisibilityIfExists : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || value.ToString().Length == 0 || value.ToString().Contains("Sala"))
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace UEK_Harmonogram.Converters
{
    public class ImprovedNameOfTermin : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var wartosc = (string)value;
            if (wartosc != null)
            {
                DateTime dt;
                if (DateTime.TryParseExact(wartosc, "yyyy-MM-dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out dt))
                    return dt.ToString("dd MMMM, dddd", CultureInfo.CurrentCulture);
                else
                    return wartosc;
            }
            return null;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace UEK_Harmonogram.Converters
{
    class ChangeIfEmpty : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            Lekcja lekcja = (Lekcja)value;

            if (parameter != null && parameter is string)
            {
                switch ((string)parameter)
                {
                    case "SetTyp":
                        if (lekcja.Przedmiot == null || lekcja.Przedmiot.ToString().Length == 0)
                        {
                            if (lekcja.Typ == null)
                                return null;
                            else
                                return lekcja.Typ.ToUpper();
                        }
                        return lekcja.Przedmiot;

                    case "UwagaToSala":
                        if (lekcja.Sala == null || lekcja.Sala.ToString().Length == 0)
                        {
                            if (lekcja.Uwaga != null)
                            {
                                if (lekcja.Uwaga.Contains("Sala") == true)
                                {
                                    return lekcja.Uwaga;
                                }
                                return null;
                            }
                            return null;
                        }
                        return lekcja.Sala;

                    default: return null;
                }
            }
            return null;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

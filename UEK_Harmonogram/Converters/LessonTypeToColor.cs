using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace UEK_Harmonogram.Converters
{
    class LessonTypeToColor : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            switch (value as string)
            {
                case "ćwiczenia": return "#4DBD02";             // zielony
                case "wykład": return "#6077E0";                // niebieski
                case "wykład do wyboru": return "#37C6C7";      // błękitny
                case "egzamin": return "#ED094A";               // czerwony
                case "lektorat": return "#FF8B17";              // pomarańczowy
                case "rezerwacja": return "#DBBC0B";            // żółty
                case "seminarium": return "#890DDB";            // fioletowy
                case "Przeniesienie zajęć": return "#bbbbbb";   // jasnoszary
                default: return "#555555";                      // ciemnoszary
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

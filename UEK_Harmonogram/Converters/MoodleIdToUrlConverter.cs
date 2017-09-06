using System;
using Windows.UI.Xaml.Data;

namespace UEK_Harmonogram.Converters
{
    class MoodleIdToUrlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var moodleId = value as string;
            if (moodleId != null && moodleId != string.Empty)
                return new Uri($"https://e-uczelnia.uek.krakow.pl/course/view.php?id={moodleId}");
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

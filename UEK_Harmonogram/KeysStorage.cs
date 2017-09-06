using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace UEK_Harmonogram
{
    static class KeysStorage
    {
        private static ApplicationDataContainer container = ApplicationData.Current.LocalSettings;

        public static T GetKey<T>(string key, T defaultValue)
        {
            container.Values.TryGetValue(key, out object output);
            if (output == null)
                return defaultValue;
            return (T)output;
        }

        public static void PutKey(string key, object value)
        {
            if (container.Values.ContainsKey(key))
                container.Values[key] = value;
            else
                container.Values.Add(key, value);
        }

        public static void RemoveKey(string key)
        {
            if (container.Values[key] != null)
                container.Values.Remove(key);
        }

        public static void RemoveKeys()
        {
            container.Values.Clear();
        }

        public static async void ShowMessageWithKeysList()
        {
            var keys = ApplicationData.Current.LocalSettings.Values;

            string text = "";

            foreach (var key in keys)
                text = text + key.Key.ToString() + " - " + key.Value.ToString() + "\n";
            var msg = new Windows.UI.Popups.MessageDialog(text);
            await msg.ShowAsync();
            System.Diagnostics.Debug.WriteLine(text);
        }
    }
}

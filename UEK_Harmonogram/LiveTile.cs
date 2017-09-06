using System.Collections.Generic;
using System.Linq;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace UEK_Harmonogram
{
    class LiveTile
    {
        bool isMobile;

        public LiveTile()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                this.isMobile = true;
            else
                this.isMobile = false;
        }

        private string CreateTileAsString(Lekcja lekcja, int all, int counter)
        {
            var termin = new Converters.ImprovedNameOfTermin().Convert(lekcja.Termin, null, null, null);
            string content = $@"<tile>
                                    <visual>
                                        <binding template='TileWide' branding='{(isMobile ? "none" : "name")}'>
                                            <text hint-style='base'> {counter}/{all} | {termin}</text>
                                            <text hint-style='caption'>{lekcja.Start}-{lekcja.Stop} @ {lekcja.Sala}</text>
                                            <text hint-style='captionSubtle'>({lekcja.Typ[0]}) {lekcja.Przedmiot}</text>
                                            <text hint-style='captionSubtle'>{lekcja.ZKim}</text>
                                        </binding>
                                    </visual>
                                </tile>";
            return content;
        }

        public void UpdateTiles(List<Lekcja> lessons)
        {
            if (lessons != null && lessons.Count > 0)
            {
                var firstTermin = lessons.First().Termin;
                var todaysLessons = lessons.TakeWhile(item => item.Termin == firstTermin);
                if (todaysLessons != null)
                {
                    var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                    updater.EnableNotificationQueue(true);
                    updater.Clear();

                    for (int i = 0, all = todaysLessons.Count(), counter = 1; i < all; i++)
                    {
                        var xmlContent = CreateTileAsString(todaysLessons.ElementAt(i), all, counter++);
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(xmlContent);
                        updater.Update(new TileNotification(doc));
                    }
                }
            }
        }

        public void ClearTiles() {
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
        }
    }
}

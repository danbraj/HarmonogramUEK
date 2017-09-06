using Newtonsoft.Json;
using Windows.Data.Xml.Dom;

namespace UEK_Harmonogram
{
    class Grupa
    {
        public string Nazwa { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }
        public bool IsLanguageCourse { get; set; }
        [JsonIgnore]
        public bool IsSelected { get; set; }

        public Grupa() { }

        public Grupa(IXmlNode node, string type, bool isLanguageCourse)
        {
            Nazwa = node.Attributes[2].InnerText;
            Id = node.Attributes[1].InnerText;
            Type = type;
            IsLanguageCourse = isLanguageCourse;
        }

        // mogą się też pojawić grupy zablokowane przez prowadzących
    }
}

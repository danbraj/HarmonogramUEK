using System;
using System.Linq;
using Windows.Data.Xml.Dom;

namespace UEK_Harmonogram
{
    class Lekcja
    {
        public int EPOCH { get; set; }
        public int CzasTrwania { get; set; }
        public int Punkt { get; set; }
        public string Termin { get; set; }
        public string Start { get; set; }
        public string Stop { get; set; }
        public string Przedmiot { get; set; }
        public string Typ { get; set; }
        public string Sala { get; set; }
        public string ZKim { get; set; }
        public string NazwaGrupy { get; set; }
        public string MoodleId { get; set; }
        public string Uwaga { get; set; }

        public Lekcja() { } //

        public Lekcja(IXmlNode node, Grupa grupa)
        {
            Przedmiot = node.SelectSingleNode("przedmiot").InnerText;
            Termin = node.SelectSingleNode("termin").InnerText;
            Start = node.SelectSingleNode("od-godz").InnerText;
            Stop = node.SelectSingleNode("do-godz").InnerText;
            Typ = node.SelectSingleNode("typ").InnerText;
            NazwaGrupy = grupa.Nazwa;

            if (grupa.Type == "G")
            {
                ZKim = node.SelectSingleNode("nauczyciel").InnerText;
                if (node.SelectSingleNode("nauczyciel").Attributes.Count() > 0)
                    MoodleId = node.SelectSingleNode("nauczyciel").Attributes[0].NodeValue.ToString().Substring(1);
            }
            else if (grupa.Type == "N")
                ZKim = node.SelectSingleNode("grupa").InnerText;

            if (node.SelectSingleNode("uwagi") != null)
                Uwaga = node.SelectSingleNode("uwagi").InnerText;

            string nodeSala = node.SelectSingleNode("sala").InnerText;
            if (nodeSala.Contains("Paw.A") == true)
                Sala = System.Text.RegularExpressions.Regex.Match(nodeSala, @"Paw\.A [0-9a-zA-Z]+").Groups[0].ToString();
            else
                Sala = node.SelectSingleNode("sala").InnerText;

            string[] data = node.SelectSingleNode("termin").InnerText.Split('-');
            string[] startCzas = node.SelectSingleNode("od-godz").InnerText.Split(':');
            string[] stopCzas = node.SelectSingleNode("do-godz").InnerText.Split(':');
            var startLekcja = new DateTime(
                Int32.Parse(data[0]), Int32.Parse(data[1]), Int32.Parse(data[2]),
                Int32.Parse(startCzas[0]), Int32.Parse(startCzas[1]), 0
                );

            var dogodz = new TimeSpan(Int32.Parse(stopCzas[0]), Int32.Parse(stopCzas[1]), 0);
            var odgodz = new TimeSpan(Int32.Parse(startCzas[0]), Int32.Parse(startCzas[1]), 0);

            Punkt = (int)(odgodz - new TimeSpan(7, 50, 0)).TotalMinutes;
            CzasTrwania = (int)(dogodz - odgodz).TotalMinutes;
            EPOCH = (int)(startLekcja - new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Data.Xml.Dom;

namespace UEK_Harmonogram
{
    static class Extensions
    {
        static public List<Lekcja> GetLessonsCollection(this Grupa group, string wwwContent)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(wwwContent);
            return GetLessonsFromNodes(xml.DocumentElement.SelectNodes("/plan-zajec/zajecia"), group);
        }

        static public async Task<List<Lekcja>> GetLessonsCollection(this Grupa group)
        {
            XmlDocument xml = new XmlDocument();
            using (HttpClient client = new HttpClient())
            {
                var uri = string.Format("http://planzajec.uek.krakow.pl/index.php?typ={0}&id={1}&okres=3&xml", group.Type, group.Id);
                var response = await client.GetAsync(new Uri(uri));
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = XDocument.Load(await response.Content.ReadAsStreamAsync()).ToString();
                    xml.LoadXml(result);
                }
                else
                    return null; // błąd połączenia www
            }
            return GetLessonsFromNodes(xml.DocumentElement.SelectNodes("/plan-zajec/zajecia"), group);
        }

        static private List<Lekcja> GetLessonsFromNodes(XmlNodeList nodes, Grupa group)
        {
            if (nodes.Count > 0)
            {
                List<Lekcja> lista = new List<Lekcja>();
                bool czyLektoraty = Settings.IsSetFlag(Setting.ShowLanguageCourses);

                bool czyZablokowany = nodes.First().SelectSingleNode("przedmiot").InnerText.Contains("Publikacja tego planu zajęć została zablokowana przez prowadzącego zajęcia.");
                if (!czyZablokowany)
                {
                    foreach (var node in nodes)
                    {
                        if (czyLektoraty)
                            lista.Add(new Lekcja(node, group));
                        else
                            if (node.SelectSingleNode("przedmiot").InnerText != "Język obcy" && node.SelectSingleNode("przedmiot").InnerText != "Język obcy w biznesie")
                            lista.Add(new Lekcja(node, group));
                    }
                    return lista.OrderBy(x => x.EPOCH).Cast<Lekcja>().ToList();
                }
                // plan zajęć zablokowany przez prowadzącego
            }
            // brak zajęć
            return null;
        }

        public static async Task<string> GetLessonsAsString(this Grupa group)
        {
            using (HttpClient client = new HttpClient())
            {
                var uri = string.Format("http://planzajec.uek.krakow.pl/index.php?typ={0}&id={1}&okres=3&xml", group.Type, group.Id);
                var response = await client.GetAsync(new Uri(uri));
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    if (content != null)
                        return content;
                }
            }
            return null;
        }
    }
}

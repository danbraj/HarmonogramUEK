using Microsoft.Toolkit.Uwp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace UEK_Harmonogram
{
    class Synchronizacja
    {
        public static bool IsChanged = false;

        public async Task DoSynchronize()
        {
            var jsonPackages = KeysStorage.GetKey<string>("packages", string.Empty);
            if (jsonPackages != string.Empty)
            {
                await DoSynchronize(JsonConvert.DeserializeObject<ObservableCollection<Zestaw>>(jsonPackages));
            }
        }

        public async Task DoSynchronize(ObservableCollection<Zestaw> packages)
        {
            IsChanged = false;
            List<string[]> report = new List<string[]>();

            FilesStorage storage = new FilesStorage();
            for (int i = 0, k = packages.Count; i < k; i++)
            {
                var groups = packages[i].Groups;
                if (groups != null)
                {
                    for (int j = 0, l = groups.Count; j < l; j++)
                    {
                        var group = groups[j];
                        if (group != null)
                        {
                            string[] record = new string[2];
                            record[0] = group.Nazwa;
                            bool IsNeedSynchronization = false;

                            using (HttpClient client = new HttpClient())
                            {
                                var uri = string.Format("http://planzajec.uek.krakow.pl/index.php?typ={0}&id={1}&okres=3&xml", group.Type, group.Id);
                                var response = await client.GetAsync(new Uri(uri));
                                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    string left = response.Content.ReadAsStringAsync().Result;
                                    // tutaj można by sprawdzić poprawność XML'a
                                    if (left != string.Empty)
                                    {
                                        var file = await storage.GetFileIfExists(group.Type + group.Id + ".xml");
                                        if (file != null)
                                        {
                                            string right = await Windows.Storage.FileIO.ReadTextAsync(file);

                                            var @new = Regex.Match(left, @"<zajecia>.*<\/zajecia>").Groups[0].ToString();
                                            var old = Regex.Match(right, @"<zajecia>.*<\/zajecia>").Groups[0].ToString();

                                            IsNeedSynchronization = !@new.Equals(old);
                                        }
                                    }

                                    if (IsNeedSynchronization)
                                    {
                                        IsChanged = true;
                                        await storage.StringToFile(left, group.Type + group.Id + ".xml");
                                    }
                                }

                                if (IsNeedSynchronization)
                                    record[1] = "zaktualizowano";
                                else
                                    record[1] = "brak zmian";

                                report.Add(record);
                            }
                        }
                    }
                }
            }
            ShowToast(IsChanged, report);
        }

        public static void ShowToast(bool isChanged, List<string[]> groups)
        {
            string groupsContent = string.Empty;
            int groupsCount = groups.Count;
            if (groupsCount > 0)
            {
                string leftSide = string.Empty;
                string rightSide = string.Empty;
                for (int i = 0; i < groupsCount; i++)
                {
                    leftSide += $@"<text hint-style='captionSubtle'>{groups[i][0]}</text>";
                    rightSide += $@"<text hint-style='captionSubtle' hint-align='right'>{groups[i][1]}</text>";
                }
                groupsContent += $@"<group><subgroup>{leftSide}</subgroup><subgroup>{rightSide}</subgroup></group>";
            }            

            ToastNotifier ToastNotifier = ToastNotificationManager.CreateToastNotifier();
            string content = $@"<toast launch='show-plan'>
						  <visual>
							<binding template='ToastGeneric'>
									<text hint-maxLines='1'>Raport synchronizacji</text>
									<text>{(isChanged ? "Zobacz! Nastąpiły zmiany w planie zajęć." : "Wszystko w najlepszym porządku.")}</text>
								{groupsContent}
							</binding>
						  </visual>
						  {(isChanged ? "<audio src='ms-winsoundevent:Notification.Reminder'/>" : "")}
					   </toast>";
            XmlDocument xmltest = new XmlDocument();
            xmltest.LoadXml(content);

            ToastNotification toast = new ToastNotification(xmltest);
            if (!isChanged)
            {
                toast.SuppressPopup = true;
                toast.ExpirationTime = DateTime.Now.AddHours(1);
            }
            ToastNotifier.Show(toast);
        }
    }
}

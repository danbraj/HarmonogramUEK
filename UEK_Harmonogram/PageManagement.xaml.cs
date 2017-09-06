using Microsoft.Toolkit.Uwp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace UEK_Harmonogram
{
    public sealed partial class PageManagement : Page
    {
        ObservableCollection<Zestaw> Packages = new ObservableCollection<Zestaw>();
        const int MaxPackages = 1;

        public PageManagement()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var jsonPackages = KeysStorage.GetKey<string>("packages", string.Empty);
            if (jsonPackages != string.Empty)
                Packages = JsonConvert.DeserializeObject<ObservableCollection<Zestaw>>(jsonPackages);

            if (e.Parameter != null)
            {
                var zestaw = e.Parameter as Zestaw;
                Packages.Add(zestaw);
                KeysStorage.PutKey("packages", JsonConvert.SerializeObject(Packages));
                CreateFiles();
            }

            if (Packages.Count < MaxPackages)
                btnAddZestaw.Visibility = Visibility.Visible;
        }

        private void appbtnAccept_Click(object sender, RoutedEventArgs e)
        {
            if (Packages.Count > 0)
            {
                var package = Packages.First();
                if (package.IsSeparatedGroups)
                {
                    KeysStorage.PutKey("presentation", (int)Presentation.Blocks);
                }
                else
                {
                    KeysStorage.PutKey("presentation", (int)Presentation.Items);
                }
            }
            else
            {
                KeysStorage.PutKey("presentation", (int)Presentation.Items);
                KeysStorage.PutKey("exists", false);
            }
            this.Frame.Navigate(typeof(MainPage));
        }

        private void btnAddZestaw_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(PageSearch), Mode.Save);
        }

        private void btnDeleteAction_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var zestaw = (Zestaw)btn.DataContext;
            Packages.Remove(zestaw);
            KeysStorage.PutKey("packages", JsonConvert.SerializeObject(Packages));

            if (Packages.Count < MaxPackages)
                btnAddZestaw.Visibility = Visibility.Visible;
        }

        private async void CreateFiles()
        {
            indicatorPanel.Visibility = Visibility.Visible;

            if (NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                BottomAppBar.IsEnabled = false;
                
                indicatorProgressRing.IsActive = true;
                indicatorProgressRing.Visibility = Visibility.Visible;
                indicatorText.Text = "Proszę czekać...";

                FilesStorage storage = new FilesStorage();
                for (int i = 0, k = Packages.Count; i < k; i++)
                {
                    var groups = Packages[i].Groups;
                    if (groups != null)
                    {
                        for (int j = 0, l = groups.Count; j < l; j++)
                        {
                            var group = groups[j];
                            if (group != null)
                            {
                                var content = await group.GetLessonsAsString();
                                await storage.StringToFile(content, group.Type + group.Id + ".xml");
                                //Packages[i].Groups[j].Information = Status.Zapisana;
                            }
                        }
                    }
                }
                await storage.DeleteUnusedFiles(Packages);

                indicatorProgressRing.Visibility = Visibility.Collapsed;
                indicatorProgressRing.IsActive = false;

                KeysStorage.PutKey("exists", true);
                
                //var nowEPOCH = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                //KeysStorage.PutKey("syncTime", nowEPOCH + Synchronizacja.Cooldown);

                BottomAppBar.IsEnabled = true;
                indicatorText.Text = "Wykonano pomyślnie.";
            }
            else
            {
                indicatorText.Text = "Wymagane jest połączenie internetowe.";
            }
        }
    }
}

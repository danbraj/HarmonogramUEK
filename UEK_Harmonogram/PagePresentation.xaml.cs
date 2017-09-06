using Microsoft.Toolkit.Uwp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace UEK_Harmonogram
{
    public sealed partial class PagePresentation : Page
    {
        public PagePresentation()
        {
            this.InitializeComponent();
            // cache?
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                var group = e.Parameter as Grupa;
                if (NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                {
                    var lessons = await group.GetLessonsCollection();
                    this.BindLessons(lessons);
                }
            }
            else
            {
                var packages = GetPackages();
                if (packages != null) { 
                    var lessons = await LoadLessonsFromFiles(packages);
                    this.BindLessons(lessons);
                }

                appbtnSync.Visibility = Visibility.Visible;
            }
        }

        private ObservableCollection<Zestaw> GetPackages()
        {
            var jsonPackages = KeysStorage.GetKey<string>("packages", string.Empty);
            if (jsonPackages != string.Empty)
            {
                return JsonConvert.DeserializeObject<ObservableCollection<Zestaw>>(jsonPackages);
            }
            return null;
        }

        private async Task<List<Lekcja>> LoadLessonsFromFiles(ObservableCollection<Zestaw> packages)
        {
            FilesStorage storage = new FilesStorage();
            
            if (packages.Count > 0)
            {
                var groups = packages[0].Groups;
                List<Lekcja> lessons = new List<Lekcja>();
                for (int j = 0, l = groups.Count; j < l; j++)
                {
                    var wwwContent = await storage.FileToString(groups[j].Type + groups[j].Id + ".xml");
                    if (wwwContent != null)
                    {
                        var tmp = groups[j].GetLessonsCollection(wwwContent);
                        if (tmp != null)
                            lessons.AddRange(tmp);
                    }
                }
                return lessons;
            }
            return null;
        }

        //private void BindLessons(List<Lekcja> lessons)
        //{
        //    if (lessons != null)
        //    {
        //        lessons = lessons.OrderBy(x => x.EPOCH).Cast<Lekcja>().ToList();
        //        var enumerable = lessons.AsEnumerable();
        //        src.Source = from lesson in enumerable
        //                     group lesson by lesson.Termin into grp
        //                     orderby grp.Key
        //                     select grp;
        //    }
        //}
        private void BindLessons(List<Lekcja> lessons)
        {
            if (lessons != null)
            {
                if (Settings.IsSetFlag(Setting.ActivateLiveTile) || !Settings.IsSetFlag(Setting.ShowPrevious))
                {
                    var nowEPOCH = (int)(DateTime.Now.Date - new DateTime(1970, 1, 1)).TotalSeconds;
                    var nextLessons = lessons.Where(element => element.EPOCH > nowEPOCH).ToList();

                    if (Settings.IsSetFlag(Setting.ActivateLiveTile)) { 
                        LiveTile liveTile = new LiveTile();
                        liveTile.UpdateTiles(nextLessons);
                        //liveTile.UpdateTiles(lessons);
                    }
                    lessons = !Settings.IsSetFlag(Setting.ShowPrevious) ? nextLessons : lessons;
                }

                if (lessons.Count > 0)
                {
                    lessons = lessons.OrderBy(x => x.EPOCH).Cast<Lekcja>().ToList();
                    var enumerable = lessons.AsEnumerable();
                    src.Source = from lesson in enumerable
                                 group lesson by lesson.Termin into grp
                                 orderby grp.Key
                                 select grp;
                }
                else
                    ShowMessage("WSZYSTKIE ZAJĘCIA ZOSTAŁY ZAKOŃCZONE!");
            }
            else
                ShowMessage("BRAK ZAJĘĆ");
        }

        private void ShowMessage(string text)
        {
            FindName("tbMessage");
            tbMessage.Text = text;
        }

        private void appbtnBackward_Click(object sender, RoutedEventArgs e)
        {
            // cache off?
            if (this.Frame.CanGoBack)
                this.Frame.GoBack();
            else
                this.Frame.Navigate(typeof(MainPage));
        }

        private async void appbtnSync_Click(object sender, RoutedEventArgs e)
        {
            if (NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                appbtnSync.IsEnabled = false;
                IconRotation.Begin();
                
                await new Synchronizacja().DoSynchronize();
                if (Synchronizacja.IsChanged)
                {
                    var againPackages = GetPackages();
                    if (againPackages != null)
                    {
                        var lessons = await LoadLessonsFromFiles(againPackages);
                        this.BindLessons(lessons);
                    }
                }
                
                IconRotation.Stop();
                appbtnSync.IsEnabled = true;
            }
            else
            {
                await new MessageDialog("Sprawdź połączenie z internetem.").ShowAsync();
            }
        }

        private void lvPlan_ItemClick(object sender, ItemClickEventArgs e)
        {
            var lekcja = (Lekcja)e.ClickedItem;
            this.Frame.Navigate(typeof(PageDetails), lekcja);
        }
    }
}

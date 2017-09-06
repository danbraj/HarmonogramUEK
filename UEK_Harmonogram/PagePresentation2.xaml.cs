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
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace UEK_Harmonogram
{
    public sealed partial class PagePresentation2 : Page
    {
        const int LessonBlockWidth = 295;

        List<List<Lekcja>> siatkaLekcji = new List<List<Lekcja>>();
        int liczbaGrup;

        public PagePresentation2()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            List<Grupa> Grupy = new List<Grupa>();

            var selectedGroups = e.Parameter as List<Grupa>;
            if (selectedGroups != null)
            {
                if (NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                {
                    Grupy = selectedGroups;
                    liczbaGrup = Grupy.Count;
                    if (liczbaGrup > 0)
                    {
                        for (int i = 0; i < liczbaGrup; i++)
                        {
                            //info.changeMessage("Ładowanie grupy " + Grupy[i].Nazwa + " (" + (i + 1) + "/" + liczbaGrup + ")");
                            siatkaLekcji.Add(await Grupy[i].GetLessonsCollection());
                        }
                    }
                }
                //else info.showMessage("Brak dostępu do internetu");
            }
            else
            {
                FilesStorage storage = new FilesStorage();
                var packages = GetPackages();
                if (packages != null && packages.Count > 0)
                {
                    appbtnSync.Visibility = Visibility.Visible;

                    Grupy = packages[0].Groups;
                    liczbaGrup = Grupy.Count;
                    for (int j = 0, l = Grupy.Count; j < l; j++)
                    {
                        var wwwContent = await storage.FileToString(Grupy[j].Type + Grupy[j].Id + ".xml");
                        if (wwwContent != null)
                        {
                            siatkaLekcji.Add(Grupy[j].GetLessonsCollection(wwwContent));
                        }
                    }
                } else
                    ShowMessage("BRAK ZAJĘĆ");
            }

            obszar.Children.Add(createTimeColumn());
            obszar.Children.Add(createGroupsRow(Grupy));
            obszar.Children.Add(createMain());

            cdp.Date = DateTime.Today;

            //info.Stop();
            control.IsEnabled = true;

        }

        private Canvas createMain(string data = "")
        {
            Canvas canvas = new Canvas();

            if (data != "")
            {
                int x = 50;
                for (int j = 0; j < liczbaGrup; j++)
                {
                    if (siatkaLekcji[j] != null) { 
                        var Lekcje = siatkaLekcji[j].Where(item => item.Termin == data).ToList();
                        int y = 50;
                    
                        for (int i = 0, k = Lekcje.Count; i < k; i++)
                        {

                            LessonBlock li = new LessonBlock();
                            li.DataContext = Lekcje[i];
                            li.Tapped += Li_Tapped;

                            Canvas.SetTop(li, Lekcje[i].Punkt + y);
                            Canvas.SetLeft(li, x);

                            canvas.Children.Add(li);
                        }
                    }
                    x += LessonBlockWidth;
                }
                //todo? ustawienia dynamicznego rozmiaru (procentowy)?
                obszar.Width = x;
            }
            return canvas;
        }

        private void Li_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var lekcja = (Lekcja)((LessonBlock)sender).DataContext;
            this.Frame.Navigate(typeof(PageDetails), lekcja);
        }

        private Canvas createTimeColumn()
        {
            Canvas canvas = new Canvas();

            var hours = new string[] { "7:50", "9:35", "11:20", "13:05", "14:50", "16:30", "18:10", "19:45" };

            for (int i = 0, k = hours.Count(); i < k;)
            {
                var time = new TextBlock();
                var j = i.ToString();
                time.Text = hours[i];
                time.TextAlignment = TextAlignment.Center;
                time.Width = 40;
                time.Height = 105;

                Canvas.SetTop(time, 105 * (i++) + 40);
                canvas.Children.Add(time);
            }
            return canvas;
        }

        private Canvas createGroupsRow(List<Grupa> Groups)
        {
            Canvas canvas = new Canvas();

            for (int i = 0, k = Groups.Count; i < k;)
            {
                var group = new TextBlock();
                group.Text = Groups[i].Nazwa;
                group.TextAlignment = TextAlignment.Center;
                group.Width = LessonBlockWidth;
                group.Height = 40;
                group.FontSize = 16;
                group.FontWeight = FontWeights.Bold;

                Canvas.SetLeft(group, LessonBlockWidth * (i++) + 50);
                canvas.Children.Add(group);
            }
            return canvas;
        }

        private void CalendarDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            var data = sender.Date.Value.ToString("yyyy-MM-dd");
            obszar.Children.RemoveAt(2);
            obszar.Children.Add(createMain(data));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            cdp.Date = cdp.Date.Value.AddDays(-1);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            cdp.Date = cdp.Date.Value.AddDays(1);
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
                    if (this.Frame.CanGoBack)
                        this.Frame.GoBack();
                    else
                        this.Frame.Navigate(typeof(MainPage));
                }

                IconRotation.Stop();
                appbtnSync.IsEnabled = true;
            }
            else
            {
                await new MessageDialog("Sprawdź połączenie z internetem.").ShowAsync();
            }
        }

        private void appbtnBackward_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
                this.Frame.GoBack();
            else
                this.Frame.Navigate(typeof(MainPage));
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

        private void ShowMessage(string text)
        {
            FindName("tbMessage");
            tbMessage.Text = text;
        }
    }
}

using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Data.Xml.Dom;
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
    enum Mode
    {
        Search,
        Save
    }

    public sealed partial class PageSearch : Page
    {
        ObservableCollection<Grupa> Groups = new ObservableCollection<Grupa>();
        bool IsMultipleSelection = false;
        bool IsGroupsLoaded = false;
        Mode mode = Mode.Search;

        public PageSearch()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                mode = (Mode)e.Parameter;
                if (mode == Mode.Save)
                {
                    appbtnConfirm.Label = "Zapisz";
                    appbtnConfirm.Icon = new SymbolIcon(Symbol.Save);
                }
                else
                {
                    appbtnConfirm.Label = "Pokaż";
                    appbtnConfirm.Icon = new SymbolIcon(Symbol.View);
                }

                if (!this.IsGroupsLoaded)
                {
                    indicatorPanel.Visibility = Visibility.Visible;

                    if (NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                    {
                        indicatorProgressRing.IsActive = true;
                        indicatorProgressRing.Visibility = Visibility.Visible;

                        indicatorText.Text = "Pobieranie listy grup...";
                        await GetGroupsFromURL(Groups, "G", "^Kr.*|SJO-.*$");
                        indicatorText.Text = "Pobieranie listy prowadzących...";
                        await GetGroupsFromURL(Groups, "N", "^.* .*, .+$");

                        indicatorPanel.Visibility = Visibility.Collapsed;
                        indicatorText.Text = "";
                        indicatorProgressRing.Visibility = Visibility.Collapsed;
                        indicatorProgressRing.IsActive = false;

                        autoBoxSearch.Visibility = Visibility.Visible;
                        this.IsGroupsLoaded = true;
                    }
                    else
                        indicatorText.Text = "Sprawdź połączenie z internetem";
                }
            }
        }

        private async static Task GetGroupsFromURL(ObservableCollection<Grupa> groups, string type, string pattern)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var uri = string.Format("http://planzajec.uek.krakow.pl/index.php?typ={0}&xml", type);
                    var response = await client.GetAsync(new Uri(uri));
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        XDocument xml = XDocument.Load(await response.Content.ReadAsStreamAsync());
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(xml.ToString());
                        XmlNodeList nodes = xmlDoc.DocumentElement.SelectNodes("/plan-zajec/zasob");
                        foreach (var node in nodes)
                            if (Regex.IsMatch(node.Attributes[2].InnerText, pattern))
                            {
                                if (node.Attributes[2].InnerText.StartsWith("SJO-"))
                                {
                                    groups.Add(new Grupa(node, type, true));
                                }
                                else
                                {
                                    groups.Add(new Grupa(node, type, false));
                                }
                            }     
                    }
                }
                return;
            }
            catch (HttpRequestException exception)
            {
                System.Diagnostics.Debug.WriteLine("CAUGHT EXCEPTION:");
                System.Diagnostics.Debug.WriteLine(exception);
            }
        }

        private void autoBoxSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            string filter = sender.Text.ToUpper();
            if (this.IsMultipleSelection)
            {
                List<Grupa> selectedGroups = lvGroups.SelectedItems.Cast<Grupa>().ToList();
                lvGroups.ItemsSource = Groups.Where(s => (s.IsSelected) || (s.Nazwa.ToUpper().Contains(filter)));
                foreach (var s in selectedGroups)
                    lvGroups.SelectedItems.Add(s);
            }
            else
                lvGroups.ItemsSource = Groups.Where(s => s.Nazwa.ToUpper().Contains(filter));
        }

        private async void appbtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            List<Grupa> selectedGroups = new List<Grupa>();

            foreach (Grupa item in lvGroups.SelectedItems)
                selectedGroups.Add(item);

            if (selectedGroups.Count < 2 || selectedGroups.Count > 10)
            {
                var msg = new MessageDialog("Możliwa liczba grup do porównania musi znajdować się w zakresie 2-10.");
                await msg.ShowAsync();
            }
            else
            {
                if (mode == Mode.Save)
                {
                    this.Frame.Navigate(typeof(PageManagement), new Zestaw(selectedGroups));
                }
                else
                    this.Frame.Navigate(typeof(PagePresentation2), selectedGroups);
            }
        }

        private void appbtnBackward_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack) this.Frame.GoBack();
        }

        private void apptbtnSwitch_Checked(object sender, RoutedEventArgs e)
        {
            this.IsMultipleSelection = true;
            var apptbtn = (AppBarToggleButton)sender;
            apptbtn.Label = "Wiele grup";
            appbtnConfirm.Visibility = Visibility.Visible;
            lvGroups.SelectionMode = ListViewSelectionMode.Multiple;
        }

        private void apptbtnSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            this.IsMultipleSelection = false;
            var apptbtn = (AppBarToggleButton)sender;
            apptbtn.Label = "Pojedyńcza grupa";
            appbtnConfirm.Visibility = Visibility.Collapsed;
            lvGroups.SelectionMode = ListViewSelectionMode.Single;
        }

        private void lvGroups_ItemClick(object sender, ItemClickEventArgs e)
        {
            var group = (Grupa)e.ClickedItem;
            if (this.IsMultipleSelection)
                group.IsSelected = !group.IsSelected;
            else
            {
                if (mode == Mode.Save)
                {
                    var groups = new List<Grupa>
                    {
                        group
                    };
                    this.Frame.Navigate(typeof(PageManagement), new Zestaw(groups));
                }
                else
                    this.Frame.Navigate(typeof(PagePresentation), group);
            }
        }
    }
}

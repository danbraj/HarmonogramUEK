using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
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
    enum Presentation
    {
        Items,
        Blocks
    }

    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            SetViewButtonAvailibility();
        }

        private void SetViewButtonAvailibility()
        {
            btnView.IsEnabled = KeysStorage.GetKey<bool>("exists", false);
        }

        private void btnManagement_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(PageManagement));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(PageSearch), Mode.Search);
        }

        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            var value = KeysStorage.GetKey<Presentation>("presentation", (int)Presentation.Items);
            if (value == Presentation.Items)
                this.Frame.Navigate(typeof(PagePresentation));
            else
                this.Frame.Navigate(typeof(PagePresentation2));
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(PageSettings));
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(PageAbout));
        }
    }
}

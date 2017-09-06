using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
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
    public sealed partial class PageSettings : Page
    {
        public PageSettings()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            TSwitchAfterLaunch.IsOn         = Settings.IsSetFlag(Setting.ShowAfterLaunch);
            TSwitchPrevious.IsOn            = Settings.IsSetFlag(Setting.ShowPrevious);
            TSwitchLanguageCourses.IsOn     = Settings.IsSetFlag(Setting.ShowLanguageCourses);
            TSwitchLiveTile.IsOn            = Settings.IsSetFlag(Setting.ActivateLiveTile);
            TSwitchAutoSync.IsOn            = Settings.IsSetFlag(Setting.ActivateAutoSync);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Settings.SetFlagValue(TSwitchAfterLaunch.IsOn, Setting.ShowAfterLaunch);
            Settings.SetFlagValue(TSwitchPrevious.IsOn, Setting.ShowPrevious);
            Settings.SetFlagValue(TSwitchLanguageCourses.IsOn, Setting.ShowLanguageCourses);
            Settings.SetFlagValue(TSwitchLiveTile.IsOn, Setting.ActivateLiveTile);
            Settings.SetFlagValue(TSwitchAutoSync.IsOn, Setting.ActivateAutoSync);

            KeysStorage.PutKey("setting", (byte)Settings.FlagsValue);
            if (!Settings.IsSetFlag(Setting.ActivateLiveTile))
                new LiveTile().ClearTiles();

            if (Settings.IsSetFlag(Setting.ActivateAutoSync))
            {
                RegisterBackgroundTask("AutoSync Task");
            } 
            else
                BackgroundTaskHelper.Unregister("AutoSync Task");
        }

        private async void RegisterBackgroundTask(string taskName)
        {
            if (IsBackgroundTaskRegistered(taskName))
            {
                return;
            }

            // Check for background access.
            await BackgroundExecutionManager.RequestAccessAsync();

            // Registering Single-Process Background task
            BackgroundTaskHelper.Register(taskName, new TimeTrigger(480, false), false, true, new SystemCondition(SystemConditionType.InternetAvailable));
            //BackgroundTaskHelper.Register(taskName, new TimeTrigger(20, false));
        }

        private bool IsBackgroundTaskRegistered(string taskName)
        {
            if (BackgroundTaskHelper.IsBackgroundTaskRegistered(taskName))
            {
                // Background task already registered.
                return true;
            }

            return false;
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TSwitchAfterLaunch.IsOn =       false;
            TSwitchPrevious.IsOn =          false;
            TSwitchLanguageCourses.IsOn =   true;
            TSwitchLiveTile.IsOn =          false;
            TSwitchAutoSync.IsOn =          false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AmbientSounds.Views.Dialogs
{
    public sealed partial class SettingsDialog : ContentDialog
    {
        public SettingsDialog()
        {
            InitializeComponent();
            object themeObject = ApplicationData.Current.LocalSettings.Values["themeSetting"];
            if (themeObject != null)
            {
                string theme = themeObject.ToString();
                switch (theme)
                {
                    case "light":
                        LightThemeButton.IsChecked = true;
                        break;
                    case "dark":
                        DarkThemeButton.IsChecked = true;
                        break;
                    case "default":
                        DefaultThemeButton.IsChecked = true;
                        break;
                    default:
                        break;
                }
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            switch (radioButton.Name)
            {
                case "DarkThemeButton":
                    ApplicationData.Current.LocalSettings.Values["themeSetting"] = "dark";
                    break;
                case "LightThemeButton":
                    ApplicationData.Current.LocalSettings.Values["themeSetting"] = "light";
                    break;
                case "DefaultThemeButton":
                    ApplicationData.Current.LocalSettings.Values["themeSetting"] = "default";
                    break;
                default:
                    break;
            }
            ThemeWarning.Visibility = Visibility.Visible;
        }
    }
}
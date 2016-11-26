using Ini;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SideNote
{
    public partial class SettingsWindow
    {
        // Pathes & files
        public string SaveFolder, SaveFolderTmp, AppdataDir;
        public string[] SplittedSaveFolder;
        public string AppName = "Notes";
        public string SaveFile = "Settings.ini";
        //public string CompleteSavePath;

        // Settings
        List<string> ThemeChoices;// = { "Dark Orange", "Light" };

        // Objects
        FontConverter FontConverter = new FontConverter();

        // Events
        public static readonly RoutedEvent HideEvent = EventManager.RegisterRoutedEvent(
        "DoHide", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SettingsWindow));
        public event RoutedEventHandler DoHide
        {
            add { AddHandler(HideEvent, value); }
            remove { RemoveHandler(HideEvent, value); }
        }

        public static readonly RoutedEvent ShowEvent = EventManager.RegisterRoutedEvent(
        "DoShow", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SettingsWindow));
        public event RoutedEventHandler DoShow
        {
            add { AddHandler(ShowEvent, value); }
            remove { RemoveHandler(ShowEvent, value); }
        }

        public SettingsWindow()
        {
            InitializeComponent();
            this.AllowsTransparency = true;

            //this.WindowState = System.Windows.WindowState.Minimized;

            ThemeChoices = readThemeChoices();

            // Set up settings
            FontFamilyDrop.ItemsSource = Fonts.SystemFontFamilies;  // Dropdown Liste mit Werten füllen
            AppThemeDrop.ItemsSource = ThemeChoices;

            loadSettings();

            // Set up listeners
            SettingsCloseButton.Click += SettingsCloseButton_Click;
            FontSizeSlider.ValueChanged += FontSizeSlider_ValueChanged;
            FontFamilyDrop.SelectionChanged += FontFamilyDrop_SelectionChanged;
            AppThemeDrop.SelectionChanged += AppThemeDrop_SelectionChanged;
        }

        // Helper functions
        private List<string> readThemeChoices()
        {
            var input = MainWindow.GetResourcesUnder("Themes");
            var output = new List<string>();

            for (int i = 0; i < input.Length; i++)
            {
                var no_extension = System.IO.Path.GetFileNameWithoutExtension(input[i]);
                var escaped = Uri.UnescapeDataString(no_extension);
                var titlecase = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(escaped);
                output.Add(titlecase);
            }

            return output;
        }

        private void updateTextBox()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    (window as MainWindow).myTextBox.FontFamily = new System.Windows.Media.FontFamily(StorageHolder.readSetting(MainWindow.Key_FontFamily));
                    (window as MainWindow).myTextBox.FontSize = Double.Parse(StorageHolder.readSetting(MainWindow.Key_FontSize));
                }
            }
        }

        private void loadSettings()
        {
            System.Windows.Media.FontFamily[] FontFamilyArray = Fonts.SystemFontFamilies.ToArray();
            for (int i = 0; i < FontFamilyArray.GetLength(0); i++)
            {
                if (FontFamilyArray[i].ToString() == StorageHolder.readSetting(MainWindow.Key_FontFamily))
                {
                    FontFamilyDrop.SelectedIndex = i;
                }
            }

            FontSizeSlider.Value = Double.Parse(StorageHolder.readSetting(MainWindow.Key_FontSize));


            for (int i = 0; i < ThemeChoices.Count; i++)
            {
                if (ThemeChoices[i].ToString() == StorageHolder.readSetting(MainWindow.Key_AppTheme))
                {
                    AppThemeDrop.SelectedIndex = i;
                }
            }
        }

        // Events
        private void FontFamilyDrop_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StorageHolder.writeSetting(MainWindow.Key_FontFamily, FontFamilyDrop.SelectedValue.ToString());
            updateTextBox();
        }

        private void FontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            StorageHolder.writeSetting(MainWindow.Key_FontSize, FontSizeSlider.Value.ToString());
            updateTextBox();
        }

        private void SettingsWindow_DragMove(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void AppThemeDrop_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StorageHolder.writeSetting(MainWindow.Key_AppTheme, AppThemeDrop.SelectedValue.ToString());
            ThemeEngine.applyTheme(AppThemeDrop.SelectedValue.ToString());
        }


        // Window events
        private void SettingsCloseButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseHideEvent();
        }

        public void RaiseHideEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(SettingsWindow.HideEvent);
            RaiseEvent(newEventArgs);
        }

        public void RaiseShowEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(SettingsWindow.ShowEvent);
            RaiseEvent(newEventArgs);
        }

        public void changeVisibility(bool mode)
        {
            if (mode && this.Visibility == Visibility.Hidden)
            {
                RaiseShowEvent();
            }
            else
            {
                RaiseHideEvent();
            }
        }
    }
}

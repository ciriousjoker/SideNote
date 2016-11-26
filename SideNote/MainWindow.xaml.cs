using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ini;
using System.Reflection;
using System.IO;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Resources;
using System.Collections;
using System.Globalization;

namespace SideNote
{
    // TODO: Make draggable tabs
    // TODO: Support Markdown

    public partial class MainWindow
    {
        // App properties
        static string AppName = Assembly.GetEntryAssembly().GetName().Name;

        // Settings keys
        public static string Key_FontSize = "FontSize";
        public static string Key_FontFamily = "FontFamily";
        public static string Key_AppTheme = "Theme";

        // Settings default values
        static string Default_FontSize = "14";
        static string Default_FontFamily = "Segoe UI";

        // Environment properties
        int ScreenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
        int ScreenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

        // Timers
        DispatcherTimer KeyHookTimer = new System.Windows.Threading.DispatcherTimer();
        DispatcherTimer QuickSaveTimer = new System.Windows.Threading.DispatcherTimer();

        // Settings
        public System.Windows.Media.Color FontColor, BackgroundColor;
        public string myFontSize, myFontFamily;

        

        // Objects
        public SideNote.SettingsWindow SettingsWindow;

        // Events
        public static readonly RoutedEvent HideEvent = EventManager.RegisterRoutedEvent(
        "DoHide", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MainWindow));
        public event RoutedEventHandler DoHide
        {
            add { AddHandler(HideEvent, value); }
            remove { RemoveHandler(HideEvent, value); }
        }

        public static readonly RoutedEvent ShowEvent = EventManager.RegisterRoutedEvent(
        "DoShow", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MainWindow));
        public event RoutedEventHandler DoShow
        {
            add { AddHandler(ShowEvent, value); }
            remove { RemoveHandler(ShowEvent, value); }
        }

        public static readonly RoutedEvent CloseEvent = EventManager.RegisterRoutedEvent(
        "DoClose", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MainWindow));
        public event RoutedEventHandler DoClose
        {
            add { AddHandler(ShowEvent, value); }
            remove { RemoveHandler(ShowEvent, value); }
        }

        // Dll calls
        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);

        public MainWindow()
        {
            InitializeComponent();

            writeAutostart();
            initializeIni();

            ThemeEngine.applyTheme(StorageHolder.readSetting(MainWindow.Key_AppTheme));

            // Set up buttons
            TitlebarCloseButton.Click += TitlebarCloseButton_Click;
            TitlebarSettingsButton.Click += TitlebarSettingsButton_Click;
            TitlebarResizeButton.Click += TitlebarHideButton_Click;

            // Set up window properties
            this.AllowsTransparency = true;
            this.Topmost = true;
            this.WindowState = System.Windows.WindowState.Normal;
            this.Show();

            this.Left = ScreenWidth - this.ActualWidth;
            this.Top = 0;
            this.MinHeight = ScreenHeight;
            this.MaxHeight = ScreenHeight;

            // Set up animations
            Animation_Hide_Move.To = Animation_Show_Move.From = ScreenWidth;
            Animation_Hide_Move.From = Animation_Show_Move.To = ScreenWidth - this.ActualWidth;
            Animation_Close_Move.To = ScreenHeight;

            // Timers
            KeyHookTimer.Interval = TimeSpan.FromSeconds(0.1);
            KeyHookTimer.Tick += KeyHookTimerTick;
            KeyHookTimer.Start();

            QuickSaveTimer.Tick += QuickSaveTimerTick;
            QuickSaveTimer.Interval = new TimeSpan(0, 0, 20);
            QuickSaveTimer.Start();

            // Load window content
            loadTextBox();

            // Set up the settings window
            SettingsWindow = new SideNote.SettingsWindow();
            SettingsWindow.Show();
            SettingsWindow.WindowState = System.Windows.WindowState.Normal;
            SettingsWindow.Hide();
        }

        // Helper functions
        private void initializeIni()
        {
            try
            {
                if (String.IsNullOrEmpty(StorageHolder.readSetting(Key_FontSize)))
                {
                    StorageHolder.writeSetting(Key_FontSize, Default_FontSize);
                }
                if (String.IsNullOrEmpty(StorageHolder.readSetting(Key_FontFamily)))
                {
                    StorageHolder.writeSetting(Key_FontFamily, Default_FontFamily);
                }
                if (String.IsNullOrEmpty(StorageHolder.readSetting(Key_AppTheme)))
                {
                    StorageHolder.writeSetting(Key_AppTheme, ThemeEngine.DefaultTheme);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occured while trying to set up the settings:\n\n" + e.ToString());
                Application.Current.Shutdown();
            }

        }

        static public void generatePathes()
        {

            /*
            // Only generate again if not already set up
            if (string.IsNullOrEmpty(SaveFolder) ||
                SplittedSaveFolder.Length == 0 ||
                string.IsNullOrEmpty(AppdataDir) ||
                !Directory.Exists(AppdataDir + @"\" + AppName) ||
                string.IsNullOrEmpty(CompleteSavePath) ||
                string.IsNullOrEmpty(IniSavePath))
            {
                SaveFolder = System.Windows.Forms.Application.UserAppDataPath.ToString();
                string SaveFolderTmp = SaveFolder.Replace(@"\", "?");
                SplittedSaveFolder = SaveFolderTmp.Split(new char[] { '?' });
                AppdataDir = SplittedSaveFolder[0] + @"\" + SplittedSaveFolder[1] + @"\" + SplittedSaveFolder[2] + @"\" + SplittedSaveFolder[3] + @"\" + SplittedSaveFolder[4];
                Directory.CreateDirectory(AppdataDir + @"\" + AppName);
                CompleteSavePath = AppdataDir + @"\" + AppName + @"\" + SaveFile;
                IniSavePath = AppdataDir + @"\" + AppName + @"\" + IniSaveFile;
            }
            */
        }

        public static string[] GetResourcesUnder(string folder)
        {
            folder = folder.ToLower() + "/";

            var assembly = Assembly.GetCallingAssembly();
            var resourcesName = assembly.GetName().Name + ".g.resources";
            var stream = assembly.GetManifestResourceStream(resourcesName);
            var resourceReader = new ResourceReader(stream);

            var resources =
                from p in resourceReader.OfType<DictionaryEntry>()
                let theme = (string)p.Key
                where theme.StartsWith(folder)
                select theme.Substring(folder.Length);

            return resources.ToArray();
        }

        private void loadTextBox()
        {
            myTextBox.FontSize = Convert.ToDouble(StorageHolder.readSetting(Key_FontSize));
            myTextBox.FontFamily = new System.Windows.Media.FontFamily(StorageHolder.readSetting(Key_FontFamily));

            readNotes();
        }

        private void readNotes()
        {
            myTextBox.Text = StorageHolder.readText();
        }

        private void saveNotes()
        {
            StorageHolder.writeText(myTextBox.Text);
        }

        private void closeWindow()
        {
            Application.Current.Shutdown();
        }

        // Registry stuff
        private void writeAutostart()
        {
            MainWindow.generatePathes();
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", AppName, System.Reflection.Assembly.GetEntryAssembly().Location.ToString());
        }

        // Timers
        private void KeyHookTimerTick(object sender, EventArgs e)
        {
            if (GetAsyncKeyState(0x09) == -32767 && GetAsyncKeyState(0x10) == -32767) // shift & tab
            {
                if (this.Visibility == Visibility.Visible)
                {
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window.GetType() == typeof(SettingsWindow))
                        {
                            (window as SettingsWindow).RaiseHideEvent();
                        }
                        else if (window.GetType() == typeof(MainWindow))
                        {
                            (window as MainWindow).RaiseHideEvent();
                        }
                    }
                }
                else
                {
                    RaiseShowEvent();
                }
            }
        }
        private void QuickSaveTimerTick(object sender, EventArgs e)
        {
            saveNotes();
        }

        // Window events
        private void Window_Closed(object sender, EventArgs e)
        {
            saveNotes();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Animation_Hide_Move.To = Animation_Show_Move.From = ScreenWidth;
            Animation_Hide_Move.From = Animation_Show_Move.To = ScreenWidth - this.ActualWidth;
        }


        // Button listeners
        private void TitlebarHideButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(SettingsWindow))
                {
                    (window as SettingsWindow).RaiseHideEvent();
                }
                else if (window.GetType() == typeof(MainWindow))
                {
                    (window as MainWindow).RaiseHideEvent();
                }
            }
        }

        private void TitlebarSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(SettingsWindow))
                {
                    if ((window as SettingsWindow).Visibility == Visibility.Hidden)
                    {
                        (window as SettingsWindow).changeVisibility(true);
                    }
                    else
                    {
                        (window as SettingsWindow).changeVisibility(false);
                    }
                }
            }
        }

        private void CloseAnimation_Completed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void TitlebarCloseButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(SettingsWindow))
                {
                    (window as SettingsWindow).RaiseHideEvent();
                }
                else if (window.GetType() == typeof(MainWindow))
                {
                    (window as MainWindow).RaiseCloseEvent();
                }
            }
        }


        // Animations
        void RaiseHideEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(MainWindow.HideEvent);
            RaiseEvent(newEventArgs);
        }

        void RaiseShowEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(MainWindow.ShowEvent);
            RaiseEvent(newEventArgs);
        }

        void RaiseCloseEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(MainWindow.CloseEvent);
            RaiseEvent(newEventArgs);
        }
    }
}

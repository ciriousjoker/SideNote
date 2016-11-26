using Ini;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SideNote
{
    static class StorageHolder
    {
        static string AppName = Assembly.GetEntryAssembly().GetName().Name;

        static string IniSettingsGroup = "Settings";


        // Pathes & files
        public static string SaveFile = "saveFile.note.txt";
        public static string IniSaveFile = "Settings.ini";
        public static string AppdataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public static string CompleteSavePath = AppdataDir + @"\" + AppName + @"\" + SaveFile;
        public static string IniSavePath = AppdataDir + @"\" + AppName + @"\" + IniSaveFile;

        static IniFile SettingsIni;

        static string WelcomeString = "Press <shift> + <tab> to toggle visibility";

        static Dictionary<string, string> SettingsCache = new Dictionary<string, string>();


        static StorageHolder()
        {
            try
            {
                Directory.CreateDirectory(AppdataDir + @"\" + AppName);
                SettingsIni = new IniFile(IniSavePath);
            }
            catch (Exception e)
            {
                MessageBox.Show("Something went wrong while loading the settings:\n" + e.ToString(), "Sorry");
                Application.Current.Shutdown();
            }

        }

        // Settings
        public static bool writeSetting(string key, string value)
        {
            try
            {
                if (!SettingsCache.Keys.Contains(key))
                {
                    SettingsCache.Add(key, value);
                }
                else
                {
                    SettingsCache[key] = value;
                }
                SettingsIni.IniWriteValue(IniSettingsGroup, key, value);
            }
            catch (Exception e)
            {
                MessageBox.Show("Something went wrong while saving the settings:\n" + e.ToString(), "Sorry");
                return false;
            }
            return true;
        }

        public static string readSetting(string key)
        {
            try
            {
                if (SettingsCache.Keys.Contains(key))
                {
                    if (!String.IsNullOrWhiteSpace(SettingsCache[key]))
                    {
                        return SettingsCache[key];
                    }
                }
                else
                {
                    SettingsCache.Add(key, SettingsIni.IniReadValue(IniSettingsGroup, key));
                }
                return SettingsCache[key];
            }
            catch (Exception e)
            {
                MessageBox.Show("Something went wrong while loading a setting:\n" + e.ToString(), "Sorry");
                Application.Current.Shutdown();
            }
            return "";
        }

        // Text
        public static bool writeText(string text)
        {
            try
            {
                File.WriteAllText(CompleteSavePath, text);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Couldn't save the text:\n" + e.ToString() + "\n\nMake sure the application is allowed to access %appdata%/" + AppName + "/", "Sorry");
                return false;
            }
        }

        public static string readText()
        {
            try
            {
                if(File.Exists(CompleteSavePath))
                {
                    return File.ReadAllText(CompleteSavePath);
                }
                return WelcomeString;
            }
            catch (Exception e)
            {
                MessageBox.Show("Couldn't read the text:\n" + e.ToString() + "\n\nMake sure the application is allowed to access %appdata%/" + AppName + "/", "Sorry");
                Application.Current.Shutdown();
                return "";
            }
        }
    }
}

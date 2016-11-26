using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SideNote
{
    static class ThemeEngine
    {
        public static string DefaultTheme = "Dark Orange";
        static string Default_Theme_URI = "pack://application:,,,/Themes/" + DefaultTheme + ".xaml";

        static string[] DefaultSources = {
            "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml",
            "pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Primary/MaterialDesignColor.Orange.xaml",
            "pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Accent/MaterialDesignColor.Lime.xaml",

            "pack://application:,,,/MaterialDesignColors;component/Themes/MaterialDesignColor.Grey.Named.xaml",
            "pack://application:,,,/WindowSettings/TextBlock.xaml"
        };



        static ThemeEngine()
        {

        }

        public static void applyTheme(string name)
        {
            ResourceDictionary dict = new ResourceDictionary();

            string Resource_URI = "pack://application:,,,/Themes/" + name + ".xaml";



            Application.Current.Resources.MergedDictionaries.Clear();

            try
            {
                dict.Source = new Uri(Resource_URI);
            }
            catch (Exception)
            {
                MessageBox.Show("Something went wrong while loading the theme and it has been reverted to the light one.", "Sorry");
                dict.Source = new Uri(Default_Theme_URI);
                StorageHolder.writeSetting(MainWindow.Key_AppTheme, DefaultTheme);
            }

            Application.Current.Resources.MergedDictionaries.Add(dict);
        }


        // Helper functions
        private static bool ResourceExists(string resourcePath)
        {
            var assembly = Assembly.GetExecutingAssembly();

            return ResourceExists(assembly, resourcePath);
        }

        private static bool ResourceExists(Assembly assembly, string resourcePath)
        {
            return GetResourcePaths(assembly)
                .Contains(resourcePath.ToLowerInvariant());
        }

        private static IEnumerable<object> GetResourcePaths(Assembly assembly)
        {
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            var resourceName = assembly.GetName().Name + ".g";
            var resourceManager = new ResourceManager(resourceName, assembly);

            try
            {
                var resourceSet = resourceManager.GetResourceSet(culture, true, true);

                foreach (System.Collections.DictionaryEntry resource in resourceSet)
                {
                    yield return resource.Key;
                }
            }
            finally
            {
                resourceManager.ReleaseAllResources();
            }
        }
    }


}

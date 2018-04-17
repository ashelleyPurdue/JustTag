using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

namespace JustTag
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void installShellButton_Click(object sender, RoutedEventArgs e)
        {
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            Registry.SetValue
            (
                @"HKEY_CLASSES_ROOT\*\shell\JustTag",
                "",
                "Open w&ith JustTag"
            );

            Registry.SetValue
            (
                @"HKEY_CLASSES_ROOT\*\shell\JustTag",
                "Icon",
                exePath
            );

            Registry.SetValue
            (
                @"HKEY_CLASSES_ROOT\*\shell\JustTag\command",
                "",
                "\"" + exePath + "\" \"%1\""
            );
        }
    }
}

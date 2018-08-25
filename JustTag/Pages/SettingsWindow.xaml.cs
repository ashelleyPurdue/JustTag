using Microsoft.Win32;
using System.Windows;
using System.Security.Principal;

namespace JustTag.Pages
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;

        private string[] registryPaths = new string[]
        {
            @"*\shell\JustTag",                     // Right-click option on files
            @"Directory\shell\JustTag",             // Right-click option on directories
            @"Directory\Background\shell\JustTag"   // Right-click option on the background of the current directory
        };

        public SettingsWindow()
        {
            InitializeComponent();
            InitializeContextMenuCheckbox();
        }

        // Misc methods
        private void InitializeContextMenuCheckbox()
        {
            // Disable the checkbox if we're not in admin mode.
            // Stolen from https://stackoverflow.com/questions/5953240/c-sharp-administrator-privilege-checking
            bool isElevated;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            if (!isElevated)
                installContextMenuCheckbox.IsEnabled = false;

            // Start the install checkbox as checked if it's already installed(and has the correct paths)
            if (IsContextMenuInstalled())
            {
                // We need to temporarily unsubscribe it from the Checked event, because simply changing
                // IsChecked will trigger it otherwise.
                installContextMenuCheckbox.Checked -= installContextMenuCheckbox_Checked;
                installContextMenuCheckbox.IsChecked = true;
                installContextMenuCheckbox.Checked += installContextMenuCheckbox_Checked;
            }
        }
        
        /// <summary>
        /// Checks to see if the "Open with JustTag" option has been installed in
        /// Windows Explorer.  It is installed if all the required registry entries
        /// are present and set to the correct value.
        /// </summary>
        /// <returns></returns>
        private bool IsContextMenuInstalled()
        {
            string expectedVal = "\"" + exePath + "\" \"%1\"";

            // Return false if any of the paths don't match
            foreach (string path in registryPaths)
            {
                string fullPath = "HKEY_CLASSES_ROOT\\" + path + "\\command";
                string regVal = Registry.GetValue(fullPath, "", null) as string;

                if (regVal != expectedVal)
                    return false;
            }

            // They all exist and match, so return true
            return true;
        }


        // Event handlers

        private void installContextMenuCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            // Set the proper entries in the registry to make this program show up in the
            // context menu
            foreach (string path in registryPaths)
            {
                string fullPath = "HKEY_CLASSES_ROOT\\" + path;

                Registry.SetValue(fullPath, "", "Open w&ith JustTag");  // Set the text on the context menu button
                Registry.SetValue(fullPath, "Icon", exePath);           // Set the icon to match the executable's icon
                Registry.SetValue(fullPath + "\\command", "", "\"" + exePath + "\" \"%V\"");    // Set this program as the command to run
            }

        }

        private void installContextMenuCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Delete the keys added by install
            foreach (string path in registryPaths)
                Registry.ClassesRoot.DeleteSubKeyTree(path, false);
        }
    }
}

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
using System.IO;
using JustTag.Controls;
using JustTag.Tagging;

namespace JustTag.Pages
{
    /// <summary>
    /// Interaction logic for FindReplaceTagsWindow.xaml
    /// </summary>
    public partial class FindReplaceTagsWindow : Window
    {
        private string directory;

        public FindReplaceTagsWindow(string directory, IEnumerable<string> autoCompleteTags)
        {
            InitializeComponent();

            this.directory = directory;

            // Set the autocomplete sources
            findTextbox.autoCompletionSource = autoCompleteTags;
            replaceTextbox.autoCompletionSource = autoCompleteTags;
        }

        private async void replaceButton_Click(object sender, RoutedEventArgs e)
        {
            string findTag = findTextbox.Text;
            string replaceTag = replaceTextbox.Text;

            // Disable the controls while we wait
            IsEnabled = false;
            progressBar.Visibility = Visibility.Visible;

            // Replace the tags asynchronously
            await Task.Run(() => TagUtils.RenameTag(directory, findTag, replaceTag));

            // Close this window
            Close();
        }

        private void textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            replaceButton.IsEnabled = true;

            AutoCompleteTextbox[] boxes = { findTextbox, replaceTextbox };
            foreach (AutoCompleteTextbox tb in boxes)
            {
                bool valid = Utils.IsTagValid(tb.Text);

                // Turn this box red if it's invalid
                tb.Background = valid ? Brushes.White : Brushes.Red;

                // Disable the button if it's invalid
                if (!valid)
                    replaceButton.IsEnabled = false;
            }
        }
    }
}

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
using JustTag.Tagging;

namespace JustTag.Pages
{
    /// <summary>
    /// Interaction logic for DeleteTagWindow.xaml
    /// </summary>
    public partial class DeleteTagWindow : Window
    {
        private string directory;

        public DeleteTagWindow(string directory, IEnumerable<string> autoCompleteTags)
        {
            InitializeComponent();
            this.directory = directory;
            deleteTextbox.autoCompletionSource = autoCompleteTags;
        }

        // Events
        private async void goButton_Click(object sender, RoutedEventArgs e)
        {
            string tag = deleteTextbox.Text;

            // Disable the controls while we wait
            IsEnabled = false;
            progressBar.Visibility = Visibility.Visible;

            // Replace the tags asynchronously
            await Task.Run(() => TagUtils.DeleteTag(directory, tag));

            // Close this window
            Close();
        }

        private void deleteTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO: Refactor this copy/pasted code
            bool valid = Utils.IsTagValid(deleteTextbox.Text);

            // Turn this box red if it's invalid
            deleteTextbox.Background = valid ? Brushes.White : Brushes.Red;

            // Disable the button if it's invalid
            if (!valid)
                goButton.IsEnabled = false;
        }
    }
}

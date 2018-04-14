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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace FileBrowserWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NavigationStack<string> pathHistory;

        public MainWindow()
        {
            InitializeComponent();

            pathHistory = new NavigationStack<string>(Directory.GetCurrentDirectory());
            UpdateCurrentDirectory();
        }
        
        // Misc methods

        private void UpdateCurrentDirectory()
        {
            Console.WriteLine(pathHistory.ToString());

            // Move to the directory
            Directory.SetCurrentDirectory(pathHistory.Current);
            DirectoryInfo currentDir = new DirectoryInfo(pathHistory.Current);

            // Set the textbox to the working directory
            currentPathBox.Text = pathHistory.Current;

            // Update the forward/back buttons
            forwardButton.IsEnabled = pathHistory.HasNext;
            backButton.IsEnabled = pathHistory.HasPrev;

            // Populate the list box
            folderContentsBox.Items.Clear();
            
            var subdirs = currentDir.EnumerateDirectories();
            foreach (DirectoryInfo dir in subdirs)
                folderContentsBox.Items.Add(dir.Name);

            var files = currentDir.EnumerateFiles();
            foreach (FileInfo file in files)
                folderContentsBox.Items.Add(file.Name);
        }


        // Event handlers

        private void currentPathBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;     // Don't go on if it's not the enter key

            // Navigate to the place.
            if (Directory.Exists(currentPathBox.Text))
            {
                pathHistory.Push(currentPathBox.Text);
                UpdateCurrentDirectory();
            }
        }

        private void currentPathBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Change to a red background if the text is not a valid path

            bool valid = Directory.Exists(currentPathBox.Text);
            currentPathBox.Background = valid ? Brushes.White : Brushes.Red;
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            pathHistory.MoveBack();
            UpdateCurrentDirectory();
        }

        private void forwardButton_Click(object sender, RoutedEventArgs e)
        {
            pathHistory.MoveForward();
            UpdateCurrentDirectory();
        }
    }
}

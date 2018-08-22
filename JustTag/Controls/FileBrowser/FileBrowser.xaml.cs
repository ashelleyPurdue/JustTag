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
using System.Collections;
using System.IO;
using JustTag.Tagging;

namespace JustTag.Controls.FileBrowser
{
    /// <summary>
    /// Interaction logic for FileBrowser.xaml
    /// </summary>
    public partial class FileBrowser : UserControl
    {
        // Interface
        public event SelectionChangedEventHandler SelectedFileChanged
        {
            add    => folderContentsBox.SelectionChanged += value;
            remove => folderContentsBox.SelectionChanged -= value;
        }

        public int SelectedIndex
        {
            get => folderContentsBox.SelectedIndex;
            set => folderContentsBox.SelectedIndex = value;
        }

        public FileSystemInfo SelectedItem => folderContentsBox.SelectedItem;   // TODO: Change this to a string

        public FileSystemInfo[] VisibleFiles => folderContentsBox.ItemsSource.ToArray();    // TODO: Chnage this to a string[]

        public IList SelectedItems => folderContentsBox.SelectedItems;

        // Child control accessors
        public AutoCompleteTextbox FilterTextbox => tagFilterTextbox;

        // Private fields
        private NavigationStack<string> pathHistory;


        // Constructors
        public FileBrowser()
        {
            InitializeComponent();

            // Set the filter textbox's autocomplete source to all the tags
            tagFilterTextbox.autoCompletionSource = Utils.allKnownTags;

            // Populate the sort-by combo box
            sortByBox.ItemsSource = Enum.GetValues(typeof(SortMethod));
            sortByBox.SelectedIndex = 0;

            // Start out in the current directory
            pathHistory = new NavigationStack<string>(Directory.GetCurrentDirectory());
            RefreshCurrentDirectory();
        }


        // Misc methods

        public void RefreshCurrentDirectory()
        {
            int selectedIndex = folderContentsBox.SelectedIndex;
            var selectedItem = folderContentsBox.SelectedItem;

            // Move to the directory
            Directory.SetCurrentDirectory(pathHistory.Current);
            DirectoryInfo currentDir = new DirectoryInfo(pathHistory.Current);

            // Set the textbox to the working directory
            currentPathBox.Text = pathHistory.Current;

            // Update the forward/back/up buttons
            forwardButton.IsEnabled = pathHistory.HasNext;
            backButton.IsEnabled = pathHistory.HasPrev;
            upButton.IsEnabled = Directory.GetParent(pathHistory.Current) != null;

            // Get all the files and folders that match the filter
            SortMethod sortMethod = (SortMethod)sortByBox.SelectedValue;

            var matchingFiles = TagUtils.GetMatchingFiles
            (
                pathHistory.Current,
                tagFilterTextbox.Text,
                (SortMethod)sortByBox.SelectedItem,
                (bool)descendingBox.IsChecked
            );

            // Add them all to the list view
            // TODO: Remove this hacky select statement after we migrate FileList
            folderContentsBox.ItemsSource = matchingFiles
                .Select(f => f.IsFolder ? (new DirectoryInfo(f.FullPath) as FileSystemInfo) : new FileInfo(f.FullPath));

            // Scroll back to the old selected index
            folderContentsBox.SelectedIndex = selectedIndex;
            folderContentsBox.ScrollIntoView(selectedItem);

            // Record all encountered tags in the "all known tags" list.
            foreach (TaggedFilePath file in matchingFiles)
            {
                foreach (string tag in file.Tags)
                    Utils.allKnownTags.Add(tag);
            }
        }


        // Event handlers

        private void textbox_KeyUp(object sender, KeyEventArgs e)
        {
            // Navigates to the directory in the address bar
            // when the user presses "enter"

            if (e.Key != Key.Enter) return;     // Don't go on if it's not the enter key

            // Navigate to the place.
            if (Directory.Exists(currentPathBox.Text))
            {
                pathHistory.Push(currentPathBox.Text);
                RefreshCurrentDirectory();
            }
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            // Apply the filter in the filter textbox
            RefreshCurrentDirectory();
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
            RefreshCurrentDirectory();
        }

        private void upButton_Click(object sender, RoutedEventArgs e)
        {
            // Move to the parent directory
            string parent = Directory.GetParent(pathHistory.Current).FullName;
            pathHistory.Push(parent);
            RefreshCurrentDirectory();
        }

        private void forwardButton_Click(object sender, RoutedEventArgs e)
        {
            pathHistory.MoveForward();
            RefreshCurrentDirectory();
        }

        private void folderContentsBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FileSystemInfo selectedItem = folderContentsBox.SelectedItem as FileSystemInfo;

            // If it's a shortcut, look up its target
            if (selectedItem.Extension.ToLower() == ".lnk")
                selectedItem = Utils.GetShortcutTarget(selectedItem);

            // If the selected item is a folder, move to that folder.
            DirectoryInfo dir = selectedItem as DirectoryInfo;
            if (dir != null)
            {
                pathHistory.Push(dir.FullName);
                RefreshCurrentDirectory();

                return;
            }

            // If the selected item is a file, open that file.
            FileInfo file = selectedItem as FileInfo;
            if (file != null)
            {
                System.Diagnostics.Process.Start(file.FullName);
            }
        }
    }
}

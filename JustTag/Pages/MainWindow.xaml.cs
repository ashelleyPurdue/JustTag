﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Text.RegularExpressions;
using JustTag.Controls;
using JustTag.Tagging;

namespace JustTag.Pages
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Hook the listbox up to the list of known tags
            allTagsListbox.ItemsSource   = Utils.allKnownTags;
            tagsBox.autoCompletionSource = Utils.allKnownTags;
        }

        // Event handlers

        private void fullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            TaggedFilePath[] browsableFiles = fileBrowser.VisibleFiles;
            Fullscreen fullscreen = new Fullscreen(filePreviewer, browsableFiles, fileBrowser.SelectedIndex);
            Hide();
            fullscreen.ShowDialog();
            Show();
        }

        private async void fileBrowser_SelectedFileChanged(object sender, SelectionChangedEventArgs e)
        {
            // Don't do anything if the last preview hasn't loaded yet
            if (filePreviewer.IsOpening)
                return;

            // Don't do anything if selection is null
            if (fileBrowser.SelectedItem == null)
                return;

            // If the selected item is a shortcut, resolve it.
            TaggedFilePath selectedItem = fileBrowser.SelectedItem;
            if (selectedItem.Extension.ToLower() == ".lnk")
                selectedItem = Utils.GetShortcutTarget(selectedItem);

            // Show the file preview
            await filePreviewer.OpenPreview(selectedItem);

            // Enable the tag box and update it with this file's tags
            // NOTE: This affects the shortcut itself, not its target.  This is intentional.
            StringBuilder builder = new StringBuilder();
            foreach (string t in selectedItem.Tags)
                builder.AppendLine(t);

            tagsBox.IsEnabled = true;
            tagsBox.Text = builder.ToString();

            // Hide the save button
            tagSaveButton.Visibility = Visibility.Hidden;
        }

        private void tagsBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Make the background red and disable the save button if any of the tags are invalid
            string[] tags = tagsBox.Text.Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string tag in tags)
            {
                if (!Utils.IsTagValid(tag))
                {
                    tagsBox.Background = Brushes.Red;
                    tagSaveButton.Visibility = Visibility.Hidden;

                    return;
                }
            }

            // Show the save button
            tagsBox.Background = Brushes.White;
            tagSaveButton.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Updates the selected file's tags
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void tagSaveButton_Click(object sender, RoutedEventArgs e)
        {
            TaggedFilePath selectedItem = fileBrowser.SelectedItem;

            // Parse the tags into a list
            string[] tags = tagsBox.Text.Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Rename the file
            try
            {
                await filePreviewer.ClosePreview();
                TagUtils.ChangeTagsAndSave(selectedItem, tags);
            }
            catch (IOException err)
            {
                MessageBox.Show("ERROR: Could not rename file." + err.Message);
            }

            // Refresh the file browser
            fileBrowser.RefreshCurrentDirectory();

            // Hide the save button
            tagSaveButton.Visibility = Visibility.Hidden;
        }

        private void tagsBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // HACK: Don't do anything if the save button is invisible
            if (tagSaveButton.Visibility == Visibility.Hidden)
                return;

            // Press the save button if it's shift+enter
            bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            if (e.Key == Key.Enter && shift)
            {
                e.Handled = true;
                tagSaveButton_Click(sender, null);
            }
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Open the settings window
            SettingsWindow w = new SettingsWindow();
            w.ShowDialog();
        }

        private async void findReplaceTagsButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the currently open file in case it needs to be renamed
            await filePreviewer.ClosePreview();

            // Show a window for finding/replacing
            var findReplaceWindow = new FindReplaceTagsWindow(Directory.GetCurrentDirectory(), Utils.allKnownTags);
            findReplaceWindow.ShowDialog();

            // Refresh the UI
            fileBrowser.RefreshCurrentDirectory();
        }

        private async void deleteTagButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the currently open file in case it needs to be renamed
            await filePreviewer.ClosePreview();

            // Show the window
            var toolWindow = new DeleteTagWindow(Directory.GetCurrentDirectory(), Utils.allKnownTags);
            toolWindow.ShowDialog();

            // Refresh the UI
            fileBrowser.RefreshCurrentDirectory();
        }

        private void allTagsListbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Add the selected tag to the filter, if it isn't there already
            string selectedTag = allTagsListbox.SelectedItem as string;
            string[] filterTags = fileBrowser.tagFilterTextbox.Text.Split(' ');

            if (!filterTags.Contains(selectedTag))
                fileBrowser.tagFilterTextbox.Text += " " + selectedTag;
        }

        /// <summary>
        /// Allows the user to drag a tag and drop it into the tags textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void allTagsListbox_MouseMove(object sender, MouseEventArgs e)
        {
            // Only go on if we're left clicking and the mouse is over a listbox item
            IInputElement item = allTagsListbox.InputHitTest(e.GetPosition(allTagsListbox));
            bool mouseOverItem = item is TextBlock;

            if (e.LeftButton != MouseButtonState.Pressed || !mouseOverItem)
                return;

            // Start dragging and dropping.
            DragDrop.DoDragDrop(allTagsListbox, allTagsListbox.SelectedItem, DragDropEffects.Copy);
        }

        private void tagsBox_PreviewDrop(object sender, DragEventArgs e)
        {
            // Don't let the default drag-and-drop stuff happen
            e.Handled = true;

            // Don't do anything if the dragged data is not a string
            if (!e.Data.GetDataPresent(typeof(string)))
                return;

            // Add the dropped tag to the textbox if it's not there already
            string tag = (string)e.Data.GetData(typeof(string));
            string[] existingTags = tagsBox.Text.Split(' ', '\n', '\r');

            if (existingTags.Contains(tag))
                return;

            // Add a newline if there isn't one already
            if (tagsBox.Text.Length > 0 && !tagsBox.Text.EndsWith("\n"))
                tagsBox.Text += '\n';

            tagsBox.Text += tag;
        }


    }
}

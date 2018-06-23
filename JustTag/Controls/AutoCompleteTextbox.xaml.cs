using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace JustTag
{
    /// <summary>
    /// Interaction logic for AutoCompleteTextbox.xaml
    /// </summary>
    public partial class AutoCompleteTextbox : UserControl
    {
        public event TextChangedEventHandler TextChanged
        {
            add { textbox.TextChanged += value; }
            remove { textbox.TextChanged -= value; }
        }

        public bool AcceptsReturn
        {
            get { return textbox.AcceptsReturn; }
            set { textbox.AcceptsReturn = value; }
        }

        public string Text
        {
            get { return textbox.Text; }
            set { textbox.Text = value; }
        }

        public new Brush Background
        {
            get { return textbox.Background; }
            set { textbox.Background = value; }
        }

        public new Brush Foreground
        {
            get { return textbox.Foreground; }
            set { textbox.Foreground = value; }
        }

        public IEnumerable<string> autoCompletionSource;


        public AutoCompleteTextbox()
        {
            InitializeComponent();
        }

        private string currentWord;


        // Misc methods

        private Point GetCursorPos()
        {
            // Returns the relative position of the typing cursor

            // Get all the text before the cursor so we can measure it.
            string earlierText = textbox.Text.Substring(0, textbox.CaretIndex);

            // Measure it
            FormattedText formattedText = Utils.GetFormattedText(earlierText, textbox);
            formattedText.MaxTextWidth = textbox.ActualWidth;

            return new Point(formattedText.Width, formattedText.Height);
        }

        private string GetLongestString(IEnumerable<string> strings)
        {
            // Returns the longest string in the collection
            string longest = "";

            foreach (string s in strings)
            {
                if (s.Length > longest.Length)
                    longest = s;
            }

            return longest;
        }

        private string GetWordAt(string str, int caretIndex)
        {
            // Returns the word at the given cursor position.
            // Returns null if it's not on a word (ie: in the middle of whitespace)

            // There are no words in an empty string, so pos can't be in a word
            if (str == "")
                return null;

            // Since the caret exists in *between* characters, not *on* them,
            // let's just say the caret points to the *previous* character.
            int pos = caretIndex - 1;

            // HACK: If pos is out of bounds, move it back in bounds
            // This is to accomodate situations where the typing cursor
            // is at the very start or end of the string.
            if (pos >= str.Length)
                pos = str.Length - 1;

            if (pos < 0)
                pos = 0;

            // If we're already in whitespace, we're not in a word
            if (Char.IsWhiteSpace(str[pos]))
                return null;

            // Rewind until we reach whitespace or the beginning of the string
            while (true)
            {
                if (pos == 0)
                    break;

                if (Char.IsWhiteSpace(str[pos - 1]))
                    break;

                pos--;
            }


            // Step forward until we reach the end of the word
            StringBuilder builder = new StringBuilder();
            while (pos < str.Length && !Char.IsWhiteSpace(str[pos]))
            {
                builder.Append(str[pos]);
                pos++;
            }

            return builder.ToString();
        }

        private void UpdateSuggestionBox()
        {
            // If the textbox isn't in focus, hide the box and don't go on
            if (!textbox.IsKeyboardFocused)
            {
                suggestionBox.IsOpen = false;
                return;
            }

            // Update the current word
            currentWord = GetWordAt(textbox.Text, textbox.CaretIndex);

            // If the cursor is not in a word, hide the suggestion box and don't go on
            if (currentWord == null)
            {
                suggestionBox.IsOpen = false;
                return;
            }

            // Fill the suggestion box with the words that complete it
            Regex wordRegex = new Regex("^" + Regex.Escape(currentWord) + ".+");

            var matchingWords = from word in autoCompletionSource
                                where wordRegex.IsMatch(word)
                                orderby word.Length
                                select word;

            suggestionList.ItemsSource = matchingWords;

            // Stop if the suggestion box is empty
            if (suggestionList.Items.Count == 0)
            {
                suggestionBox.IsOpen = false;
                return;
            }

            // Resize the list so it matches the width of the suggestions
            string longestStr = GetLongestString(matchingWords);

            FormattedText textSize = Utils.GetFormattedText(longestStr, suggestionList);
            suggestionList.Width = textSize.Width + 15;

            // Show the suggestion box at the cursor
            Point pos = GetCursorPos();
            suggestionBox.HorizontalOffset = pos.X + suggestionList.Width;
            suggestionBox.VerticalOffset = pos.Y;

            suggestionBox.IsOpen = true;
        }


        // Event Handlers

        private void textbox_LostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            // Hide the suggestion box when losing keyboard focus
            suggestionBox.IsOpen = false;
        }

        private void textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSuggestionBox();
        }

        private void textbox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateSuggestionBox();
        }

        private void textbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Don't do anything special if there aren't any items in the suggestion list
            if (!suggestionBox.IsOpen || suggestionList.Items.Count == 0)
                return;

            // If it's the tab key, autocomplete
            if (e.Key == Key.Tab)
            {
                e.Handled = true;

                // Get the selected word from the suggestion list
                if (suggestionList.SelectedIndex < 0)
                    suggestionList.SelectedIndex = 0;

                string insertedWord = (string)suggestionList.Items[suggestionList.SelectedIndex];

                // Chop off the part the user has already typed.
                insertedWord = insertedWord.Substring(currentWord.Length);

                // Insert the word
                textbox.BeginChange();

                int caretIndex = textbox.CaretIndex;
                textbox.Text = textbox.Text.Insert(caretIndex, insertedWord);
                textbox.CaretIndex = caretIndex + insertedWord.Length;

                textbox.EndChange();

                return;
            }

            // If it's the up or down keys, change the user's selection
            int selectedIndex = suggestionList.SelectedIndex;

            if (e.Key == Key.Up)
            {
                e.Handled = true;
                selectedIndex--;
            }

            if (e.Key == Key.Down)
            {
                e.Handled = true;
                selectedIndex++;
            }

            // Make the selected index loop around
            selectedIndex = Utils.WrapIndex(selectedIndex, suggestionList.Items.Count);

            // Apply the selection change
            suggestionList.SelectedIndex = selectedIndex;
        }

        private void suggestionBox_Opened(object sender, EventArgs e)
        {
            // Reset the selected index to zero.  This way the selected index
            // will always be where the user expects it when the box opens.
            suggestionList.SelectedIndex = 0;
        }
    }
}

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

        public IEnumerable<string> autoCompletionSource;


        public AutoCompleteTextbox()
        {
            InitializeComponent();
        }


        // Misc methods

        private Thickness GetCursorPos()
        {
            // Returns the relative position of the typing cursor

            // Get all the text before the cursor so we can measure it.
            string earlierText = textbox.Text.Substring(0, textbox.CaretIndex);

            // Measure it
            FormattedText formattedText = Utils.GetFormattedText(earlierText, textbox);
            formattedText.MaxTextWidth = textbox.ActualWidth;

            double width = formattedText.Width;
            double height = formattedText.Height;

            return new Thickness(formattedText.Width, formattedText.Height, 0, 0);
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
            // Get the current word
            string currentWord = GetWordAt(textbox.Text, textbox.CaretIndex);

            // If the cursor is not in a word or the textbox is not in focus, hide the suggestion box and don't go on
            if (currentWord == null || !textbox.IsKeyboardFocused)
            {
                suggestionBox.Visibility = Visibility.Collapsed;
                return;
            }

            // Show the suggestion box
            suggestionBox.Visibility = Visibility.Visible;

            // Fill the suggestion box with the words that match it.
            Regex wordRegex = new Regex("^" + currentWord + ".*");

            var matchingWords = from word in autoCompletionSource
                                where wordRegex.IsMatch(word)
                                select word;

            suggestionBox.ItemsSource = matchingWords;
            suggestionBox.Margin = GetCursorPos();       // Move the menu to the cursor pos

            // Resize the dropdown list so it matches the width of the suggestions
            string longestStr = GetLongestString(matchingWords);

            FormattedText textSize = Utils.GetFormattedText(longestStr, suggestionBox);
            suggestionBox.Width = textSize.Width + 15;
        }


        // Event Handlers

        private void textbox_LostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            // Hide the suggestion box when losing keyboard focus
            suggestionBox.Visibility = Visibility.Collapsed;
        }

        private void textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSuggestionBox();
        }

        private void textbox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateSuggestionBox();
        }
    }
}

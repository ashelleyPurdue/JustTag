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

        private string currentWord;

        public AutoCompleteTextbox()
        {
            InitializeComponent();
        }

        private void textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Set the current word to the last word in the textbox
            // TODO: use the cursor position to find the current word
            string[] words = textbox.Text.Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            currentWord = words[words.Length - 1];

            // Fill the combo box with the words that match it.
            // TODO: Use a regex to filter this
            autoCompleteDropdown.ItemsSource = autoCompletionSource;
        }
    }
}

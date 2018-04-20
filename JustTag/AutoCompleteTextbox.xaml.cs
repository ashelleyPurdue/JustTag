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

        public IEnumerable<string> autoCompletionSource;

        public AutoCompleteTextbox()
        {
            InitializeComponent();
        }
    }
}

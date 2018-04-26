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

namespace JustTag
{
    /// <summary>
    /// Interaction logic for FindReplaceTagsWindow.xaml
    /// </summary>
    public partial class FindReplaceTagsWindow : Window
    {
        public FindReplaceTagsWindow(string directory, IEnumerable<string> autoCompleteTags)
        {
            InitializeComponent();

            // Set the autocomplete sources
            findTextbox.autoCompletionSource = autoCompleteTags;
            replaceTextbox.autoCompletionSource = autoCompleteTags;
        }
    }
}

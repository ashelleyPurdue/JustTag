using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using JustTag.Tagging;

namespace JustTag.Controls.PreviewerControls
{
    public interface IPreviewerControl
    {
        Visibility Visibility { get; set; }     // If an IPreviewerControl also happens to inherit from UserControl,
                                                // then it will already have this property.

        /// <summary>
        /// Returns true if this previewer should be used to open this file
        /// False otherwise
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        bool CanOpen(TaggedFilePath file);

        Task OpenPreview(TaggedFilePath file);
        Task ClosePreview();
    }
}

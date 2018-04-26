using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace JustTag
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App():base()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            Unosquare.FFME.MediaElement.FFmpegDirectory = System.IO.Path.Combine(baseDir, "ffmpeg\\");
        }
    }
}

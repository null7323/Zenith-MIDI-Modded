using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
using ZenithEngine;

namespace Zenith_MIDI
{
    /// <summary>
    /// PluginLoadingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DependenciesLoadingWindow : Window
    {
        public DependenciesLoadingWindow()
        {
            InitializeComponent();
            // this.Close();
        }
    }
}

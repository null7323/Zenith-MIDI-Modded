using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ZenithShared;

namespace Zenith_MIDI
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
#if !DEBUG
            try
            {
#endif
                Console.Title = "Zenith 2.0.6 (Modified Version 3)";
                Console.WriteLine("Zenith 2.0.6 | Modified by TBL & qishipai");
                // just tips
                Console.WriteLine("警告: 此版本可能不稳定");
                Console.WriteLine("如果您发现了任何Bug，请及时报告");
                Console.WriteLine("--------------------------------------------");
                Application app = new Application();
                app.Run(new MainWindow());
#if !DEBUG
            }
            catch (Exception e)
            {
                string msg = e.Message + "\n" + e.Data + "\n";
                msg += e.StackTrace;
                MessageBox.Show(msg, "Zenith has crashed!");
            }
#endif
        }
    }
}

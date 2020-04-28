using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
// using ZenithShared;

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
                Console.Title = "Zenith 2.0.6 (Modded Version 5.2)";
                Console.WriteLine("Zenith 2.0.6 | Modded by TBL, qishipai, FruityTeaTIPB5");
                // just tips
                Console.WriteLine("Warning: This modded version is not so stable.");
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

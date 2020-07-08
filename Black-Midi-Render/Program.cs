using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZenithShared;
using ZenithEngine;

namespace Zenith_MIDI
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //#if !DEBUG
            //            string MachineName = Environment.MachineName;
            //            string GetSHA512Of(string SourceString)
            //            {
            //                byte[] StrBuffer = Encoding.UTF8.GetBytes(SourceString);
            //                var Provider = new SHA512CryptoServiceProvider();
            //                byte[] ReturnBuffer = Provider.ComputeHash(StrBuffer);
            //                return BitConverter.ToString(ReturnBuffer).Replace("-", string.Empty);
            //            }
            //            // Check whether the key exists
            //            if (!File.Exists("key.zmk"))
            //            {
            //                MessageBox.Show("Zenith Mod has refused to start.\nPlease check whether 'key.zmk' is in the same folder.", "Key not found");
            //                return;
            //            }
            //            else
            //            {
            //                StreamReader Reader = new StreamReader("key.zmk");
            //                string inKey = Reader.ReadToEnd().Replace("\n", string.Empty);
            //                if (inKey == GetSHA512Of(MachineName))
            //                {
            //                    // Console.WriteLine("Correct key! Zenith Mod will continue starting...");
            //                }
            //                else
            //                {
            //                    MessageBox.Show("Zenith Mod has refused to start.\nThe key is incorrect. You need to re-generate the key.", "Invalid key");
            //                    return;
            //                }
            //            }
            //#endif
#if !DEBUG
            try
            {
#endif
#if !DEBUG
                Console.Title = "Zenith Modded (6.1.7)";
#else
                Console.Title = "Zenith Modded (Debug)";
#endif
                System.Windows.Application app = new System.Windows.Application();
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

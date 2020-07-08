using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zenith_MIDI
{
    class Config
    {
        public struct GeneralConfiguration
        {
            public int Width;
            public int Height;
            public int SSAA;
            public int PreviewWidth;
            public int PreviewHeight;
            public int FPS;
            public double Multiplier;
            public int SkippingTicks;
            public bool EnableVSync;
            public bool IsPaused;
            public bool IsRealtimePlayback;
            public bool UseBackground;
            public string BackgroundPath;
            public bool IgnoreColorEvents;
            public string Module;
            public string Name;
        }

        public const string DefaultConfig = "Name=Default Config\nIsConfig=1\nWidth=1920\nHeight=1080\nPreviewWidth=1920\nPreviewHeight=1080\nSSAA=1\nFPS=60\nMultiplier=1\nSkippingTicks=5000\nEnableVSync=0\nIsPaused=0\nIsRealtimePlayback=1" +
            "UseBackground=0\nBackgroundPath=\nIgnoreColorEvents=0\nModule=Classic";

        public static void LoadConfigurations()
        {
            string FileName = "config/config";
            string extensionName = ".zmconfig";
            string currFileName;
            for (int fileIndex = 1; fileIndex < 9; ++fileIndex)
            {
                currFileName = FileName + fileIndex + extensionName;
                if (!File.Exists(currFileName))
                {
                    StreamWriter writer = new StreamWriter(currFileName, false);
                    writer.Write(DefaultConfig);
                }
                StreamReader reader = new StreamReader(currFileName);
                string[] lines = reader.ReadToEnd().Split('\n');
                GeneralConfiguration Config = new GeneralConfiguration();
                foreach (var each in lines)
                {
                    ParseLine(fileIndex, ref Config, each);
                }
            }
        }
        public static bool ParseLine(int index, ref GeneralConfiguration config, string text)
        {
            int strlen = text.Length;
            if (text.Contains("IsConfig=1"))
            {
                return true;
            }
            else if (text.Contains("Width="))
            {
                int width;
                bool parsed = int.TryParse(text.Substring(6), out width);
                if (parsed)
                {
                    config.Width = width;
                    return true;
                }
                return false;
            }
            else if (text.Contains("Height="))
            {
                int height;
                bool parsed = int.TryParse(text.Substring(7), out height);
                if (parsed)
                {
                    config.Height = height;
                    return true;
                }
                return false;
            }
            else if (text.Contains("PreviewWidth="))
            {
                int previewWidth;
                bool parsed = int.TryParse(text.Substring(13), out previewWidth);
                if (parsed)
                {
                    config.PreviewWidth = previewWidth;
                    return true;
                }
                return false;
            }
            else if (text.Contains("PreviewHeight="))
            {
                int previewHeight;
                bool parsed = int.TryParse(text.Substring(13), out previewHeight);
                if (parsed)
                {
                    config.PreviewHeight = previewHeight;
                    return true;
                }
                return false;
            }
            else if (text.Contains("Name="))
            {
                config.Name = text.Substring(5);
                return true;
            }
            else if (text.Contains("SSAA="))
            {
                if (!int.TryParse(text.Substring(5), out config.SSAA))
                {
                    return false;
                }
                return true;
            }
            else if (text.Contains("IgnoreColorEvents="))
            {
                int IgnoreColorEventsOrNot;
                if (!int.TryParse(text.Substring(18), out IgnoreColorEventsOrNot))
                {
                    return false;
                }
                if (IgnoreColorEventsOrNot == 1)
                {
                    config.IgnoreColorEvents = true;
                }
                else
                {
                    config.IgnoreColorEvents = false;
                }
                return true;
            }
            else if (text.Contains("SkippingTicks="))
            {
                int skip;
                if (!int.TryParse(text.Substring(13), out skip))
                {
                    return false;
                }
                else
                {
                    config.SkippingTicks = skip;
                }
                return true;
            }
            else if (text.Contains("IsPaused="))
            {
                int isPaused;
                if (!int.TryParse(text.Substring("IsPaused=".Length), out isPaused))
                {
                    return false;
                }
                if (isPaused == 1)
                {
                    config.IsPaused = true;
                }
                else
                {
                    config.IsPaused = false;
                }
                return true;
            }
            else
            {
                return false;
            }
            
        }
    }
}

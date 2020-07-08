using ZenithEngine;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Path = System.IO.Path;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using ZenithShared;
using System.Management;
using ClassicRender;

namespace Zenith_MIDI
{

    struct CurrentRendererPointer
    {
        public Queue<IPluginRender> disposeQueue/* = new Queue<IPluginRender>()*/;
        public IPluginRender renderer/* = null*/;
    }

    public enum UpdateProgress
    {
        NotDownloading,
        Downloading,
        Downloaded
    }

    public partial class MainWindow : Window
    {
        #region Chrome Window scary code
        //private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        //{
        //    switch (msg)
        //    {
        //        case 0x0024:
        //            WmGetMinMaxInfo(hwnd, lParam);
        //            handled = true;
        //            break;
        //    }
        //    return (IntPtr)0;
        //}

        //private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        //{
        //    MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
        //    int MONITOR_DEFAULTTONEAREST = 0x00000002;
        //    IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
        //    if (monitor != IntPtr.Zero)
        //    {
        //        MONITORINFO monitorInfo = new MONITORINFO();
        //        GetMonitorInfo(monitor, monitorInfo);
        //        RECT rcWorkArea = monitorInfo.rcWork;
        //        RECT rcMonitorArea = monitorInfo.rcMonitor;
        //        mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
        //        mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
        //        mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
        //        mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
        //    }
        //    Marshal.StructureToPtr(mmi, lParam, true);
        //}

        //[StructLayout(LayoutKind.Sequential)]
        //public struct POINT
        //{
        //    /// <summary>x coordinate of point.</summary>
        //    public int x;
        //    /// <summary>y coordinate of point.</summary>
        //    public int y;
        //    /// <summary>Construct a point of coordinates (x,y).</summary>
        //    public POINT(int x, int y)
        //    {
        //        this.x = x;
        //        this.y = y;
        //    }
        //}

        //[StructLayout(LayoutKind.Sequential)]
        //public struct MINMAXINFO
        //{
        //    public POINT ptReserved;
        //    public POINT ptMaxSize;
        //    public POINT ptMaxPosition;
        //    public POINT ptMinTrackSize;
        //    public POINT ptMaxTrackSize;
        //};

        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        //public class MONITORINFO
        //{
        //    public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
        //    public RECT rcMonitor = new RECT();
        //    public RECT rcWork = new RECT();
        //    public int dwFlags = 0;
        //}

        //[StructLayout(LayoutKind.Sequential, Pack = 0)]
        //public struct RECT
        //{
        //    public int left;
        //    public int top;
        //    public int right;
        //    public int bottom;
        //    public static readonly RECT Empty = new RECT();
        //    public int Width { get { return Math.Abs(right - left); } }
        //    public int Height { get { return bottom - top; } }
        //    public RECT(int left, int top, int right, int bottom)
        //    {
        //        this.left = left;
        //        this.top = top;
        //        this.right = right;
        //        this.bottom = bottom;
        //    }
        //    public RECT(RECT rcSrc)
        //    {
        //        left = rcSrc.left;
        //        top = rcSrc.top;
        //        right = rcSrc.right;
        //        bottom = rcSrc.bottom;
        //    }
        //    public bool IsEmpty { get { return left >= right || top >= bottom; } }
        //    public override string ToString()
        //    {
        //        if (this == Empty) { return "RECT {Empty}"; }
        //        return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
        //    }
        //    public override bool Equals(object obj)
        //    {
        //        if (!(obj is Rect)) { return false; }
        //        return (this == (RECT)obj);
        //    }
        //    /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
        //    public override int GetHashCode() => left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
        //    /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
        //    public static bool operator ==(RECT rect1, RECT rect2) { return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom); }
        //    /// <summary> Determine if 2 RECT are different(deep compare)</summary>
        //    public static bool operator !=(RECT rect1, RECT rect2) { return !(rect1 == rect2); }
        //}

        //[DllImport("user32")]
        //internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        //[DllImport("User32")]
        //internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);
        #endregion

        public RenderSettings settings;
        // public RenderSettings GetSettings { get { return settings; } }
        MidiFile midifile = null;
        string midipath = "";

        Control pluginControl = null;

        List<IPluginRender> RenderPlugins = new List<IPluginRender>();

        CurrentRendererPointer renderer = new CurrentRendererPointer();

        List<Dictionary<string, ResourceDictionary>> Languages = new List<Dictionary<string, ResourceDictionary>>();

        bool foundOmniMIDI = true;
        bool OmniMIDIDisabled = false;

        string defaultPlugin = "Classic";

        Settings metaSettings = new Settings();

        List<Config.GeneralConfiguration> Configs = new List<Config.GeneralConfiguration>();

        List<Label> ConfigLabel = new List<Label>();

        DependenciesLoadingWindow loadDependenciesWindow = new DependenciesLoadingWindow();

        const string Console_Title = "Zenith Modded (6.1.7)";
        const string GC_Title = "Zenith Modded (6.1.7) (Collecting Unused Memory)";

        void RunLanguageCheck()
        {
        }

        void RunUpdateCheck()
        {
            if (!metaSettings.AutoUpdate) return;


            string ver;
            try
            {
                ver = ZenithUpdates.GetLatestVersion();
            }
            catch { return; }
        }

        void CheckUpdateDownloaded()
        {
            if (metaSettings.PreviousVersion != metaSettings.VersionName)
            {
                if (File.Exists("settings.json")) File.Delete("settings.json");
                metaSettings.PreviousVersion = metaSettings.VersionName;
                metaSettings.SaveConfig();
            }

            //if (metaSettings.AutoUpdate)
            //{
            //    if (File.Exists(ZenithUpdates.DefaultUpdatePackagePath))
            //    {
            //        try
            //        {
            //            using (var z = File.OpenRead(ZenithUpdates.DefaultUpdatePackagePath))
            //            using (ZipArchive archive = new ZipArchive(z))
            //            { }
            //            Dispatcher.InvokeAsync(() => windowTabs.UpdaterProgress = UpdateProgress.Downloaded).Wait();
            //            if (!ZenithUpdates.IsAnotherProcessRunning())
            //            {
            //                Process.Start(ZenithUpdates.InstallerPath, "update -Reopen");
            //            }
            //        }
            //        catch (Exception) { File.Delete(ZenithUpdates.DefaultUpdatePackagePath); }
            //    }
            //}
        }

        public long GetMemory()
        {
            ManagementClass wmiClass = new ManagementClass("Win32_PhysicalMemory");
            ManagementObjectCollection collection = wmiClass.GetInstances();
            long MemoryCapacity = 0;
            foreach (var mo in collection)
            {
                MemoryCapacity += (long)Math.Round((decimal)(long.Parse(mo.Properties["Capacity"].Value.ToString()) / 1024 / 1024), 0);
            }
            return MemoryCapacity;
        }

        public MainWindow()
        {
            // CheckUpdateDownloaded();

            InitializeComponent();

            loadDependenciesWindow.Show();
            Task omnimidiLoader = null;
            if (foundOmniMIDI)
            {
                //omnimidiLoader = Task.Run(() =>
                //{
                if (GetMemory() > 4096)
                {
                    try
                    {
                        KDMAPI.InitializeKDMAPIStream();
                        Console.WriteLine("Loaded KDMAPI!");
                    }
                    catch
                    {
                        Console.WriteLine("Failed to load KDMAPI, disabling");
                        foundOmniMIDI = false;
                    }
                    //});
                }
                else
                {
                    OmniMIDIDisabled = true;
                    disableKDMAPI.Content = Resources["enableKDMAPI"];
                }
            }
            if (!foundOmniMIDI)
            {
                disableKDMAPI.IsEnabled = false;
            }

            // windowTabs.VersionName = metaSettings.VersionName;
            windowTabs.VersionName = "Mod 6.1.7";

            tempoMultSlider.nudToSlider = v => Math.Log(v, 2);
            tempoMultSlider.sliderToNud = v => Math.Pow(2, v);

            bool dontUpdateLanguages = true;

            if (!File.Exists("Settings/settings.json"))
            {
                var sett = new JObject
                {
                    { "defaultBackground", "" },
                    { "ignoreKDMAPI", "false" },
                    { "defaultPlugin", "Classic" },
                    { "ignoreLanguageUpdates", "false" }
                };
                File.WriteAllText("Settings/settings.json", JsonConvert.SerializeObject(sett));
            }

            {
                dynamic sett = JsonConvert.DeserializeObject(File.ReadAllText("Settings/settings.json"));
                if (sett.defaultBackground != "")
                {
                    try
                    {
                        bgImagePath.Text = sett.defaultBackground;
                        settings.BGImage = bgImagePath.Text;
                    }
                    catch
                    {
                        settings.BGImage = null;
                        if (bgImagePath.Text != "")
                            MessageBox.Show("Couldn't load default background image");
                    }
                }
                if ((bool)sett.ignoreKDMAPI) foundOmniMIDI = false;
                defaultPlugin = (string)sett.defaultPlugin;
                dontUpdateLanguages = (bool)sett.ignoreLanguageUpdates;
            }


            Task languageLoader = null;
            settings = new RenderSettings();
            settings.PauseToggled += ToggledPause;
            InitialiseSettingsValues();
            //creditText.Text = "Video was rendered with Zenith\nhttps://arduano.github.io/Zenith-MIDI/start";
            // if today is not my birthday, show this:
            if (DateTime.Now.Month == 1 && DateTime.Now.Day == 8)
            {
                int age = DateTime.Now.Year - 2006;
                creditText.Text = "Today is NullptrBlacker's Birthday!\nHe turned " + age + " years old!";
                
            }
            else
            {
                // creditText.Text = "Video was rendered with Zenith Modded\nhttps://github.com/noob601/Zenith-MIDI-Modded";
                creditText.Text = "Video was rendered with Zenith Modded\nhttps://github.com/noob601/Zenith-MIDI-Modded";
            }

            if (languageLoader != null) languageLoader.Wait();

            var languagePacks = Directory.GetDirectories("Languages");
            foreach (var language in languagePacks)
            {
                var resources = Directory.GetFiles(language).Where((l) => l.EndsWith(".xaml")).ToList();
                if (resources.Count == 0) continue;

                Dictionary<string, ResourceDictionary> fullDict = new Dictionary<string, ResourceDictionary>();
                foreach (var r in resources)
                {
                    ResourceDictionary file = new ResourceDictionary
                    {
                        Source = new Uri(Path.GetFullPath(r), UriKind.RelativeOrAbsolute)
                    };
                    var name = Path.GetFileNameWithoutExtension(r);
                    fullDict.Add(name, file);
                }
                if (!fullDict.ContainsKey("window")) continue;
                if (fullDict["window"].Contains("LanguageName") && fullDict["window"]["LanguageName"].GetType() == typeof(string))
                    Languages.Add(fullDict);
            }
            Languages.Sort(new Comparison<Dictionary<string, ResourceDictionary>>((d1, d2) =>
            {
                if ((string)d1["window"]["LanguageName"] == "English") return -1;
                if ((string)d2["window"]["LanguageName"] == "English") return 1;
                else return 0;
            }));
            foreach (var lang in Languages)
            {
                var item = new ComboBoxItem() { Content = lang["window"]["LanguageName"] };
                languageSelect.Items.Add(item);
            }
            languageSelect.SelectedIndex = 0;
            if (omnimidiLoader != null)
                omnimidiLoader.Wait();
        }

        void ToggledPause()
        {
            Dispatcher.Invoke(() =>
            {
                if (previewPaused.IsChecked ^ settings.Paused)
                {
                    previewPaused.IsChecked = settings.Paused;
                }
            });

        }

        bool SetStatus(int statusID)
        {
            try
            {
                switch (statusID)
                {
                    case 0:
                        // is rendering
                        Dispatcher.Invoke(() =>
                        {
                            currStatus.Content = Resources["isRendering"];
                        });
                        break;
                    case 1:
                        // finished render
                        Dispatcher.Invoke(() =>
                        {
                            currStatus.Content = Resources["finishedRender"];
                        });
                        break;
                    case 2:
                        Dispatcher.Invoke(() =>
                        {
                            currStatus.Content = Resources["collectingUnusedMemory"];
                        });
                        break;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        void InitialiseSettingsValues()
        {
            viewWidth.Value = settings.width;
            viewHeight.Value = settings.height;
            viewFps.Value = settings.fps;
            vsyncEnabled.IsChecked = settings.vsync;
            tempoMultSlider.Value = settings.tempoMultiplier;

            enableMaxMemory.IsChecked = settings.enableMemoryLimit;
            autoDisableKDMAPI.IsChecked = settings.autoDisableKDMAPIWhenRendering;

            ReloadPlugins();
            // reset threads for rendering
            filterThreadsForRender.Value = settings.filterThreadsForRender;
            Console.WriteLine("Found " + filterThreadsForRender.Value + " logic processors");
            // set max memory
            settings.maxRenderRAM = GetMemory() / 2;
            maxRenderMemory.Maximum = GetMemory();
            maxRenderMemory.TrueMax = GetMemory();
            maxRenderMemory.Value = settings.maxRenderRAM;
            Console.WriteLine("Max render memory has been set to: " + maxRenderMemory.Value + " MBytes");
            // set width and height
            try
            {
                previewWidthSelect.Value = settings.preview_width = OpenTK.DisplayDevice.Default.Width / 3 * 2;
                previewHeightSelect.Value = settings.preview_height = OpenTK.DisplayDevice.Default.Width * 3 / 8;
            }
            catch
            {

            }
            // set memory saving mode
            enableMemorySaving.IsChecked = settings.enableMemorySavingMode;

            // set priority
            var CurrProcess = Process.GetCurrentProcess();
            CurrProcess.PriorityClass = ProcessPriorityClass.High;
            Console.WriteLine("Current process priority has been set to: Hightest.");
            // whether using full arg or not
            fullFFArg.IsChecked = settings.useFullArguments;
            settings.fullArgs = fullFFmpegArg.Text;

            collectMemoryWithCondition.IsChecked = settings.collectMemoryByNotes;
            collectNotesDrawnCondition.Value = settings.collectLimit;

            autoAdjustMemoryCollection.IsChecked = settings.autoAdjustCollection;
            doNotWrite.IsChecked = settings.disableInfoWriting;
            useMultithreadToLoadMidi.IsChecked = settings.useMultithreadToLoadMidi;
            loadDependenciesWindow.Close();
        }

        Task renderThread = null;
        RenderWindow win = null;
        void RunRenderWindow()
        {
            // Thread.CurrentThread.Priority = ThreadPriority.Highest;
            if (settings.autoDisableKDMAPIWhenRendering && settings.ffRender && foundOmniMIDI)
            {
                Console.WriteLine("Disabling KDMAPI...");
                KDMAPI.TerminateKDMAPIStream();
                Console.WriteLine("Disabled!");
            }
            bool winStarted = false;
            Task winthread = new Task(() =>
            {
                win = new RenderWindow(ref renderer, midifile, settings);
                winStarted = true;
                win.Run();
            }, TaskCreationOptions.LongRunning);
            SetStatus(0);
            winthread.Start();
            SpinWait.SpinUntil(() => winStarted);
            // waste var
            // double time = 0;
            // end
            long nc = -1;
            long maxRam = 0;
            long avgRam = 0;
            long ramSample = 0;
            long renderedNotes = 0;
            double currSpeed = 0D;
            double averageRenderSpeed = 0D;
            long speedSample = 0;
            Stopwatch timewatch = new Stopwatch();
            timewatch.Start();
            IPluginRender render = null;
            double lastWinTime = double.NaN;
            int fewNotesTime = 0;
            int denseNotesFrames = 0;
            int extremelyDenseFrames = 0;
            try
            {
                double cutoffTime;
                bool manualDelete;
                double noteCollectorOffset;
                bool receivedInfo = false;
                while (
                    (midifile.ParseUpTo(
                        (long)(win.midiTime + win.lastDeltaTimeOnScreen +  
                        (win.tempoFrameStep * 20 * settings.tempoMultiplier * (win.lastMV > 1 ? win.lastMV : 1))
                        ))
                        || nc != 0) && settings.running)
                {
                    // SpinWait.SpinUntil(() => lastWinTime != win.midiTime || render != renderer.renderer || !settings.running);
                    if (!settings.running) break;
                    Note n;
                    //double cutoffTime = win.midiTime;
                    //bool manualDelete = false;
                    //double noteCollectorOffset = 0;
                    //bool receivedInfo = false;
                    while (!receivedInfo)
                    {
                        try
                        {
                            render = renderer.renderer;
                            receivedInfo = true;
                        }
                        catch
                        { }
                    }
                    //cutoffTime = (long)win.midiTime;
                    //manualDelete = render.ManualNoteDelete;
                    //noteCollectorOffset = render.NoteCollectorOffset;
                    //cutoffTime += noteCollectorOffset;
                    if (!settings.running) break;
                    lock (midifile.globalDisplayNotes)
                    {
                        var i = midifile.globalDisplayNotes.Iterate();
                        if (render.ManualNoteDelete)
                            while (i.MoveNext(out n))
                            {
                                if (n.delete)
                                    i.Remove();
                                else
                                {
                                    ++nc;
                                    ++renderedNotes;
                                }
                            }
                        else
                            while (i.MoveNext(out n))
                            {
                                if (n.end < win.midiTime + render.NoteCollectorOffset)
                                {
                                    if (n.hasEnded)
                                    {
                                        i.Remove();
                                    }
                                }
                                if (n.start > win.midiTime + render.NoteCollectorOffset) break;
                            }
                    }
                    if (settings.enableMemorySavingMode) GC.Collect();
                    else
                    {
                        if (settings.autoAdjustCollection)
                        {
                            if (renderer.renderer.LastNoteCount < 5000)
                            {
                                ++fewNotesTime;
                                if (fewNotesTime > 1000)
                                {
                                    Console.Title = GC_Title;
                                    SetStatus(2);
                                    GC.Collect();
                                    fewNotesTime = 0;
                                    Console.Title = Console_Title;
                                }
                            }
                            else if (Process.GetCurrentProcess().PrivateMemorySize64 > settings.maxRenderRAM * 1048576 &&
                                renderer.renderer.LastNoteCount < 1500000)
                            {
                                ++denseNotesFrames;
                                if (denseNotesFrames > 500)
                                {
                                    Console.Title = GC_Title;
                                    SetStatus(2);
                                    GC.Collect();
                                    Console.Title = Console_Title;
                                    denseNotesFrames = 0;
                                }
                            }
                            else if (Process.GetCurrentProcess().PrivateMemorySize64 > settings.maxRenderRAM * 1048576 &&
                                renderer.renderer.LastNoteCount >= 1500000)
                            {
                                ++extremelyDenseFrames;
                                if (extremelyDenseFrames > 15)
                                {
                                    Console.Title = GC_Title;
                                    SetStatus(2);
                                    GC.Collect();
                                    Console.Title = Console_Title;
                                    extremelyDenseFrames = 0;
                                }
                            }
                        }
                        else
                        {
                            if (settings.enableMemoryLimit && Process.GetCurrentProcess().PrivateMemorySize64 > settings.maxRenderRAM * 1048576)
                            {
                                Console.Title = GC_Title;
                                SetStatus(2);
                                GC.Collect();
                                Console.Title = Title;
                            }
                            else if (settings.collectMemoryByNotes && renderer.renderer.LastNoteCount > settings.collectLimit)
                            {
                                Console.Title = GC_Title;
                                SetStatus(2);
                                GC.Collect();
                                Console.Title = Title;
                            }
                        }
                    }
                    SetStatus(0);
                    double progress = 0D;
                    int FPS = (int)Math.Round(settings.liveFps);
                    
                    string renderSpeedStr = (Math.Round(settings.liveFps / settings.fps, 3)).ToString();
                    if (settings.timeBasedNotes) progress = win.midiTime / 1000 / midifile.info.secondsLength;
                    else progress = win.midiTime / midifile.maxTrackTime;
                    if (!settings.disableInfoWriting)
                    {
                        if (!settings.running) break;
                        try
                        {
                            Console.WriteLine(
                                Math.Round(progress * 10000) / 100 +
                                "\tNotes drawn: " + renderer.renderer.LastNoteCount +
                                "       Render FPS: " + FPS + " (" +
                                renderSpeedStr + "x)"
                                );
                        }
                        catch
                        {
                        }
                    }
                    long ram = Process.GetCurrentProcess().PrivateMemorySize64;
                    currSpeed = Math.Round(settings.liveFps / settings.fps, 3);
                    averageRenderSpeed = Math.Round((averageRenderSpeed * speedSample + currSpeed) / (++speedSample), 3);
                    if (maxRam < ram) maxRam = ram;
                    // ramSample++;
                    avgRam = (long)((double)avgRam * ramSample + ram) / (++ramSample);
                    lastWinTime = win.midiTime;
                    Dispatcher.Invoke(() =>
                    {
                        renderProgress.Content = (Math.Round(progress * 10000) / 100).ToString() + "%";
                        renderFPS.Content = FPS.ToString();
                        renderSpeed.Content = renderSpeedStr + "x";
                        renderedFrame.Content = settings.renderedFrames;
                        averageMemory.Content = (avgRam / 1024 / 1024).ToString();
                        currentMemory.Content = (ram / 1024 / 1024).ToString();
                        maximumMemory.Content = (maxRam / 1024 / 1024).ToString();
                        notesDrawn.Content = renderer.renderer.LastNoteCount;
                        renderTime.Content = (timewatch.Elapsed.Hours.ToString()) + "h " + timewatch.Elapsed.Minutes.ToString() + "m " + timewatch.Elapsed.Seconds.ToString() + "s";
                        avgSpeed.Content = averageRenderSpeed + "x";
                    });
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    SpinWait.SpinUntil(() =>
                    (
                        (s.ElapsedMilliseconds > 1000.0 / settings.fps * 30 && false) ||
                        (win.midiTime + win.lastDeltaTimeOnScreen + 
                        (win.tempoFrameStep * 10 * settings.tempoMultiplier * (win.lastMV > 1 ? win.lastMV : 1))) > midifile.currentSyncTime ||
                        lastWinTime != win.midiTime || render != renderer.renderer || !settings.running
                    )
                    );
                    // GC.Collect();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while opeining render window. Please try again.\n\n" + ex.Message + "\n" + ex.StackTrace);
                settings.running = false;
            }
            winthread.GetAwaiter().GetResult();
            settings.running = false;
            SetStatus(1);
            Console.WriteLine("Reset midi file");
            midifile.Reset();
            win.Dispose();
            win = null;
            GC.Collect();
            GC.WaitForFullGCComplete();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(
                    "Finished render\nRAM usage (Private bytes)\nPeak: " + Math.Round((double)maxRam / 1000 / 1000 / 1000 * 100) / 100 +
                    "GB\nAvg: " + Math.Round((double)avgRam / 1000 / 1000 / 1000 * 100) / 100 +
                    "GB\nMinutes to render: " + Math.Round((double)timewatch.ElapsedMilliseconds / 600) / 100);
            Console.ResetColor();
            SetStatus(1);
            if (settings.ffRender && settings.autoDisableKDMAPIWhenRendering)
            {
                Task.Factory.StartNew(() =>
                {
                    Console.WriteLine("Reloading KDMAPI...");
                    KDMAPI.InitializeKDMAPIStream();
                    Console.WriteLine("Loaded!");
                });
            }
            Dispatcher.Invoke(() =>
            {
                Resources["notRendering"] = true;
                Resources["notPreviewing"] = true;
            });
        }

        public void ReloadPlugins()
        {
            previewImage.Source = null;
            pluginDescription.Text = "";
            if (renderer.disposeQueue == null) renderer.disposeQueue = new Queue<IPluginRender>();
            lock (renderer.disposeQueue)
            {
                foreach (var p in RenderPlugins)
                {
                    if (p.Initialized) renderer.disposeQueue.Enqueue(p);
                }
                RenderPlugins.Clear();
                var files = Directory.GetFiles("Plugins");
                var dlls = files.Where((s) => s.EndsWith(".dll"));
                foreach (var d in dlls)
                {
                    try
                    {
                        var DLL = Assembly.UnsafeLoadFrom(Path.GetFullPath(d));
                        bool hasClass = false;
                        var name = Path.GetFileName(d);
                        try
                        {
                            foreach (Type type in DLL.GetExportedTypes())
                            {
                                if (type.Name == "Render")
                                {
                                    hasClass = true;
                                    var instance = (IPluginRender)Activator.CreateInstance(type, new object[] { settings });
                                    RenderPlugins.Add(instance);
                                    Console.WriteLine("Loaded " + name);
                                    loadDependenciesWindow.Dispatcher.Invoke(() =>
                                    {
                                        loadDependenciesWindow.loading.Content = "Loaded " + name;
                                    });

                                }
                            }
                            if (!hasClass)
                            {
                                MessageBox.Show("Could not load " + name + "\nDoesn't have render class");
                            }
                        }
                        catch (RuntimeBinderException)
                        {
                            MessageBox.Show("Could not load " + name + "\nA binding error occured");
                        }
                        catch (InvalidCastException)
                        {
                            MessageBox.Show("Could not load " + name + "\nThe Render class was not a compatible with the interface");
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("An error occured while binfing " + name + "\n" + e.Message);
                        }
                    }
                    catch { }
                }

                pluginsList.Items.Clear();
                for (int i = 0, RenderPluginsCount = RenderPlugins.Count; i < RenderPluginsCount; ++i)
                {
                    pluginsList.Items.Add(new ListBoxItem() { Content = RenderPlugins[i].Name });
                }
                if (RenderPlugins.Count != 0)
                {
                    SelectRenderer(0);
                }
            }
        }

        void SelectRenderer(int id)
        {
            pluginsSettings.Children.Clear();
            pluginControl = null;
            if (id == -1)
            {
                renderer.renderer = null;
                return;
            }
            pluginsList.SelectedIndex = id;
            if (renderer.renderer == null)
            {
                renderer.renderer = RenderPlugins[id];
            }
            else
            {
                lock (renderer.renderer)
                {
                    renderer.renderer = RenderPlugins[id];
                }
            }
            previewImage.Source = renderer.renderer.PreviewImage;
            pluginDescription.Text = renderer.renderer.Description;

            var c = renderer.renderer.SettingsControl;
            if (c == null) return;
            if (c.Parent != null)
                (c.Parent as Panel).Children.Clear();
            pluginsSettings.Children.Add(c);
            c.VerticalAlignment = VerticalAlignment.Stretch;
            c.HorizontalAlignment = HorizontalAlignment.Stretch;
            c.Width = double.NaN;
            c.Height = double.NaN;
            c.Margin = new Thickness(0);
            pluginControl = c;
            if (languageSelect.SelectedIndex != -1 && Languages[languageSelect.SelectedIndex].ContainsKey(renderer.renderer.LanguageDictName))
            {
                c.Resources.MergedDictionaries[0].MergedDictionaries.Clear();
                c.Resources.MergedDictionaries[0].MergedDictionaries.Add(Languages[0][renderer.renderer.LanguageDictName]);
                c.Resources.MergedDictionaries[0].MergedDictionaries.Add(Languages[languageSelect.SelectedIndex][renderer.renderer.LanguageDictName]);
            }
        }

        private void MemoryOption_CheckToggled(object sender, RoutedEventArgs e)
        {
            if (settings == null) return;
            if (sender == enableMaxMemory) settings.enableMemoryLimit = enableMaxMemory.IsChecked;
            if (sender == collectMemoryWithCondition) settings.collectMemoryByNotes = collectMemoryWithCondition.IsChecked;
            if (sender == autoAdjustMemoryCollection) settings.autoAdjustCollection = autoAdjustMemoryCollection.IsChecked;
            if (sender == doNotWrite) settings.disableInfoWriting = doNotWrite.IsChecked;
            if (sender == enableMemorySaving) settings.enableMemorySavingMode = enableMemorySaving.IsChecked;
        }

        private void MemoryLimit_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (settings == null) return;
            if (sender == maxRenderMemory) settings.maxRenderRAM = (long)maxRenderMemory.Value;
            if (sender == collectMemoryWithCondition) settings.collectLimit = (long)collectNotesDrawnCondition.Value;
        }

        private void LoadConfig_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var open = new OpenFileDialog
            {
                Filter = "Midi files (*.mid)|*.mid"
            };
            if ((bool)open.ShowDialog())
            {
                midipath = open.FileName;
            }
            else return;

            if (!File.Exists(midipath))
            {
                MessageBox.Show("Midi file doesn't exist");
                return;
            }
            try
            {
                if (midifile != null) midifile.Dispose();
                midifile = null;
                GC.Collect();
                GC.WaitForFullGCComplete();
                midifile = new MidiFile(midipath, settings);
                Resources["midiLoaded"] = true;
                browseMidiButton.Content = Path.GetFileName(midipath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void UnloadButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Unloading midi");
            midifile.Dispose();
            midifile = null;
            GC.Collect();
            GC.WaitForFullGCComplete();
            Console.WriteLine("Unloaded");
            Resources["midiLoaded"] = false;
            browseMidiButton.SetResourceReference(Button.ContentProperty, "load");
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (renderer.renderer == null)
            {
                MessageBox.Show("No renderer is selected");
                return;
            }

            // windowTabs.SelectedIndex = 3;

            settings.realtimePlayback = realtimePlayback.IsChecked;

            settings.running = true;
            settings.width = (int)viewWidth.Value * (int)SSAAFactor.Value;
            settings.height = (int)viewHeight.Value * (int)SSAAFactor.Value;
            settings.downscale = (int)SSAAFactor.Value;
            settings.fps = (int)viewFps.Value;
            settings.ffRender = false;
            settings.Paused = false;
            settings.renderSecondsDelay = 0;
            settings.useFilterThreads = enableFilterArg.IsChecked;
            settings.filterThreadsForRender = (int)filterThreadsForRender.Value;
            settings.preview_width = (int)previewWidthSelect.Value;
            settings.preview_height = (int)previewHeightSelect.Value;
            settings.maxRenderRAM = (long)maxRenderMemory.Value;
            settings.enableMemoryLimit = enableMaxMemory.IsChecked;
            settings.autoDisableKDMAPIWhenRendering = autoDisableKDMAPI.IsChecked;
            settings.enableMemorySavingMode = enableMemorySaving.IsChecked;
            settings.collectMemoryByNotes = collectMemoryWithCondition.IsChecked;
            settings.collectLimit = (long)collectNotesDrawnCondition.Value;
            settings.autoAdjustCollection = autoAdjustMemoryCollection.IsChecked;
            settings.disableInfoWriting = doNotWrite.IsChecked;

            renderThread = Task.Factory.StartNew(RunRenderWindow, TaskCreationOptions.RunContinuationsAsynchronously | TaskCreationOptions.LongRunning);
            Resources["notPreviewing"] = false;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (settings.running == false)
            {
                Resources["notRendering"] = true;
                Resources["notPreviewing"] = true;
            }
            else
                settings.running = false;
        }

        private void StartRenderButton_Click(object sender, RoutedEventArgs e)
        {
            if (videoPath.Text == "")
            {
                MessageBox.Show("Please specify a destination path");
                return;
            }

            if (renderer.renderer == null)
            {
                MessageBox.Show("No renderer is selected");
                return;
            }

            if (File.Exists(videoPath.Text))
            {
                if (MessageBox.Show("Are you sure you want to override " + Path.GetFileName(videoPath.Text), "Override", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
            }
            if (File.Exists(alphaPath.Text))
            {
                if (MessageBox.Show("Are you sure you want to override " + Path.GetFileName(alphaPath.Text), "Override", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
            }

            settings.realtimePlayback = false;

            settings.running = true;
            settings.width = (int)viewWidth.Value * (int)SSAAFactor.Value;
            settings.height = (int)viewHeight.Value * (int)SSAAFactor.Value;
            settings.downscale = (int)SSAAFactor.Value;
            settings.fps = (int)viewFps.Value;
            settings.ffRender = true;
            settings.ffPath = videoPath.Text;
            settings.renderSecondsDelay = (double)secondsDelay.Value;

            settings.Paused = false;
            previewPaused.IsChecked = false;
            settings.tempoMultiplier = 1;
            tempoMultSlider.Value = 1;

            settings.ffmpegDebug = ffdebug.IsChecked;

            settings.useBitrate = bitrateOption.IsChecked;
            settings.CustomFFmpeg = FFmpeg.IsChecked;
            if (settings.useBitrate) settings.bitrate = (int)bitrate.Value;
            else if (settings.CustomFFmpeg)
            {
                settings.ffoption = FFmpegOptions.Text;
            }
            else
            {
                settings.crf = (int)crfFactor.Value;
                settings.crfPreset = (string)((ComboBoxItem)crfPreset.SelectedItem).Content;
            }

            settings.includeAudio = includeAudio.IsChecked;
            settings.audioPath = audioPath.Text;
            settings.ffRenderMask = includeAlpha.IsChecked;
            settings.ffMaskPath = alphaPath.Text;
            settings.useFilterThreads = enableFilterArg.IsChecked;
            settings.filterThreadsForRender = (int)filterThreadsForRender.Value;

            settings.preview_width = (int)previewWidthSelect.Value;
            settings.preview_height = (int)previewHeightSelect.Value;

            settings.maxRenderRAM = (long)maxRenderMemory.Value;
            settings.enableMemoryLimit = enableMaxMemory.IsChecked;

            settings.autoDisableKDMAPIWhenRendering = autoDisableKDMAPI.IsChecked;
            settings.enableMemorySavingMode = enableMemorySaving.IsChecked;

            settings.useFullArguments = fullFFArg.IsChecked;
            settings.fullArgs = fullFFmpegArg.Text;

            settings.collectMemoryByNotes = collectMemoryWithCondition.IsChecked;
            settings.collectLimit = (long)collectNotesDrawnCondition.Value;

            settings.autoAdjustCollection = autoAdjustMemoryCollection.IsChecked;
            settings.disableInfoWriting = doNotWrite.IsChecked;

            renderThread = Task.Factory.StartNew(RunRenderWindow, TaskCreationOptions.LongRunning | TaskCreationOptions.RunContinuationsAsynchronously);
            Resources["notPreviewing"] = false;
            Resources["notRendering"] = false;
        }

        private void BrowseVideoSaveButton_Click(object sender, RoutedEventArgs e)
        {
            var save = new SaveFileDialog
            {
                OverwritePrompt = true,
                Filter = "H.264 video (*.mp4)|*.mp4|All types|*.*"
            };
            if ((bool)save.ShowDialog())
            {
                videoPath.Text = save.FileName;
            }
        }

        private void BrowseAudioButton_Click(object sender, RoutedEventArgs e)
        {
            var audio = new OpenFileDialog
            {
                Filter = "Common audio files (*.mp3;*.wav;*.ogg;*.flac)|*.mp3;*.wav;*.ogg;*.flac"
            };
            if ((bool)audio.ShowDialog())
            {
                audioPath.Text = audio.FileName;
            }
        }

        private void BrowseAlphaButton_Click(object sender, RoutedEventArgs e)
        {
            var save = new SaveFileDialog
            {
                OverwritePrompt = true,
                Filter = "H.264 video (*.mp4)|*.mp4"
            };
            if ((bool)save.ShowDialog())
            {
                alphaPath.Text = save.FileName;
            }
        }

        private void Paused_Checked(object sender, RoutedEventArgs e)
        {
            settings.Paused = previewPaused.IsChecked;
        }

        private void VsyncEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (settings == null) return;
            settings.vsync = vsyncEnabled.IsChecked;
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                previewPaused.IsChecked = !settings.Paused;
                settings.Paused = previewPaused.IsChecked;
            }
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            ReloadPlugins();
        }

        private void PluginsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var p in RenderPlugins)
            {
                if (p.Initialized) renderer.disposeQueue.Enqueue(p);
            }
            SelectRenderer(pluginsList.SelectedIndex);
        }

        private void DoNotWrite_CheckToggled(object sender, RoutedEventArgs e)
        {
            if (settings == null) return;
            settings.disableInfoWriting = doNotWrite.IsChecked;
            if (settings.disableInfoWriting) Console.WriteLine("Console Output Disabled");
            else Console.WriteLine("Console Output Enabled");
        }

        private void ForceCollectMemory_Click(object sender, RoutedEventArgs e)
        {
            Console.Title = GC_Title;
            Console.WriteLine("Collecting Unused Memory...");
            GC.Collect();
            Console.WriteLine("Done");
            Console.Title = Console_Title;
        }

        private void ClearTheConsole_Click(object sender, RoutedEventArgs e)
        {
            Console.Clear();
        }

        private void ResolutionPreset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string preset = (string)((ComboBoxItem)resolutionPreset.SelectedItem).Content;
            switch (preset)
            {
                case "720p":
                    viewWidth.Value = 1280;
                    viewHeight.Value = 720;
                    break;
                case "1080p":
                    viewWidth.Value = 1920;
                    viewHeight.Value = 1080;
                    break;
                case "1440p":
                    viewWidth.Value = 2560;
                    viewHeight.Value = 1440;
                    break;
                case "4k":
                    viewWidth.Value = 3840;
                    viewHeight.Value = 2160;
                    break;
                case "5k":
                    viewWidth.Value = 5120;
                    viewHeight.Value = 2880;
                    break;
                case "8k":
                    viewWidth.Value = 7680;
                    viewHeight.Value = 4320;
                    break;
                case "16k":
                    viewWidth.Value = 15360;
                    viewHeight.Value = 8640;
                    break;
                default:
                    break;
            }
        }

        private void Config_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // string ConfigString = (string)((ComboBoxItem)configList.SelectedItem).Content;
            string ConfigString = "";
            if (ConfigString == "Default")
            {
                viewWidth.Value = settings.width = 1920;
                viewHeight.Value = settings.height = 1080;
                previewWidthSelect.Value = settings.preview_width = (int)(OpenTK.DisplayDevice.Default.Width / 1.5);
                previewHeightSelect.Value = settings.preview_height = settings.preview_width / 16 * 9;
                SSAAFactor.Value = 1;
                viewFps.Value = settings.fps = 60;
                noteSizeStyle.SelectedIndex = 0;
                ignoreColorEvents.IsChecked = false;
                tempoMultSlider.Value = settings.tempoMultiplier = 1;
                skipValue.Value = settings.skip = 5000;
                vsyncEnabled.IsChecked = settings.vsync = false;
                previewPaused.IsChecked = settings.Paused = false;
                realtimePlayback.IsChecked = settings.realtimePlayback = true;
            }
        }

        private void LanguageSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pluginControl != null)
                lock (renderer.renderer)
                {
                    ((UserControl)pluginControl).Resources.MergedDictionaries[0].MergedDictionaries.Clear();
                    ((UserControl)pluginControl).Resources.MergedDictionaries[0].MergedDictionaries.Add(Languages[0][renderer.renderer.LanguageDictName]);
                    ((UserControl)pluginControl).Resources.MergedDictionaries[0].MergedDictionaries.Add(Languages[languageSelect.SelectedIndex][renderer.renderer.LanguageDictName]);
                }
            Resources.MergedDictionaries[0].MergedDictionaries.Clear();
            Resources.MergedDictionaries[0].MergedDictionaries.Add(Languages[0]["window"]);
            Resources.MergedDictionaries[0].MergedDictionaries.Add(Languages[languageSelect.SelectedIndex]["window"]);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (foundOmniMIDI)
                KDMAPI.TerminateKDMAPIStream();
        }

        private void Checkbox_Checked(object sender, RoutedEventArgs e)
        {
            if (settings == null) return;
            if (sender == realtimePlayback) settings.realtimePlayback = realtimePlayback.IsChecked;
        }

        private void DisableKDMAPI_Click(object sender, RoutedEventArgs e)
        {
            if (OmniMIDIDisabled)
            {
                disableKDMAPI.Content = Resources["disableKDMAPI"];
                OmniMIDIDisabled = false;
                settings.playbackEnabled = true;
                try
                {
                    Console.WriteLine("Loading KDMAPI...");
                    KDMAPI.InitializeKDMAPIStream();
                    Console.WriteLine("Loaded!");
                }
                catch { }
            }
            else
            {
                disableKDMAPI.Content = Resources["enableKDMAPI"];
                OmniMIDIDisabled = true;
                settings.playbackEnabled = false;
                try
                {
                    Console.WriteLine("Unloading KDMAPI");
                    KDMAPI.TerminateKDMAPIStream();
                }
                catch { }
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void ReloadKDMAPI_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Reloading KDMAPI");
            KDMAPI.ResetKDMAPIStream();
            KDMAPI.SendDirectData(0x0);
            Console.WriteLine("Done");
        }

        private void NoteSizeStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (settings == null) return;
            if (noteSizeStyle.SelectedIndex == 0) settings.timeBasedNotes = false;
            if (noteSizeStyle.SelectedIndex == 1) settings.timeBasedNotes = true;
        }

        private void IgnoreColorEvents_Checked(object sender, RoutedEventArgs e)
        {
            if (settings == null) return;
            settings.ignoreColorEvents = ignoreColorEvents.IsChecked;
        }

        private void UseBGImage_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (useBGImage.IsChecked && bgImagePath.Text != "")
                {
                    settings.BGImage = bgImagePath.Text;
                }
                else
                {
                    settings.BGImage = null;
                }
                settings.lastBGChangeTime = DateTime.Now.Ticks;
            }
            catch { }
        }

        private void BrowseBG_Click(object sender, RoutedEventArgs e)
        {
            var open = new OpenFileDialog
            {
                Filter = "Image files |*.png;*.bmp;*.jpg;*.jpeg"
            };
            if ((bool)open.ShowDialog())
            {
                bgImagePath.Text = open.FileName;
                try
                {
                    settings.BGImage = bgImagePath.Text;
                }
                catch
                {
                    settings.BGImage = null;
                }
                settings.lastBGChangeTime = DateTime.Now.Ticks;
            }
        }

        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space) settings.Paused = !settings.Paused;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimiseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
            }
            catch { }
            WindowState = WindowState.Minimized;
        }

        private void tempoMultSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (settings != null) settings.tempoMultiplier = tempoMultSlider.Value;
        }

        // set value of skipping
        private void Skip_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (settings == null) return;
            settings.skip = (int)skipValue.Value;
        }

        private void UseMultithreadToLoadMidi_CheckToggled(object sender, RoutedEventArgs e)
        {
            if (settings == null) return;
            settings.useMultithreadToLoadMidi = useMultithreadToLoadMidi.IsChecked;
        }

        private void updateDownloaded_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ZenithUpdates.KillAllProcesses();
            Process.Start(ZenithUpdates.InstallerPath, "update -Reopen");
            Close();
        }

        private void enableFilterThreads_Toggled(object sender, RoutedEventArgs e)
        {
            if (settings == null) return;
            settings.useFilterThreads = enableFilterArg.IsChecked;
        }
        // set changing of threads value
        private void threadsValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            if (settings == null) return;
            settings.filterThreadsForRender = (int)filterThreadsForRender.Value;
        }
        // bitrate, crf or custom?
        private void renderSettingsChanged(object sender, RoutedEventArgs e)
        {
            if (settings == null) return;
            if (sender == bitrateOption)
            {
                bitrateOption.IsChecked = true;
                crfOption.IsChecked = false;
                FFmpeg.IsChecked = false;
            }
            else if (sender == crfOption)
            {
                bitrateOption.IsChecked = false;
                crfOption.IsChecked = true;
                FFmpeg.IsChecked = false;
            }
            else
            {
                bitrateOption.IsChecked = false;
                crfOption.IsChecked = false;
                FFmpeg.IsChecked = true;
            }
        }
    }

    public class CustomTabs : TabControl
    {

        public string VersionName
        {
            get { return (string)GetValue(VersionNameProperty); }
            set { SetValue(VersionNameProperty, value); }
        }

        public static readonly DependencyProperty VersionNameProperty =
            DependencyProperty.Register("VersionName", typeof(string), typeof(CustomTabs), new PropertyMetadata(""));
    }

    public class AndValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool b = true;
            for (int i = 0, valuesLength = values.Length; i < valuesLength; ++i) b = b && (bool)values[i];

            return b;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OrValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool b = false;
            for (int i = 0; i < values.Length; i++) b = b || (bool)values[i];

            return b;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NotValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)values[0];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}


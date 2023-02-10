using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Zeraniumu;

namespace BluestackPlugin
{
    public class Bluestacks:IEmulator
    {
        private string BlueStackPath, BootParameters, VBoxManagerPath, _adbShellOptions;
        private Process bluestacks;

        public MinitouchMode MinitouchMode => MinitouchMode.STD;

        string IEmulator.AdbShellOptions => _adbShellOptions;
        public SharedFolder GetSharedFolder
        {
            get
            {
                SharedFolder sharedFolder = new SharedFolder
                {
                    AndroidPath = "/storage/sdcard/windows/BstSharedFolder/"
                };
                for (int x = 0; x < 5; x++)
                {
                    RegistryKey key;
                    if (Environment.Is64BitOperatingSystem)
                    {
                        key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    }
                    else
                    {
                        key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                    }
                    var reg = key.OpenSubKey(@"SOFTWARE\BlueStacks\Guests\Android\SharedFolder\" + x);
                    var folder = reg.GetValue("Name").ToString();
                    if (folder == "BstSharedFolder")
                    {
                        sharedFolder.PCPath = reg.GetValue("Path").ToString();
                        break;
                    }
                }
                if (sharedFolder.PCPath == null)
                {
                    throw new NotImplementedException();
                }
                return sharedFolder;
            }
        }
        public Rectangle ActualSize()
        {
            return new Rectangle(0, 0, 876, 715);
        }

        public string AdbIpPort()
        {
            RegistryKey key;
            if (Environment.Is64BitOperatingSystem)
            {
                key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            }
            else
            {
                key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            }
            var port = key.OpenSubKey(@"SOFTWARE\BlueStacks\Guests\Android\Config").GetValue("BstAdbPort").ToString();
            return "127.0.0.1:" + port;
        }

        public bool CheckEmulatorExist(string arguments, ILog logger)
        {
            var plusMode = false;
            var frontendexe = new string[] { "HD-Frontend.exe", "HD-Player.exe" };
            RegistryKey key;
            if (Environment.Is64BitOperatingSystem)
            {
                key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            }
            else
            {
                key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            }
            key = key.OpenSubKey(@"SOFTWARE").OpenSubKey("BlueStacks");
            if(key == null)
            {
                return false;
            }
            var result = key.GetValue("Engine");
            if (result != null && result.ToString().ToLower() == "plus")
            {
                plusMode = true;
            }
            if (plusMode)
            {
                frontendexe = new string[] { "HD-Plus-Frontend.exe" };
            }
            BlueStackPath = key.GetValue("InstallDir").ToString();
            if (!Directory.Exists(BlueStackPath))
            {
                string programFiles = Environment.ExpandEnvironmentVariables("%ProgramW6432%");
                string programFilesX86 = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%");
                if (Directory.Exists(Path.Combine(programFiles, "BlueStacks")))
                {
                    BlueStackPath = Path.Combine(programFiles, "BlueStacks");
                }
                else if(Directory.Exists(Path.Combine(programFilesX86, "BlueStacks")))
                {
                    BlueStackPath = Path.Combine(programFilesX86, "BlueStacks");
                }
                else
                {
                    return false;
                }
            }
            foreach(var file in frontendexe)
            {
                if (File.Exists(Path.Combine(BlueStackPath,file)))
                {
                    VBoxManagerPath = BlueStackPath + "BstkVMMgr.exe";
                    _adbShellOptions = "/data/anr/../../system/xbin/bstk/su root ";
                    BlueStackPath = Path.Combine(BlueStackPath, file);
                    BootParameters = key.OpenSubKey(@"Guests\Android").GetValue("BootParameters").ToString();
                    
                    return true;
                }
            }
            return false;
        }

        public string DefaultArguments()
        {
            return "Android";
        }

        public Rectangle DefaultSize()
        {
            return new Rectangle(1, 12, 860, 676);
        }

        public string EmulatorName()
        {
            return "BlueStacks Android PluginAndroid|藍疊";
        }

        public Point GetAccurateClickPoint(Point point)
        {
            return new Point(32767 / ActualSize().Width * point.X, 32767 / ActualSize().Height * point.Y);
        }

        public void SetResolution(int x, int y, int dpi)
        {
            RegistryKey key;
            if (Environment.Is64BitOperatingSystem)
            {
                key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            }
            else
            {
                key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            }
            key = key.OpenSubKey(@"SOFTWARE\BlueStacks\Guests\Android\FrameBuffer\0", true);
            key.SetValue("FullScreen", 0, RegistryValueKind.DWord);
            key.SetValue("GuestHeight", y, RegistryValueKind.DWord);
            key.SetValue("GuestWidth", x, RegistryValueKind.DWord);
            key.SetValue("WindowHeight", y, RegistryValueKind.DWord);
            key.SetValue("GuestWidth", x, RegistryValueKind.DWord);
            BootParameters = Regex.Replace(BootParameters, "DPI=\\d+", "DPI=" + dpi);
            if (Environment.Is64BitOperatingSystem)
            {
                key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            }
            else
            {
                key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            }
            key.OpenSubKey(@"SOFTWARE\BlueStacks\Guests\Android", true).SetValue("BootParameters", BootParameters , RegistryValueKind.String);
        }

        public void StartEmulator(string arguments)
        {
            if(string.IsNullOrEmpty(arguments))
            {
                arguments = "Android";
            }
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = BlueStackPath;
            info.Arguments = arguments;
            bluestacks = Process.Start(info);
            Thread.Sleep(5000);
        }

        public void StopEmulator(Process emulator, string arguments)
        {
            ProcessStartInfo close = new ProcessStartInfo();
            close.FileName = VBoxManagerPath;
            if(arguments == null)
            {
                arguments = "Android";
            }
            close.Arguments = "controlvm " + arguments + " poweroff";
            close.CreateNoWindow = true;
            close.WindowStyle = ProcessWindowStyle.Hidden;
            try
            {
                emulator.Kill();
            }
            catch
            {

            }
            Process p = Process.Start(close);
            Thread.Sleep(5000);
            if (!p.HasExited)
            {
                try
                {
                    p.Kill();
                }
                catch
                {

                }
            }
            if(bluestacks!= null)
            {
                if (!bluestacks.HasExited)
                {
                    bluestacks.Kill();
                }
                bluestacks.Dispose();
                bluestacks = null;
            }

        }

        public void UnUnbotify(EmulatorController controller)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace DesktopBackground
{
    internal static class Program
    {
        private const string ShellActionKey = @"DesktopBackground\Shell\DesktopBackgroundLocation";
        private const string CommandSubkey = "command";
        private const string ShellCommandKey = ShellActionKey + "\\" + CommandSubkey;

        private static string GetWallpaperPath()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop"))
            {
                if (!(key?.GetValue("TranscodedImageCache") is byte[] wallpaperEncoded))
                {
                    throw new KeyNotFoundException(@"HKCU\Control Panel\Desktop\TranscodedImageCache");
                }

                // for some reason, Windows stores the path with some unknown metadata in the first 24 bytes (or 12 UTF-16 chars)
                string wallpaperPath = Encoding.Unicode.GetString(wallpaperEncoded);
                wallpaperPath = wallpaperPath.Substring(12).Trim('\0');

                if (!File.Exists(wallpaperPath))
                {
                    throw new FileNotFoundException("Could not find desktop background", wallpaperPath);
                }

                return wallpaperPath;
            }
        }

        private static void CreateShellEntry(string location)
        {
            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(ShellActionKey))
            {
                if (key == null)
                {
                    throw new KeyNotFoundException(ShellActionKey);
                }

                key.SetValue(null, "Find background in Explorer");
                key.SetValue("Icon", "shell32.dll,22");

                using (RegistryKey commandKey = key.CreateSubKey("command"))
                {
                    if (commandKey == null)
                    {
                        throw new KeyNotFoundException(ShellCommandKey);
                    }

                    commandKey.SetValue(null, $"\"{location}\" /explorer");
                }
            }
        }

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int pid);

        private static void Main(string[] args)
        {
            try
            {
                AttachConsole(-1);
                Console.WriteLine();

                if (args.Length > 1)
                {
                    throw new ArgumentException("Must specify at most one of /explorer, /install, /uninstall");
                }

                string wallpaperPath = GetWallpaperPath();
                string exeLocation = Assembly.GetEntryAssembly().Location;

                string mode = args.Length == 1 ? args[0] : null;
                switch (mode)
                {
                    case null:
                        Console.WriteLine($"Found desktop wallpaper at:\n\t{wallpaperPath}");
                        break;
                    case "/explorer":
                        Process.Start("explorer.exe", $"/select,\"{wallpaperPath}\"");
                        break;
                    case "/install":
                        CreateShellEntry(exeLocation);
                        Console.WriteLine("Success!");
                        break;
                    case "/uninstall":

                        Registry.ClassesRoot.DeleteSubKey(ShellCommandKey);
                        Registry.ClassesRoot.DeleteSubKey(ShellActionKey);
                        Console.WriteLine("Success!");
                        break;
                    default:
                        throw new ArgumentException($"Unknown flag {mode}");
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error: {e.Message}");
            }
        }
    }
}
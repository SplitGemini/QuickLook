using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QuickLook
{
    internal class DesktopWatcher
    {
        private static DesktopWatcher _instance;
        private static FileSystemWatcher _watcher = new FileSystemWatcher();
        private static string publicDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
        private static readonly string userDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        protected DesktopWatcher()
        {
            WatcherStart(publicDesktopPath, "*");
        }
        private static void WatcherStart(string path, string filter)
        {
            _watcher.Path = path;
            _watcher.Filter = filter;
            _watcher.Changed += new FileSystemEventHandler(OnProcess);
            _watcher.Created += new FileSystemEventHandler(OnProcess);
            _watcher.EnableRaisingEvents = true;
            _watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName
                                   | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size;
            _watcher.IncludeSubdirectories = false;
        }
        private static void OnProcess(object source, FileSystemEventArgs e)
        {
            _watcher.Changed -= new FileSystemEventHandler(OnProcess);
            _watcher.Created -= new FileSystemEventHandler(OnProcess);
            MoveFiles();
            _watcher.Changed += new FileSystemEventHandler(OnProcess);
            _watcher.Created += new FileSystemEventHandler(OnProcess);
        }
        private static void MoveFiles()
        {
            System.Threading.Thread.Sleep(2000);
            if (Directory.Exists(publicDesktopPath))
            {
                string[] files = Directory.GetFiles(publicDesktopPath);
                string fileName, destPath;
                foreach (string s in files)
                {
                    fileName = Path.GetFileName(s);
                    if (fileName == "desktop.ini") continue;
                    destPath = Path.Combine(userDesktopPath, fileName);
                    if (!File.Exists(destPath))
                        try
                        {
                            File.Move(s, destPath);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("桌面监控出现问题：移动桌面文件," + e.Message, "出现问题");
                            break;
                        }
                    else
                    {
                        try
                        {
                            File.Delete(s);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("桌面监控出现问题：删除公用桌面文件," + e.Message, "出现问题");
                            break;
                        }

                    }
                }
            }
        }
        internal static DesktopWatcher GetInstance()
        {
            return _instance ?? (_instance = new DesktopWatcher());
        }
    }
}

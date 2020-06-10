/* create by gh */
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
        private static FileSystemWatcher _watcher = null;
        private static readonly string publicDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
        private static readonly string userDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        private static readonly FileSystemEventHandler _onProcess = new FileSystemEventHandler(OnProcess);
        protected DesktopWatcher() {}

        public void WatcherStart()
        {
            if (_watcher != null) return;
            MoveFiles();
            _watcher = new FileSystemWatcher
            {
                Path = publicDesktopPath,
                Filter = "*",
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName
                                   | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size,
                IncludeSubdirectories = false
            };
            _watcher.Changed += _onProcess;
            _watcher.Created += _onProcess;
        }

        public void WatcherEnd()
        {
            _watcher.Changed -= _onProcess;
            _watcher.Created -= _onProcess;
            _watcher.Dispose();
            _watcher = null;
        }

        private static void OnProcess(object source, FileSystemEventArgs e)
        {
            _watcher.Changed -= _onProcess;
            _watcher.Created -= _onProcess;
            Task.Delay(2000).ContinueWith(_ => MoveFiles());
            _watcher.Changed += _onProcess;
            _watcher.Created += _onProcess;
        }

        private static void MoveFiles()
        {
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

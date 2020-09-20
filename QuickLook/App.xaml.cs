﻿// Copyright © 2017 Paddy Xu
// 
// This file is part of QuickLook program.
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using QuickLook.Common.Helpers;
using QuickLook.Helpers;

namespace QuickLook
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly string UserPluginPath = Path.Combine(SettingHelper.LocalDataPath, "QuickLook.Plugin\\");
        public static readonly string AppFullPath = Assembly.GetExecutingAssembly().Location;
        public static readonly string AppPath = Path.GetDirectoryName(AppFullPath);

        //comment by gh
        //public static readonly bool Is64Bit = Environment.Is64BitProcess;
        //-----------//

        public static readonly bool IsUWP = ProcessHelper.IsRunningAsUWP();
        public static readonly bool IsWin10 = Environment.OSVersion.Version >= new Version(10, 0);
        public static readonly bool IsGPUInBlacklist = SystemHelper.IsGPUInBlacklist();

        private bool _cleanExit;
        private Mutex _isRunning;

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                ProcessHelper.WriteLog(((Exception) args.ExceptionObject).ToString());
            };

            base.OnStartup(e);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!EnsureOSVersion() || !EnsureFirstInstance(e.Args)
                || !EnsureFolderWritable(SettingHelper.LocalDataPath))
            {
                _cleanExit = false;
                Shutdown();
                return;
            }

            //comment by gh - 不自动check update
            //CheckUpdate();
            //----------//

            RunListener(e);

            // first instance: run and preview this file
            if (e.Args.Any() && (Directory.Exists(e.Args.First()) || File.Exists(e.Args.First())))
                PipeServerManager.SendMessage(PipeMessages.Toggle, e.Args.First());
        }

        private bool EnsureOSVersion()
        {
            if (!ProcessHelper.IsOnWindows10S())
                return true;
            MessageBox.Show("This application does not run on Windows 10 S.");
            return false;
        }

        private bool EnsureFolderWritable(string folder)
        {
            try
            {
                var path = FileHelper.CreateTempFile(folder);
                File.Delete(path);
            }
            catch
            {
                MessageBox.Show(string.Format(TranslationHelper.Get("APP_PATH_NOT_WRITABLE"), folder), "QuickLook",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }
    /* comment by gh
    private void CheckUpdate()
    {
        if (DateTime.Now.Ticks - SettingHelper.Get<long>("LastUpdateTicks") < TimeSpan.FromDays(7).Ticks)
            return;

        Task.Delay(120 * 1000).ContinueWith(_ => Updater.CheckForUpdates(true));
        SettingHelper.Set("LastUpdateTicks", DateTime.Now.Ticks);
    }
    *///----------//


        private void RunListener(StartupEventArgs e)
        {

            //comment by gh - 启动不显示启动提示
            //TrayIconManager.GetInstance();
            //if (!e.Args.Contains("/autorun") && !IsUWP)
            //TrayIconManager.ShowNotification("", TranslationHelper.Get("APP_START"));
            //if (e.Args.Contains("/first"))
            //AutoStartupHelper.CreateAutorunShortcut();
            //----------//

            //add by gh - 自动启动隐藏后台图标
            if (SettingHelper.Get("Visible", true))
            {
                TrayIconManager.GetInstance();
                if (!e.Args.Contains("/autorun") && !IsUWP)
                    TrayIconManager.ShowNotification("", TranslationHelper.Get("APP_START"));
            }
            else if (e.Args.Contains("/setvisible"))
            {
                SettingHelper.Set("Visible", true);
                TrayIconManager.GetInstance();
                if (!IsUWP)
                    TrayIconManager.ShowNotification("", TranslationHelper.Get("APP_START"));
            }
            //自己添加的功能，与quick look无关
            if (SettingHelper.Get("Watcher", false))
            {
                DesktopWatcher.GetInstance().WatcherStart();
            }
            
            if (e.Args.Contains("/first"))
            {
                AutoStartupHelper.CreateAutorunShortcut();
            }
            //----------//

            NativeMethods.QuickLook.Init();

            PluginManager.GetInstance();
            ViewWindowManager.GetInstance();
            KeystrokeDispatcher.GetInstance();
            PipeServerManager.GetInstance();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            if (!_cleanExit)
                return;

            _isRunning.ReleaseMutex();

            PipeServerManager.GetInstance().Dispose();

            //add by gh - 自动启动隐藏后台图标
            if (SettingHelper.Get("Visible", true))
            {
                TrayIconManager.GetInstance().Dispose();
            }
            //----------//

            //comment by gh
            //TrayIconManager.GetInstance().Dispose();
            //----------//

            KeystrokeDispatcher.GetInstance().Dispose();
            ViewWindowManager.GetInstance().Dispose();
        }

        private bool EnsureFirstInstance(string[] args)
        {
            _isRunning = _isRunning = new Mutex(true, "QuickLook.App.Mutex", out bool isFirst);
            if (isFirst)
                return true;

            // second instance: preview this file
            if (args.Any() && (Directory.Exists(args.First()) || File.Exists(args.First())))
            {
                PipeServerManager.SendMessage(PipeMessages.Toggle, args.First());
            }

            //add by gh
            else if (args.Contains("/setvisible"))
            {
                SettingHelper.Set("Visible", true);
                MessageBox.Show(TranslationHelper.Get("APP_SECOND_TEXT_HIDE"), TranslationHelper.Get("APP_SECOND"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            //---------//

            // second instance: duplicate
            else
                MessageBox.Show(TranslationHelper.Get("APP_SECOND_TEXT"), TranslationHelper.Get("APP_SECOND"),
                    MessageBoxButton.OK, MessageBoxImage.Information);

            return false;
        }
    }
}
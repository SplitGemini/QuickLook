// Copyright © 2017 Paddy Xu
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
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using QuickLook.Common.Helpers;
using QuickLook.Helpers;
using QuickLook.Properties;

namespace QuickLook
{
    internal class TrayIconManager : IDisposable
    {
        private static TrayIconManager _instance;

        private readonly NotifyIcon _icon;

        private readonly MenuItem _itemAutorun =
            new MenuItem(TranslationHelper.Get("Icon_RunAtStartup"),
                (sender, e) =>
                {
                    if (AutoStartupHelper.IsAutorun())
                        AutoStartupHelper.RemoveAutorunShortcut();
                    else
                        AutoStartupHelper.CreateAutorunShortcut();
                }) {Enabled = !App.IsUWP};

        //add by gh
        private readonly MenuItem _visible =
            new MenuItem(TranslationHelper.Get("Icon_VisibleLabel"),
                (sender, e) =>
                {
                    SettingHelper.Set("Visible", false);
                    ShowNotification(TranslationHelper.Get("Icon_HideMessageTitle"), TranslationHelper.Get("Icon_HideMessage"));
                    GetInstance().Dispose();
                })
            { Enabled = true };
        //----------//

        //add by gh
        private readonly MenuItem _watcher =
            new MenuItem(TranslationHelper.Get("Icon_WatcherLabel"),
                (sender, e) =>
                {
                    if(!SettingHelper.Get("Watcher", false))
                    {
                        SettingHelper.Set("Watcher", true);
                        DesktopWatcher.GetInstance().WatcherStart();
                    }
                    else
                    {
                        SettingHelper.Set("Watcher", false);
                        DesktopWatcher.GetInstance().WatcherEnd();
                    }
                })
            { Enabled = true };
        //----------//

        private TrayIconManager()
        {
            _icon = new NotifyIcon
            {
                Text = string.Format(TranslationHelper.Get("Icon_ToolTip"),
                    Application.ProductVersion),
                Icon = GetTrayIconByDPI(),
                Visible = true,
                ContextMenu = new ContextMenu(new[]
                {
                    //comment by gh
                    //new MenuItem($"v{Application.ProductVersion}{(App.IsUWP ? " (UWP)" : "")}") {Enabled = false},
                    //----------//

                    //add by gh
                    new MenuItem($"v{Application.ProductVersion}{(App.IsUWP ? " (UWP)" : "")}",
                                (sender, e) => {  ShowNotification(TranslationHelper.Get("Icon_VersionMessageTitle"),
                                                  string.Format(TranslationHelper.Get("Icon_VersionMessage"), Application.ProductVersion));}),
                    //----------//

                    new MenuItem("-"),
                    new MenuItem(TranslationHelper.Get("Icon_CheckUpdate"), (sender, e) => Updater.CheckForUpdates()),
                    new MenuItem(TranslationHelper.Get("Icon_GetPlugin"),
                        (sender, e) => Process.Start("https://github.com/QL-Win/QuickLook/wiki/Available-Plugins")),
                    _itemAutorun,

                    //add by gh - 添加隐藏选项
                    _visible,
                    _watcher,
                    //----------//

                    new MenuItem(TranslationHelper.Get("Icon_Quit"),
                        (sender, e) => System.Windows.Application.Current.Shutdown())
                })
            };

            _icon.ContextMenu.Popup += (sender, e) => { _itemAutorun.Checked = AutoStartupHelper.IsAutorun();
                //add by gh
                _watcher.Checked = SettingHelper.Get("Watcher", false);
                //----------------//
            };
       
        }

        public void Dispose()
        {
            _icon.Visible = false;
        }

        private Icon GetTrayIconByDPI()
        {
            var scale = DpiHelper.GetCurrentScaleFactor().Vertical;

            if (!App.IsWin10)
                return scale > 1 ? Resources.app : Resources.app_16;

            return OSThemeHelper.SystemUsesDarkTheme()
                ? (scale > 1 ? Resources.app_white : Resources.app_white_16)
                : (scale > 1 ? Resources.app_black : Resources.app_black_16);
        }

        public static void ShowNotification(string title, string content, bool isError = false, int timeout = 5000,
            Action clickEvent = null,
            Action closeEvent = null)
        {
            var icon = GetInstance()._icon;
            icon.ShowBalloonTip(timeout, title, content, isError ? ToolTipIcon.Error : ToolTipIcon.Info);
            icon.BalloonTipClicked += OnIconOnBalloonTipClicked;
            icon.BalloonTipClosed += OnIconOnBalloonTipClosed;

            void OnIconOnBalloonTipClicked(object sender, EventArgs e)
            {
                clickEvent?.Invoke();
                icon.BalloonTipClicked -= OnIconOnBalloonTipClicked;
            }


            void OnIconOnBalloonTipClosed(object sender, EventArgs e)
            {
                closeEvent?.Invoke();
                icon.BalloonTipClosed -= OnIconOnBalloonTipClosed;
            }
        }

        public static TrayIconManager GetInstance()
        {
            return _instance ?? (_instance = new TrayIconManager());
            
        }
    }
}
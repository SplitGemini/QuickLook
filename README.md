<img src="https://user-images.githubusercontent.com/1687847/82130498-8c3eac80-97d4-11ea-9e88-372ab9c50295.png" width="80">

### 3.6.5 -> 3.6.6 -> 3.6.7 -> 3.6.8

QuickLook Project
- edit "TrayIconManager.cs"
- edit "App.xaml.cs"
- edit "NativeMethods.Quicklook.cs"
- edit "PipeServerManager.cs"
- add "DesktopWatcher.cs"
- edit NativeMethod/QuickLook.cs
- remove Plugin/"WindowsThumbnailProvider.cs"

QuickLook.Plugin - VideoViewer
- edit "ViewPanel.xaml.cs"
- edit "Plugin.cs"
- update LavFilters 0.72 to 0.74.1 and remove 32 bit version
- remove 32 bit version Mediainfo
- update MediaInfo.dll and MediaInfo.Wrapper.dll
- add Lyric folder to achive show lyric line when play music

QuickLook.Plugin - ImageViewer
- edit "Plugin.cs"
- update Nconvert and its plugin to 64 bit and the newest version
- update 3.6.7 to 3.6.8 - Nconvert is repaced by magick

QuickLook.Plugin - FontViewer
- edit "FontViewerPanel.xaml.cs"
- edit "Plugin.cs"

QuickLook.Plugin - MarkdownViewer
- edit "Plugin.cs"

QuickLook.Plugin - OfficeViewer-Native
- edit "Plugin.cs"

QuickLook.Plugin - TextViewer
- edit "TextViewerPanel.xaml.cs"

QuickLook.Plugin - CsvViewer
- edit "CsvViewerPanel.xaml.cs"

QuickLook.Plugin - PdfViewer
- edit "Plugin.cs"
- remove 32 bit 

remove QuickLook.Native - QuickLook.Native32 project

remove QuickLook.Native - QuickLook.WoW64HookHelper project

QuickLook.Native - QuickLook.Native64
- edit lots of files

QuickLook.Installer
- Product.wxs

QuickLook.Common
- add "ExtensionMethod/WindowsThumbnailExtension.cs"
- add "ExtensionMethod/EncondingExtensions.cs"


改进：
1. 后台任务栏图标添加隐藏选项，选择隐藏后之后不会显示后台图标，加启动参数"/setvisible"后再次启动恢复，安装包自动增加的桌面和开始菜单快捷方式默认带"/setvisible"参数
1. 修复预览.lnk快捷方式BUG （原版3.6.8版本也做了相同修改）
1. 修复当预览有多张封面的mp3时无法预览问题,音频封面预览问题
1. VideoViewer增加rmvb和bik格式预览
1. ImageViewer增加dds,tga格式预览（3.6.8 更新支持tga了）
1. 将外部插件整合进解决方案里，包括FontViewer,EpubViewer,OfficeViewer-Native
1. 修改插件界面大小：FontViewer，OfficeViewer-Native，MarkdownViewer,PdfViewer,ImageViewer
1. 修改FontViewer样例显示内容
1. QuickLook项目添加监控桌面文件夹功能，作用是把公用桌面文件全部移动到用户桌面中
1. 删除Native32和WoW64HookHelper项目，修改QuickLook、Native64、VideoViewer、PdfViewer,ImageViewer的文件，试图将QuickLook完全64位化
1. 将之前在VideoViewer添加的预览缩略图文件"WindowsThumbnailProvider.cs"移动在QuickLook.Common中并改名为"WindowsThumbnailExtension.cs"，并删除QuickLook原有的文件
1. ImageViewer读取不出heif图片大小时使用系统方法（升级到3.6.8不需要了）
1. 更新MediaViewer里的MediaInfo和MediaInfo.Wrapper
1. 添加音乐界面歌词显示
1. 文本预览和csv预览能够正确识别gbk编码

# QuickLook

[![license](https://img.shields.io/github/license/QL-Win/QuickLook.svg)](https://www.gnu.org/licenses/gpl-3.0.en.html)
[![AppVeyor](https://img.shields.io/appveyor/ci/xupefei/QuickLook.svg)](https://ci.appveyor.com/project/xupefei/QuickLook)
[![Github All Releases](https://img.shields.io/github/downloads/QL-Win/QuickLook/total.svg)](https://github.com/QL-Win/QuickLook/releases)
[![GitHub release](https://img.shields.io/github/release/QL-Win/QuickLook.svg)](https://github.com/QL-Win/QuickLook/releases/latest)

<img src="https://user-images.githubusercontent.com/1687847/64730506-2a60d100-d4e8-11e9-95a6-f13dbd869b2a.jpeg" width="400">

## Background
One of the few features I missed from macOS is [Quick Look](https://en.wikipedia.org/wiki/Quick_Look). It allows users to peek into a file content in lightning speed by just pressing the <kbd>Space</kbd> key. Windows, on the other hand, does not have this handy feature ... until now.

I am aware that several alternatives are already available on the Internet (e.g. [WinQuickLook](https://github.com/shibayan/WinQuickLook) and [Seer](https://github.com/ccseer/Seer)). Despite these options, I still decided to craft another one by myself, because they are either not being actively developed, lack of variety, or ask for some :dollar:.

## Highlights

 - Tons of supported file types (full list [here](https://github.com/QL-Win/QuickLook/wiki/Supported-File-Types))
 - Fluent design (new in version 0.3)
 - Touchscreen friendly
 - HiDPI support
 - Preview from *Open* and *Save File* Dialog
 - Preview from 3rd-party file managers (see a list [here](https://github.com/QL-Win/QuickLook/wiki/File-Managers))
 - Easily extended by [plugins](https://github.com/QL-Win/QuickLook/wiki/Available-Plugins)
 - Strict GPL license to keep it free forever

## Usage

### Download/Installation

Get it from one of the following sources:

  * Microsoft Store (Windows 10 users only, no preview in open/save-dialogs available) <a href="https://www.microsoft.com/store/apps/9nv4bs3l1h4s?ocid=badge" target="_blank"><img src="https://assets.windowsphone.com/13484911-a6ab-4170-8b7e-795c1e8b4165/English_get_L_InvariantCulture_Default.png" height="22px" alt="Store Link" /></a> 
  * Installer or portable archive of the stable version from [GitHub Release](https://github.com/QL-Win/QuickLook/releases) 
  * Nightly builds from [AppVeyor](https://ci.appveyor.com/project/xupefei/quicklook/build/artifacts)

[What are the differences between `.msi`, `.zip`, Nightly and Store versions?](https://github.com/QL-Win/QuickLook/wiki/Differences-Between-Distributions)

**Note:** Don't forget to check out the [plugins](https://github.com/QL-Win/QuickLook/wiki/Available-Plugins) for previewing office-files, epubs, fonts and more!


### Typical usecase

1. Run `QuickLook.exe` (only necessary if autostart is disabled)
1. Select any file or folder (on the Desktop, in a File Explorer window, in an *Open* or *Save-File* dialogue, doesn't matter)
1. Press <kbd>Spacebar</kbd>
1. Enjoy the preview and interact with it
1. Preview next file by clicking on it or using arrow-keys (arrow-keys move selection in the background if the preview window is not in focus)
1. When you're done close it by either hitting <kbd>Spacebar</kbd> again, pressing <kbd>Esc</kbd> or clicking the `⨉` button

### Hotkeys and buttons

 - <kbd>Spacebar</kbd> Show/Hide the preview window
 - <kbd>Esc</kbd> Hide the preview window
 - <kbd>Enter</kbd> Open/Execute current file
 - <kbd>Mouse</kbd> <kbd>↑</kbd> <kbd>↓</kbd> <kbd>←</kbd> <kbd>→</kbd> Preview another file
 - <kbd>Mouse Wheel</kbd> Zoom in/out (images)
 - <kbd>Ctrl</kbd>+<kbd>Mouse Wheel</kbd> Zoom in/out (PDFs)
 - <kbd>Wheel</kbd> Increase/decrease volume

## Supported file types, file manager intergation, etc.

See the [Wiki page](https://github.com/QL-Win/QuickLook/wiki)

## Translations

See the [Translation guide](https://github.com/QL-Win/QuickLook/wiki/Translations)

## Thanks to

 - Many [open-source projects](https://github.com/QL-Win/QuickLook/wiki/On-the-Shoulders-of-Giants) and their contributors
 - Our UI designers [@OiCkilL](https://twitter.com/OiCkilL) (“Fluent” user interface since v0.3) and [@QubitsDev](https://twitter.com/qubitsdev) (application icon since v0.3)
 - [Our contributers](https://github.com/QL-Win/QuickLook/graphs/contributors) who
     - teach QuickLook to speak *your* language
     - send pull requests, report bugs or give suggestions
 - ... and you 😊

## Licenses

![GPL-v3](https://www.gnu.org/graphics/gplv3-127x51.png)

This project references many other open-source projects. See [here](https://github.com/QL-Win/QuickLook/wiki/On-the-Shoulders-of-Giants) for the full list.

All source codes are licensed under [GPL-3.0](https://opensource.org/licenses/GPL-3.0).

If you want to make any modification on these source codes while keeping new codes not protected by GPL-3.0, please contact me for a sublicense instead.

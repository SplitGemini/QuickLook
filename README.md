<img src="https://user-images.githubusercontent.com/1687847/82130498-8c3eac80-97d4-11ea-9e88-372ab9c50295.png" width="80">



### 3.6.5 -> 3.6.6 -> 3.6.7 -> 3.6.8 -> 3.6.9

以下都是基于最新QuickLook进行的修改
改进：
1. 后台任务栏图标添加隐藏选项，选择隐藏后之后开机自启不会再显示后台图标和通知，除非启动参数加"/setvisible"后。
通过安装包自动增加的桌面和开始菜单快捷方式默认带"/setvisible"参数，即运行桌面或开始菜单的快捷方式会立刻或下次启动时（当前有QuickLook在运行）出现任务栏图标
1. 修复预览.lnk快捷方式BUG （原版3.6.8版本也做了相同修改）
1. 修复当预览有多张封面的mp3时没有音频封面预览问题
1. VideoViewer增加rmvb和bik格式预览
1. ImageViewer增加dds,tga格式预览（3.6.8 更新支持tga了）
1. 将外部插件整合进解决方案里，包括FontViewer,EpubViewer,OfficeViewer-Native
1. 修改插件界面大小：FontViewer，OfficeViewer-Native，MarkdownViewer,PdfViewer,ImageViewer等
1. 修改FontViewer样例显示内容
1. QuickLook项目添加监控桌面文件夹功能选项，作用是把公用桌面文件全部移动到用户桌面中，这样桌面就能严格按字母排序了
1. 删除Native32和WoW64HookHelper项目，修改QuickLook、Native64、VideoViewer、PdfViewer,ImageViewer的文件，试图将QuickLook完全64位化，存在的问题：在64位系统运行的32位程序的dialog界面（选择文件或文件夹的弹窗，且必须是Windows提供的接口）无法开启预览，不过能遇到这情况的几率也很小
1. ImageViewer读取不出heif图片大小时使用系统方法（升级到3.6.8不需要了）
1. 更新MediaViewer里的MediaInfo和MediaInfo.Wrapper
1. 添加音乐界面歌词显示，优先读取同目录同名lrc（所有音频文件），其次读取内嵌歌词（仅mp3），只显示有时间轴歌词，将忽略时间差10毫秒的翻译
1. 文本预览和csv预览能够正确识别小文件的gbk或gb2312或其他编码，csv添加复制粘贴选项
1. 修复媒体预览音量问题，即媒体播放器的音量数值不平均，解决方法是音量条0-1的数值将对应实际音量的0.88-1
1. 删掉VideoViewer不需要的'IsPlaying'属性，修复Loop问题（暂停时改Loop设置会继续播放）






原QuickLook链接：[QL-Win/QuickLook](https://github.com/QL-Win/QuickLook)

## Licenses

![GPL-v3](https://www.gnu.org/graphics/gplv3-127x51.png)

This project references many other open-source projects. See [here](https://github.com/QL-Win/QuickLook/wiki/On-the-Shoulders-of-Giants) for the full list.

All source codes are licensed under [GPL-3.0](https://opensource.org/licenses/GPL-3.0).

If you want to make any modification on these source codes while keeping new codes not protected by GPL-3.0, please contact me for a sublicense instead.

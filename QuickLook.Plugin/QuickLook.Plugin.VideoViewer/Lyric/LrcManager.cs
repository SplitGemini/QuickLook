/* create by gh */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using QuickLook.Common.ExtensionMethods;

namespace QuickLook.Plugin.VideoViewer.Lyric
{
    class LrcManager
    {
        public List<LrcLine> LrcList = new List<LrcLine>();
        public static int offset = 0;

        public bool LoadFromFile(string filename)
        {
            LrcList.Clear();

            using (StreamReader sr = new StreamReader(filename, EncodingExtensions.GetEncoding(filename)))
            {
                return LoadFromText(sr.ReadToEnd());
            }
        }
        public bool LoadFromText(string text)
        {
            // 不论导入成功与否，均清空当前的显示
            LrcList.Clear();
            // 导入的内容为空
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }
            var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            // 在 Windows 平台，但是换行符只有 \n 而不是 \r\n
            if (lines.Length == 1 && text.Contains("\n"))
            {
                lines = text.Split('\n');
            }

            // 查找形如 [00:00.000] 的时间标记
            var reTimeMark = new Regex(@"\[\d+\:\d+\.\d+\]");
            // 查找形如 [al:album] 的歌词信息
            var reLrcInfo = new Regex(@"\[\w+\:.+\]");
            // 查找形如 [al:album] 的歌词信息
            var offLrcInfo = new Regex(@"\[offset\:.+\]");
            // 查找纯歌词文本
            var reLyric = new Regex(@"(?<=\])[^\]]+$");

            // 文本中不包含时间信息
            if (!reTimeMark.IsMatch(text))
            {
                foreach (var line in lines)
                {
                    // 即便是不包含时间信息的歌词文本，也可能出现歌词信息
                    if (reLrcInfo.IsMatch(line))
                    {
                        LrcList.Add(new LrcLine(null, line.Trim('[', ']')));
                    }
                    // 否则将会为当前歌词行添加空白的时间标记，即便当前行是空行
                    else
                        LrcList.Add(new LrcLine(0, line));
                }
            }
            // 文本中包含时间信息
            else
            {
                // 如果在解析过程中发现存在单行的多时间标记的情况，会在最后进行排序
                bool multiLrc = false;

                int lineNumber = 1;
                try
                {
                    foreach (var line in lines)
                    {
                        // 在确认文本中包含时间标记的情况下，会忽略所有空行
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            lineNumber++;
                            continue;
                        }

                        var matches = reTimeMark.Matches(line);
                        // 出现了类似 [00:00.000][00:01.000] 的包含多个时间信息的歌词行
                        if (matches.Count > 1)
                        {
                            var lrc = reLyric.Match(line).ToString();
                            foreach (var match in matches)
                            {
                                LrcList.Add(new LrcLine(TimeSpan.Parse("00:" + match.ToString().Trim('[', ']')), lrc));
                            }

                            multiLrc = true;
                        }
                        // 常规的单行歌词 [00:00.000]
                        else if (matches.Count == 1)
                        {
                            LrcList.Add(LrcLine.Parse(line));
                        }
                        // 说明这是一个偏移
                        else if (offLrcInfo.IsMatch(line))
                        {
                            int.TryParse(offLrcInfo.Match(line).ToString().Trim('[', ']').Substring(7), out offset);
                        }
                        // 说明这是一个歌词信息行
                        else if (reLrcInfo.IsMatch(line))
                        {
                            LrcList.Add(new LrcLine(null, reLrcInfo.Match(line).ToString().Trim('[', ']')));
                        }
                        // 说明正常的歌词里面出现了一个不是空行，却没有时间标记的内容，则添加空时间标记
                        else
                        {
                            LrcList.Add(new LrcLine(TimeSpan.Zero, line.Trim('[', ']')));
                            Console.WriteLine("row line: " + line);
                        }
                        lineNumber++;
                    }
                    // 如果出现单行出现多个歌词信息的情况，所以进行排序
                    if (multiLrc)
                        LrcList = LrcList.OrderBy(x => x.LrcTime).ToList();
                }
                catch
                {
                    LrcList.Clear();
                    return false;
                }
            }
            return LrcList.Count != 0;
        }


        public LrcManager()
        {
            
        }


        /// <summary>
        /// 获取当前时间对应的当前歌词
        /// </summary>
        public string GetNearestLrc(long t)
        {
            TimeSpan time = new TimeSpan(t);
            var list = LrcList
                .Where(x => x.LrcTime != null)
                .Where(x => x.LrcTime <= time)
                .OrderBy(x => x.LrcTime)
                .Reverse()
                .Select(x => x.LrcText)
                .ToList();
            if (list.Count > 0) return list[0];
            else return string.Empty;
        }

    }

}

/* create by gh */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickLook.Plugin.VideoViewer.Lyric
{
    public class LrcLine
    {
        // https://en.wikipedia.org/wiki/LRC_(file_format)

        public TimeSpan? LrcTime { get; set; }


        public string LrcText { get; set; }

        public LrcLine(double time, string text)
        {
            LrcTime = new TimeSpan(0, 0, 0, 0, ((int)(time * 1000) + LrcManager.offset) < 0 ? 0 : ((int)(time * 1000) + LrcManager.offset));
            LrcText = text;
        }
        public LrcLine(TimeSpan? time, string text)
        {
            if (time.HasValue)
            {
                LrcTime = time + new TimeSpan(0, 0, 0, 0, LrcManager.offset);
                if (TimeSpan.Compare(LrcTime.Value, TimeSpan.Zero) < 0)
                {
                    LrcTime = TimeSpan.Zero;
                }
            }
            LrcText = text;
        }

        public LrcLine()
        {
            LrcTime = null;
            LrcText = string.Empty;
        }

        public static LrcLine Parse(string line)
        {
            // 歌曲信息|[al:album]      | Time = null, Content = Info
            // 空白行  |                | Time = null, Content = empty
            // 正常歌词|[00:00.000]Info | Time = time, Content = content
            // 空白歌词|[00:00.000]     | Time = time, Content = empty
            // 多行歌词|[00:00.000][00:01.000]Info

            // 判断是否为空白行
            if (string.IsNullOrWhiteSpace(line))
            {
                return Empty;
            }
            // 这里不考虑多行歌词的情况
            if (CheckMultiLine(line)) throw new FormatException();

            // 此时只能为正常歌词
            var slices = line.TrimStart().TrimStart('[').Split(']');
            if (slices.Length != 2) throw new FormatException();

            // 如果方括号中的内容无法转化为时间，则认为是歌曲信息
            if (!TryParseTimeSpan(slices[0], out TimeSpan time))
            {
                return new LrcLine(null, slices[0]);
            }

            // 正常歌词和空白歌词不需要进行额外区分
            return new LrcLine(time, slices[1]);

        }

        static public bool TryParseTimeSpan(string s, out TimeSpan t)
        {
            try
            {
                t = TimeSpan.Parse("00:" + s); ;
                return true;
            }
            catch
            {
                t = TimeSpan.Zero;
                return false;
            }
        }

        public static bool TryParse(string line, out LrcLine lrcLine)
        {
            try
            {
                lrcLine = Parse(line);
                return true;
            }
            catch
            {
                lrcLine = Empty;
                return false;
            }
        }

        public static readonly LrcLine Empty = new LrcLine();

        /// <summary>
        /// 判断是否为多行歌词
        /// </summary>
        public static bool CheckMultiLine(string line)
        {
            // 指的是从左侧第二个字符开始找，如果仍然能够找到“[”，则认为包含超过一个时间框
            if (line.TrimStart().IndexOf('[', 1) != -1) return true;
            else return false;
        }

        public int CompareTo(LrcLine other)
        {
            // 保证歌曲信息永远在最开头
            if (!LrcTime.HasValue) return -1;
            if (!other.LrcTime.HasValue) return 1;
            // 正常的歌词按照时间顺序进行排列
            return LrcTime.Value.CompareTo(other.LrcTime.Value);
        }
    }
}

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
        readonly int offset;

        public string LrcText { get; set; }
        public LrcLine(TimeSpan? time, string text, int offset = 0)
        {
            if (time.HasValue)
            {
                this.offset = offset;
                LrcTime = time + new TimeSpan(0, 0, 0, 0, offset);
                if (TimeSpan.Compare(LrcTime.Value, TimeSpan.Zero) < 0)
                {
                    LrcTime = TimeSpan.Zero;
                }
            }
            LrcText = text;
        }

        public static LrcLine Parse(string line, int offset)
        {
            // 只考虑正常歌词
            // 正常歌词|[00:00.000]Info | Time = time, Content = content
            // 空白歌词|[00:00.000]     | Time = time, Content = empty

            // 此时只能为正常歌词
            var slices = line.TrimStart().TrimStart('[').Split(']');
            if (slices.Length != 2) throw new FormatException();

            // 如果方括号中的内容无法转化为时间，则认为是歌曲信息
            if (!TryParseTimeSpan(slices[0], out TimeSpan time))
            {
                return new LrcLine(null, slices[0]);
            }

            // 正常歌词和空白歌词不需要进行额外区分
            return new LrcLine(time, slices[1], offset);

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

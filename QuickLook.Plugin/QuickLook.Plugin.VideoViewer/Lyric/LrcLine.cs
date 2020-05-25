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
    }
}

/* create by gh */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QuickLook.Plugin.VideoViewer.Lyric
{
    class LrcManager
    {
        public List<LrcLine> LrcList = new List<LrcLine>();
        public static int offset = 0;

        public bool LoadFromFile(string filename)
        {
            LrcList.Clear();

            using (StreamReader sr = new StreamReader(filename, GetEncoding(filename)))
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


        /// <summary>
        /// 判断读入文本的编码格式
        /// 来自：http://stackoverflow.com/questions/1025332/determine-a-strings-encoding-in-c-sharp
        /// </summary>
        public static Encoding GetEncoding(string filename, int taster = 1000)
        {
            // Function to detect the encoding for UTF-7, UTF-8/16/32 (bom, no bom, little
            // & big endian), and local default codepage, and potentially other codepages.
            // 'taster' = number of bytes to check of the file (to save processing). Higher
            // value is slower, but more reliable (especially UTF-8 with special characters
            // later on may appear to be ASCII initially). If taster = 0, then taster
            // becomes the length of the file (for maximum reliability). 'text' is simply
            // the string with the discovered encoding applied to the file.
            string text;
            byte[] b = File.ReadAllBytes(filename);

            //////////////// First check the low hanging fruit by checking if a
            //////////////// BOM/signature exists (sourced from http://www.unicode.org/faq/utf_bom.html#bom4)
            if (b.Length >= 4 && b[0] == 0x00 && b[1] == 0x00 && b[2] == 0xFE && b[3] == 0xFF) { text = Encoding.GetEncoding("utf-32BE").GetString(b, 4, b.Length - 4); return Encoding.GetEncoding("utf-32BE"); }  // UTF-32, big-endian 
            else if (b.Length >= 4 && b[0] == 0xFF && b[1] == 0xFE && b[2] == 0x00 && b[3] == 0x00) { text = Encoding.UTF32.GetString(b, 4, b.Length - 4); return Encoding.UTF32; }    // UTF-32, little-endian
            else if (b.Length >= 2 && b[0] == 0xFE && b[1] == 0xFF) { text = Encoding.BigEndianUnicode.GetString(b, 2, b.Length - 2); return Encoding.BigEndianUnicode; }     // UTF-16, big-endian
            else if (b.Length >= 2 && b[0] == 0xFF && b[1] == 0xFE) { text = Encoding.Unicode.GetString(b, 2, b.Length - 2); return Encoding.Unicode; }              // UTF-16, little-endian
            else if (b.Length >= 3 && b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF) { text = Encoding.UTF8.GetString(b, 3, b.Length - 3); return Encoding.UTF8; } // UTF-8
            else if (b.Length >= 3 && b[0] == 0x2b && b[1] == 0x2f && b[2] == 0x76) { text = Encoding.UTF7.GetString(b, 3, b.Length - 3); return Encoding.UTF7; } // UTF-7


            //////////// If the code reaches here, no BOM/signature was found, so now
            //////////// we need to 'taste' the file to see if can manually discover
            //////////// the encoding. A high taster value is desired for UTF-8
            if (taster == 0 || taster > b.Length) taster = b.Length;    // Taster size can't be bigger than the filesize obviously.


            // Some text files are encoded in UTF8, but have no BOM/signature. Hence
            // the below manually checks for a UTF8 pattern. This code is based off
            // the top answer at: http://stackoverflow.com/questions/6555015/check-for-invalid-utf8
            // For our purposes, an unnecessarily strict (and terser/slower)
            // implementation is shown at: http://stackoverflow.com/questions/1031645/how-to-detect-utf-8-in-plain-c
            // For the below, false positives should be exceedingly rare (and would
            // be either slightly malformed UTF-8 (which would suit our purposes
            // anyway) or 8-bit extended ASCII/UTF-16/32 at a vanishingly long shot).
            int i = 0;
            bool utf8 = false;
            while (i < taster - 4)
            {
                if (b[i] <= 0x7F) { i += 1; continue; }     // If all characters are below 0x80, then it is valid UTF8, but UTF8 is not 'required' (and therefore the text is more desirable to be treated as the default codepage of the computer). Hence, there's no "utf8 = true;" code unlike the next three checks.
                if (b[i] >= 0xC2 && b[i] <= 0xDF && b[i + 1] >= 0x80 && b[i + 1] < 0xC0) { i += 2; utf8 = true; continue; }
                if (b[i] >= 0xE0 && b[i] <= 0xF0 && b[i + 1] >= 0x80 && b[i + 1] < 0xC0 && b[i + 2] >= 0x80 && b[i + 2] < 0xC0) { i += 3; utf8 = true; continue; }
                if (b[i] >= 0xF0 && b[i] <= 0xF4 && b[i + 1] >= 0x80 && b[i + 1] < 0xC0 && b[i + 2] >= 0x80 && b[i + 2] < 0xC0 && b[i + 3] >= 0x80 && b[i + 3] < 0xC0) { i += 4; utf8 = true; continue; }
                utf8 = false; break;
            }
            if (utf8 == true)
            {
                text = Encoding.UTF8.GetString(b);
                return Encoding.UTF8;
            }


            // The next check is a heuristic attempt to detect UTF-16 without a BOM.
            // We simply look for zeroes in odd or even byte places, and if a certain
            // threshold is reached, the code is 'probably' UF-16.          
            double threshold = 0.1; // proportion of chars step 2 which must be zeroed to be diagnosed as utf-16. 0.1 = 10%
            int count = 0;
            for (int n = 0; n < taster; n += 2) if (b[n] == 0) count++;
            if (((double)count) / taster > threshold) { text = Encoding.BigEndianUnicode.GetString(b); return Encoding.BigEndianUnicode; }
            count = 0;
            for (int n = 1; n < taster; n += 2) if (b[n] == 0) count++;
            if (((double)count) / taster > threshold) { text = Encoding.Unicode.GetString(b); return Encoding.Unicode; } // (little-endian)


            // Finally, a long shot - let's see if we can find "charset=xyz" or
            // "encoding=xyz" to identify the encoding:
            for (int n = 0; n < taster - 9; n++)
            {
                if (
                    ((b[n + 0] == 'c' || b[n + 0] == 'C') && (b[n + 1] == 'h' || b[n + 1] == 'H') && (b[n + 2] == 'a' || b[n + 2] == 'A') && (b[n + 3] == 'r' || b[n + 3] == 'R') && (b[n + 4] == 's' || b[n + 4] == 'S') && (b[n + 5] == 'e' || b[n + 5] == 'E') && (b[n + 6] == 't' || b[n + 6] == 'T') && (b[n + 7] == '=')) ||
                    ((b[n + 0] == 'e' || b[n + 0] == 'E') && (b[n + 1] == 'n' || b[n + 1] == 'N') && (b[n + 2] == 'c' || b[n + 2] == 'C') && (b[n + 3] == 'o' || b[n + 3] == 'O') && (b[n + 4] == 'd' || b[n + 4] == 'D') && (b[n + 5] == 'i' || b[n + 5] == 'I') && (b[n + 6] == 'n' || b[n + 6] == 'N') && (b[n + 7] == 'g' || b[n + 7] == 'G') && (b[n + 8] == '='))
                    )
                {
                    if (b[n + 0] == 'c' || b[n + 0] == 'C') n += 8; else n += 9;
                    if (b[n] == '"' || b[n] == '\'') n++;
                    int oldn = n;
                    while (n < taster && (b[n] == '_' || b[n] == '-' || (b[n] >= '0' && b[n] <= '9') || (b[n] >= 'a' && b[n] <= 'z') || (b[n] >= 'A' && b[n] <= 'Z')))
                    { n++; }
                    byte[] nb = new byte[n - oldn];
                    Array.Copy(b, oldn, nb, 0, n - oldn);
                    try
                    {
                        string internalEnc = Encoding.ASCII.GetString(nb);
                        text = Encoding.GetEncoding(internalEnc).GetString(b);
                        return Encoding.GetEncoding(internalEnc);
                    }
                    catch { break; }    // If C# doesn't recognize the name of the encoding, break.
                }
            }


            // If all else fails, the encoding is probably (though certainly not
            // definitely) the user's local codepage! One might present to the user a
            // list of alternative encodings as shown here: http://stackoverflow.com/questions/8509339/what-is-the-most-common-encoding-of-each-language
            // A full list can be found using Encoding.GetEncodings();
            text = Encoding.Default.GetString(b);
            return Encoding.Default;
        }

    }

}

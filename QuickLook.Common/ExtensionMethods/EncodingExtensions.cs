using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UtfUnknown;

namespace QuickLook.Common.ExtensionMethods
{
    public static class EncodingExtensions
    {
        /// <summary>
        /// 判断读入文本的编码格式
        /// 
        /// </summary>
        public static Encoding GetEncoding(string filename, int taster = 1000)
        {
            /* 改为默认uf8奇葩方法保留
            //unix 可能是识别失败，使用奇葩方法，常见代码默认utf-8
            //黑名单自动识别
            string[] black_list = { ".txt" };
            if(!black_list.Any(filename.ToLower().EndsWith))
            {
                return Encoding.UTF8;
            }
            */
            var encoding = Encoding.Default;
            var buffer = new MemoryStream();
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                while (fs.Position < fs.Length && buffer.Length < taster)
                {
                    var lb = new byte[8192];
                    int len = fs.Read(lb, 0, lb.Length);
                    buffer.Write(lb, 0, len);
                }
            }
            var bufferCopy = buffer.ToArray();
            buffer.Dispose();
            var Detected = CharsetDetector.DetectFromBytes(bufferCopy).Detected;
            Debug.WriteLine("Confidence = " + Detected?.Confidence);
            if (Detected?.Confidence > 0.5)
            {
                encoding = Detected.Encoding ?? Encoding.UTF8;
                Debug.WriteLine("UTF-UNKNOWN Charset = " + encoding.EncodingName);
                return Detected.Encoding;
            } else return Encoding.UTF8;
        }
        public static Encoding GetEncoding(byte[] buffer)
        {
            return CharsetDetector.DetectFromBytes(buffer).Detected?.Encoding ?? Encoding.UTF8;
        }
    }
    
}

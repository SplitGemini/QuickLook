using NChardet;
using System.Diagnostics;
using System.IO;
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
            
            var encoding = Encoding.Default;
            Detector detect = new Detector();
            CharsetDetectionObserver cdo = new CharsetDetectionObserver();
            detect.Init(cdo);
            var buffer = new MemoryStream();
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var lb = new byte[8192];
                bool done = false;
                bool isAscii = true;
                while (fs.Position < fs.Length && buffer.Length < taster)
                {
                    int len = fs.Read(lb, 0, lb.Length);
                    buffer.Write(lb, 0, len);
                    // 探测是否为 Ascii 编码
                    if (isAscii)
                        isAscii = detect.isAscii(lb, len);

                    // 如果不是 Ascii 编码，并且编码未确定，则继续探测
                    if (!isAscii && !done)
                        done = detect.DoIt(lb, len, false);
                    //if (done)
                        //break;
                }
            }
            detect.DataEnd();
            var Detected = CharsetDetector.DetectFromStream(buffer).Detected;
            Debug.WriteLine("Confidence = " + Detected?.Confidence);
            if (Detected?.Confidence > 0.5)
            {
                encoding = Detected.Encoding ?? Encoding.Default;
                Debug.WriteLine("UTF-UNKNOWN Charset = " + encoding.EncodingName);
                return Detected.Encoding;
            }
            string[] prob = detect.getProbableCharsets();
            if (prob.Length > 0)
            {
                try {
                    encoding = Encoding.GetEncoding(prob[0]);
                    foreach(var str in prob)
                    {
                        Debug.WriteLine("NChardet Probable Charset = " + str);
                    }
                }
                catch
                {
                    encoding = Encoding.Default;
                }
                
            }
            buffer.Dispose();
            return encoding;
        }
        public static Encoding GetEncoding(byte[] buffer)
        {
            return CharsetDetector.DetectFromBytes(buffer).Detected?.Encoding ?? Encoding.Default;
        }
    }
    public class CharsetDetectionObserver : ICharsetDetectionObserver
    {
        public string Charset = null;

        public void Notify(string charset)
        {
            Charset = charset;
        }
    }
    
}

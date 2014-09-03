using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Omniaudio.Helpers
{
    public sealed class Logger
    {
        private static readonly Lazy<Logger> logger =
          new Lazy<Logger>(() => new Logger());

        public static Logger Instance { get { return logger.Value; } }

        private StringBuilder sb;
        private string path;
        private string fileName;
        
        private Logger()
        {
            sb = new StringBuilder();
            path = null;
            fileName = null;
        }
        public void Log(string fileName, string msg)
        {
            path = Environment.CurrentDirectory + "\\" + fileName + ".log";
            this.fileName = fileName;
            sb.Append(msg + Environment.NewLine);
        }

        public void Flush()
        {
            File.AppendAllText(path, sb.ToString());
            sb.Clear();
        }
    }
}

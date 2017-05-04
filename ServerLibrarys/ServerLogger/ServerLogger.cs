using Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServerLogger
{
    public class ServerLogger:AbstractLogger
    {
        private bool doConsolePrint = false;
        private bool infoConsolePrint = false;
        private bool warnConsolePrint = true;
        private bool errorConsolePrint = true;
        private bool doFilePrint = false;

        private string prefix;
        private string logFileName="";
        private string Logo = "FR->";
        private string baseDir = "C:";
        private StreamWriter tw;

        public ServerLogger(string logFilePath)
        {
            this.baseDir = logFilePath;
        }

        public void Init(string prefix, bool infoConsolePrint, bool warnConsolePrint, bool errorConsolePrint, bool doFilePrint)
        {
            this. doConsolePrint = false;
            this.infoConsolePrint = infoConsolePrint;
            this.warnConsolePrint = warnConsolePrint;
            this.errorConsolePrint = errorConsolePrint;
            this.doFilePrint = doFilePrint;
            this.prefix = prefix;
            //we create a new log file every time we run the app
            logFileName = GetSaveFileName(prefix);
            //Create a new Writer ande open the file;
            tw = new StreamWriter(logFileName);
            tw.AutoFlush = false;
        }

        private string GetSaveFileName(string prefix)
        {
            string path =string.Format("{0}{1}{2}",baseDir,DateTime.Now.ToString("yyyy_MM_dd"),"/"); 
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception e)
            {
                LOG.Warn("Could not create save directory for log/ See Logger.cs.");
                LOG.Error("{0}",e.ToString());
            }
            string assemblyFullName = Assembly.GetExecutingAssembly().FullName;
            Int32 index = assemblyFullName.IndexOf(',');
            string dt = string.Format("{0}{1}", "", DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss"));
            return string.Format("{0}{1}{2}{3}{4}", path, prefix,"_",dt,".txt");
        }



        public override void Write(object obj)
        {
            throw new NotImplementedException();
        }

        public override void WriteLine(object obj)
        {
            throw new NotImplementedException();
        }

        public override void Info(object obj)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                string info = string.Format("{0}{1}{2}{3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff "), Logo, "[info] ", obj);
                Console.WriteLine(info);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public override void InfoLine(object obj)
        {
            Info(obj);
        }

        public override void Error(object obj)
        {
            throw new NotImplementedException();
        }

        public override void ErrorLine(object obj)
        {
            throw new NotImplementedException();
        }

        public override void Warn(object obj)
        {
            throw new NotImplementedException();
        }

        public override void WarnLine(object obj)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            throw new NotImplementedException();
        }
    }
}

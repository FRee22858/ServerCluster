using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logger
{
    class DeafaultLogger:AbstractLogger
    {
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
            throw new NotImplementedException();
        }

        public override void InfoLine(object obj)
        {
            throw new NotImplementedException();
        }

        public override void Error(object obj)
        {
            try
            {
                string info = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " [Error]" + obj;
                Console.ForegroundColor =ConsoleColor.Red;
                Console.WriteLine(info);
                Console.ForegroundColor =ConsoleColor.White;
            }
            catch (Exception e)
            {
                Console.ForegroundColor =ConsoleColor.Red;
                Console.WriteLine(e.ToString());
                Console.ForegroundColor =ConsoleColor.White;
            }
        }

        public override void ErrorLine(object obj)
        {
            Error(obj);
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

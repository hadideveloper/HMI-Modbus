using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace HardwareInterface
{
    public enum LogStat
    {
        ERRROR,
        WARNNING,
        INFO,
        DEBUG
    }

    public class Logger
    {
        private static Logger _instance = null;
        private StreamWriter mStream;
        private string[] ERRORS_NAME = new string[] {"E" , "W" , "I" , "D" };

        private ReaderWriterLockSlim mLock = new ReaderWriterLockSlim();

        public static Logger Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Logger();

                return _instance;
            }
        }

        private Logger()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fileName = DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";

            mStream = new StreamWriter(path + "\\" + fileName, false);
            mStream.AutoFlush = true;
        }

        public void Add(string className, string functionName, string message, LogStat stat)
        {
            mLock.EnterWriteLock();
            try
            {
                mStream.WriteLine($"{ERRORS_NAME[(int)stat]}  |  {DateTime.Now.ToString()}  |  {className}  |  {functionName}  |  {message}");
            }
            finally
            {
                mLock.ExitWriteLock();
            }
        }

        public void AddError(object sender, string functionName, string message)
        {
            Add(sender.GetType().Name, functionName, message, LogStat.ERRROR);
        }

        public void AddWarnning(object sender, string functionName, string message)
        {
            Add(sender.GetType().Name, functionName, message, LogStat.WARNNING);
        }

        public void AddInfo(object sender, string functionName, string message)
        {
            Add(sender.GetType().Name, functionName, message, LogStat.INFO);
        }

        public void AddDebug(object sender, string functionName, string message)
        {
            Add(sender.GetType().Name, functionName, message, LogStat.DEBUG);
        }

        public void Close()
        {
            if( mStream != null)
            {
                mStream.Flush();
                mStream.Close();
                mStream.Dispose();
                mStream = null;
            }
        }
    }
}
using log4net;
using log4net.Appender;
using System;
using System.IO;

namespace Machine
{
    class Log
    {
        private static string filepath = AppDomain.CurrentDomain.BaseDirectory + @"..\Log\";

        private static readonly log4net.ILog logComm = log4net.LogManager.GetLogger("AppLog");

        static Log()
        {
            log4net.Config.XmlConfigurator.Configure(new FileInfo("log.config"));

            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }
        }

        /// <summary>
        /// 输出系统日志
        /// </summary>
        /// <param name="msg">信息内容</param>
        /// <param name="source">信息来源</param>
        private static void WriteLog(string msg, Action<object> action)
        {
            string filename = DateTime.Now.ToString("D") + ".log";
            var repository = LogManager.GetRepository();
            var appenders = repository.GetAppenders();
            if (appenders.Length > 0)
            {
                RollingFileAppender targetApder = null;
                foreach (var Apder in appenders)
                {
                    if (Apder.Name == "AppLog")
                    {
                        targetApder = Apder as RollingFileAppender;
                        break;
                    }
                }
                if (targetApder != null)
                {
                    if (!targetApder.File.Contains(filename))
                    {
                        targetApder.File = @"SysLog\" + filename;
                        targetApder.ActivateOptions();
                    }
                }
            }
            action(msg);
        }
        public static void WriteError(string msg)
        {
            WriteLog(msg, logComm.Error);
        }
        public static void WriteInfo(string msg)
        {
            WriteLog(msg, logComm.Info);
        }
        public static void WriteWarn(string msg)
        {
            WriteLog(msg, logComm.Warn);
        }
    }
}

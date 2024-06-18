using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBLKoWizard.Utlis
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Empty
    }

    public class EventLogger
    {
        private string logDirectoryPath;
        private string logFileName;

        public EventLogger(string logDirectoryPath, string logFileName)
        {
            this.logDirectoryPath = logDirectoryPath;

            if (!Directory.Exists(logDirectoryPath))
            {
                Directory.CreateDirectory(logDirectoryPath);
            }

            this.logFileName = logFileName;
        }

        public void LogEvent(string eventName, LogLevel level)
        {
            try
            {
                DateTime currentTime = DateTime.Now;
                string logFilePath = Path.Combine(logDirectoryPath, logFileName);

                string prefix;

                switch (level)
                {
                    case LogLevel.Info:
                        prefix = "INFO";
                        break;
                    case LogLevel.Warning:
                        prefix = "WARNING";
                        break;
                    case LogLevel.Error:
                        prefix = "ERROR";
                        break;
                    case LogLevel.Empty:
                        prefix = "";
                        break;
                    default:
                        prefix = "LOG";
                        break;
                }
                string logMessage = "";
                if (level != LogLevel.Empty)
                {
                    logMessage = $"{currentTime:yyyy-MM-dd HH:mm:ss} [{prefix}] - {eventName}";
                } else
                {
                    logMessage = eventName;
                }

                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine(logMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }
    }
}

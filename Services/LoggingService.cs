using System;
using System.IO;


namespace redmine_notifier.Services
{

    public class LoggingService
    {

        private static readonly string LogFilePath = "./log.txt";
        public LoggingService()
        {
            if (File.Exists(LogFilePath))
            {
                File.Delete(LogFilePath);
            }

        }



        public void Log(string message)
        {
            try
            {
                File.AppendAllText("./log.txt", message);
            } catch (Exception e)
            {
                
            }
        }
    }
}
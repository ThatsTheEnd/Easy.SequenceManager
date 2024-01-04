using log4net.Config;
using log4net;
using System.Reflection;

namespace Easy.SequenceManager
{
    public static class LogHelper
    {
        public static void ConfigureLog4Net()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetExecutingAssembly());
            XmlConfigurator.Configure(logRepository, new System.IO.FileInfo("log4net.config"));
        }
    }

}

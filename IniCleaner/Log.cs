using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: log4net.Config.DOMConfigurator(ConfigFileExtension = "config", Watch = true)]
namespace IniCleaner
{
    public class Log
    {
        ILog logger = log4net.LogManager.GetLogger("Ini");

        #region singleon
        public static Log singleon;
        private Log()
        {

        }

        public static Log GetInstance
        {
            get
            {
                if (singleon == null)
                {
                    object root = new object();
                    lock (root)
                    {
                        if (singleon == null)
                        {
                            singleon = new Log();
                        }
                    }
                }
                return singleon;
            }
        }
        #endregion singleon

        public void Info(string source)
        {
            logger.Error(source);
        }
    }
}

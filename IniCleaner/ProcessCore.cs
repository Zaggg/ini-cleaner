using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IniCleaner
{
    public class ProcessCore
    {
        public void Go()
        {
            try
            {
                Console.WriteLine("start to work...");
                var iniPath = ConfigurationManager.AppSettings["IniPath"];
                Console.WriteLine("back up...");
                ConfigReader.GetInstance.FileBackUp(iniPath, iniPath.Replace(".ini", "-backup.ini"));
                Console.WriteLine("read ini...");
                var dic = InheritTreeTool.GetInstance.GetNewIniDictionary(InheritTreeTool.GetInstance.BuildInheritTree(iniPath));
                Console.WriteLine("write new ini...");
                ConfigReader.GetInstance.WriteIniFile(iniPath, dic);
                Console.WriteLine("process end,press any key to quit...");
                Console.ReadKey();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }


    }
}

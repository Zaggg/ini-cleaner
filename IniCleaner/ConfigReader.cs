using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IniCleaner
{
    public class ConfigReader
    {
        #region singleon
        public static ConfigReader singleon;
        private ConfigReader()
        {

        }

        public static ConfigReader GetInstance
        {
            get
            {
                if (singleon == null)
                {
                    object root = new object();
                    lock(root)
                    {
                        if (singleon == null)
                        {
                            singleon = new ConfigReader();
                        }
                    }
                }
                return singleon;
            }
        }
        #endregion singleon
        public List<string> GetAllFileName(string path)
        {
            //TODO: remove hard coding here
            var fileNames = Directory.GetFiles(path, "*.cpp");
            return fileNames.ToList();
        }

        public bool WriteIniFile(string filePath, Dictionary<string, WeaponIniModel> dic)
        {
            try
            {
                var writer = new StreamWriter(filePath);
                foreach (var pair in dic)
                {
                    if(pair.Value.Data.Count>0)
                    {
                        writer.WriteLine("[" + pair.Value.FolderName + "." + pair.Key + "]");
                        foreach (var kv in pair.Value.Data)
                        {
                            writer.WriteLine(kv.Key + "=" + kv.Value);
                        }
                        writer.WriteLine();
                    }
                }
                writer.Close();
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            //return false;
        }

        public List<string> ReadIniLine(string filePath)
        {
            List<string> result = new List<string>();
            try
            {
                var lines = File.ReadLines(filePath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    if (line.Trim().StartsWith("//") || line.Trim().StartsWith("/") || line.Trim().StartsWith("*"))
                        continue;
                    result.Add(line);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
            return result;
        }

        public string QueryIniFile(string filePath)
        {
            return null;
        }

        //only read until claiming declaration for optimization
        public string ReadClassFile(string fileName,out bool shouldRemove)
        {
            string firstLine = null;
            shouldRemove = false;
            try
            {
                var lines = File.ReadLines(fileName);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    if (line.Trim().StartsWith("//") || line.Trim().StartsWith("/") || line.Trim().StartsWith("#") || line.Trim().StartsWith("*"))
                        continue;
                    if (line.Trim().StartsWith("class", StringComparison.InvariantCultureIgnoreCase) || line.Trim().StartsWith("extends", StringComparison.InvariantCultureIgnoreCase))
                    {
                        firstLine +=" " + line;
                        if(!line.Trim().ToLowerInvariant().Contains("extends"))
                        {
                            continue;
                        }
                        firstLine = firstLine.Trim();
                        break;
                    }
                    else
                        continue;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("file read fail:" + fileName);
                Log.GetInstance.Info(ex.Message);
                Log.GetInstance.Info("file read fail:" + fileName);
                shouldRemove = true;
                //throw ex;
            }
            
            return firstLine;
        }

        public void FileBackUp(string originFile,string newPath)
        {
            FileInfo file = new FileInfo(originFile);
            if(file.Exists)
            {
                file.CopyTo(newPath, true);
            }
        }

        public List<string> ReadPath()
        {
            return null;
        }
    }
}

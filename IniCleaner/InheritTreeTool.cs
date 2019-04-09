using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IniCleaner
{
    public class InheritTreeTool
    {
        #region singleon
        public static InheritTreeTool singleon;
        private InheritTreeTool()
        {

        }

        public static InheritTreeTool GetInstance
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
                            singleon = new InheritTreeTool();
                        }
                    }
                }
                return singleon;
            }
        }
        #endregion  singleon

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public Dictionary<string, WeaponIniModel> BuildInheritTree(string IniPath)
        {
            Dictionary<string, WeaponIniModel> dic = new Dictionary<string, WeaponIniModel>();
            try
            {
                //read ini first then query file won't build a complete inherit tree at all,but can be used to check useless config
                //dic = IniParser(ConfigReader.GetInstance.ReadIniLine(IniPath));
                //QueryBaseClass(ref dic);


                //scan all class version first
                dic = ScanAndSaveInheritTreeInDictionary();
                IniParserUseExistDic(ConfigReader.GetInstance.ReadIniLine(IniPath), ref dic);

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
            return dic;
        }

        public Dictionary<string, WeaponIniModel> GetNewIniDictionary(Dictionary<string, WeaponIniModel> dic)
        {
            //List<string> newLines = new List<string>();
            Dictionary<string, WeaponIniModel> newDic = new Dictionary<string, WeaponIniModel>();
            foreach (var pair in dic)
            {
                //newLines.Clear();
                List<KVModel> kvList = new List<KVModel>();
                kvList = pair.Value.Data;
                //newLines.AddRange(pair.Value.Data.Select(o => o.Value));

                var currentKey = pair.Value.BaseClassName;
                while (!string.IsNullOrEmpty(currentKey) && dic.ContainsKey(currentKey))
                {
                    //var sameLine = kvList.FindAll(o => dic[currentKey].Data.Exists(p => p.Value == o.Value && o.Key == o.Key));
                    kvList.RemoveAll(o => dic[currentKey].Data.Exists(p => p.Value == o.Value && o.Key == o.Key));//this version cannot detect dynamic array,try function below which have not been tested
                    //kvList.RemoveAll(o => dic[currentKey].Data.Exists(p => p.Value == o.Value && o.Key == o.Key) && dic[currentKey].Data.GroupBy(x=>x.Key).Where(x=>x.Count() == 1).SingleOrDefault() != null);
                    //kvList.Except(dic[currentKey].Data,new Func<KVModel,KVModel,bool>((KVModel a,KVModel b) => { return a.Key == b.Key && a.Value == b.Value; }));
                    currentKey = dic[currentKey].BaseClassName;
                }

                //foreach(var line in kvList)
                //{

                //}
                //kvList.ForEach(o => WritePrivateProfileString(pair.Value.FolderName + "." + pair.Key.ToString(), o.Key, o.Value, ""));
                //newLines.AddRange()
                newDic.Add(pair.Key, new WeaponIniModel() { Data = kvList,FolderName = pair.Value.FolderName });
            }
            return newDic;
        }

        public Dictionary<string, WeaponIniModel> ScanAndSaveInheritTreeInDictionary()
        {
            Dictionary<string, WeaponIniModel> dic = new Dictionary<string, WeaponIniModel>();
            try
            {
                Dictionary<string, List<string>> pathDic = new Dictionary<string, List<string>>();
                var classRootPath = ConfigurationManager.AppSettings["ClassPath"];
                pathDic.Add("FolderName", ConfigReader.GetInstance.GetAllFileName(string.Format(classRootPath, "FolderName")));
                bool noUse;
                foreach (var pair in pathDic)
                {
                    foreach (var fileName in pair.Value)
                    {
                        var declarationLine = ConfigReader.GetInstance.ReadClassFile(fileName, out noUse);
                        if(declarationLine == null)
                        {
                            Log.GetInstance.Info("declaration read fail:" + fileName + " which may with no declaration");
                            continue;
                        }
                        var classNames = GetClassName(declarationLine);
                        if (classNames == null)
                        {
                            Log.GetInstance.Info("declaration read fail:" + fileName + " it may cause by comment not start by symbol *.");
                            continue;
                        }
                        if (!dic.ContainsKey(classNames.Item1))
                        {
                            dic.Add(classNames.Item1, new WeaponIniModel() { BaseClassName = classNames.Item2, FolderName = pair.Key });
                        }
                        else
                        {
                            dic[classNames.Item1].BaseClassName = classNames.Item2;
                            dic[classNames.Item1].FolderName = pair.Key;
                        }
                        //do not add base class in current iteration
                        //if (dic.ContainsKey(classNames.Item2))
                        //{
                        //    dic.Add(classNames.Item2, new WeaponIniModel());
                        //}
                    }

                }
            }
            catch(Exception ex)
            {

            }
            
            return dic;
        }
        [Obsolete]
        public void QueryBaseClass(ref Dictionary<string, WeaponIniModel> dic)
        {
            var classRootPath = ConfigurationManager.AppSettings["ClassPath"];
            var removeList = new List<string>();
            bool shouldRemove = false;
            foreach (var pair in dic)
            {
                var classPath = Path.Combine(string.Format(classRootPath,pair.Value.FolderName), pair.Key + ".cpp");
                var baseClassName = ConfigReader.GetInstance.ReadClassFile(classPath,out shouldRemove);
                if (baseClassName != null)
                    pair.Value.BaseClassName = GetBaseClassName(pair.Key,baseClassName);
                if(shouldRemove)
                {
                    removeList.Add(pair.Key);
                }
            }

            //remove useless config
            foreach(var name in removeList)
            {
                if(dic.ContainsKey(name))
                {
                    dic.Remove(name);
                }
            }

        }
        [Obsolete]
        public Dictionary<string, WeaponIniModel> IniParser(List<string> originLines)
        {
            Dictionary<string, WeaponIniModel> dic = new Dictionary<string, WeaponIniModel>();
            string currentKey = null;
            Regex pattern = new Regex("\\[(\\w+)\\.(\\w+)\\]");
            //WeaponIniModel data = null;// new WeaponIniModel();
            foreach (var line in originLines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;
                if (line.Trim().StartsWith("["))
                {
                    var data = new WeaponIniModel();
                    //data.Data = new List<KVModel>();
                    var result = pattern.Match(line);
                    if (result != null && result.Groups != null && result.Groups.Count > 2)
                    {
                        data.FolderName = result.Groups[1].Value.Trim();
                        currentKey = result.Groups[2].Value.Trim();
                        if(!dic.ContainsKey(result.Groups[2].Value.Trim()))
                            dic.Add(result.Groups[2].Value.Trim(),data);
                        else
                        {
                            Console.WriteLine("encounter same config:\n" + result.Groups[2].Value.Trim());// + "\nPlease check the ini file and retry later"
                            Log.GetInstance.Info("encounter same config:\n" + result.Groups[2].Value.Trim());
                            Console.WriteLine("name was changed from " + result.Groups[2].Value.Trim() +" to " + result.Groups[2].Value.Trim() + "2,please check it later");
                            Log.GetInstance.Info("name was changed from " + result.Groups[2].Value.Trim() + " to " + result.Groups[2].Value.Trim() + "2,please check it later");
                            dic.Add(result.Groups[2].Value.Trim()+"2", data);
                            currentKey = result.Groups[2].Value.Trim() + "2";
                        }
                    }    
                }
                if(line.Contains("="))
                {
                    var key = line.Substring(0,line.Trim().IndexOf('='));
                    var value = line.Substring(line.Trim().IndexOf('=') + 1);
                    dic[currentKey].Data.Add(new KVModel { Key = key, Value = value });
                }
            }
            return dic;
        }

        public Dictionary<string, WeaponIniModel> IniParserUseExistDic(List<string> originLines,ref Dictionary<string, WeaponIniModel> dic)
        {
            string currentKey = null;
            Regex pattern = new Regex("\\[(\\w+)\\.(\\w+)\\]");
            foreach (var line in originLines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;
                if (line.Trim().StartsWith("["))
                {
                    var result = pattern.Match(line);
                    if (result != null && result.Groups != null && result.Groups.Count > 2)
                    {
                        var data = new WeaponIniModel();
                        data.FolderName = result.Groups[1].Value.Trim();
                        currentKey = result.Groups[2].Value.Trim();
                        if (!dic.ContainsKey(result.Groups[2].Value.Trim()))
                        {
                            dic.Add(result.Groups[2].Value.Trim(), data);
                        }
                        else
                        {
                            if(dic[result.Groups[2].Value.Trim()].Data.Count>0)
                            {
                                Console.WriteLine("encounter same config:\n" + result.Groups[2].Value.Trim());
                                Log.GetInstance.Info("encounter same config:\n" + result.Groups[2].Value.Trim());
                                Console.WriteLine("name was changed from " + result.Groups[2].Value.Trim() + " to " + result.Groups[2].Value.Trim() + "2,please check it later");
                                Log.GetInstance.Info("name was changed from " + result.Groups[2].Value.Trim() + " to " + result.Groups[2].Value.Trim() + "2,please check it later");
                                dic.Add(result.Groups[2].Value.Trim() + "2", data);
                                currentKey = result.Groups[2].Value.Trim() + "2";
                            }
                        }
                        
                    }
                }
                if (line.Contains("="))
                {
                    var key = line.Substring(0, line.Trim().IndexOf('='));
                    var value = line.Substring(line.Trim().IndexOf('=') + 1);
                    dic[currentKey].Data.Add(new KVModel { Key = key, Value = value });
                }
            }
            return dic;
        }
        [Obsolete]
        public string GetBaseClassName(string className,string originString)
        {
            try
            {
                Regex pattern = new Regex(string.Format("class {0} extends (\\w+)", className), RegexOptions.IgnoreCase);
                var result = pattern.Match(originString);
                if (result != null && result.Groups != null && result.Groups.Count > 1)
                    return result.Groups[1].Value.Trim();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
            return null;
        }

        public Tuple<string,string> GetClassName(string originString)
        {
            try
            {
                Regex pattern = new Regex(string.Format(ConfigurationManager.AppSettings["ClassClaimRegex"]), RegexOptions.IgnoreCase);
                var result = pattern.Match(originString);
                if (result != null && result.Groups != null && result.Groups.Count > 2)
                    return new Tuple<string, string>(result.Groups[1].Value.Trim(), result.Groups[2].Value.Trim());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
            return null;
        }
    }
}

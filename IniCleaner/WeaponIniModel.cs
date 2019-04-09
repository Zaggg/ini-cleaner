using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IniCleaner
{
    public class WeaponIniModel
    {
        public WeaponIniModel()
        {
            Data = new List<KVModel>();
        }

        public string BaseClassName { get; set; }

        public string FolderName { get; set; }
        public List<KVModel> Data { get; set; }
    }
    public class KVModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}

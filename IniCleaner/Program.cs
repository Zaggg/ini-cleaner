using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IniCleaner
{
    public class Program
    {
        //store all class in a map,check each class in ini file and backtracking from current node to root node.
        //write result in a new ini
        static void Main(string[] args)
        {
            ProcessCore process = new ProcessCore();
            process.Go();
        }
    }
}

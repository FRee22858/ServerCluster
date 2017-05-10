using CommonUtility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibTest
{
    class Program
    {
        static void Main(string[] args)
        {
            
        }

        void InitData()
        {
            string[] files = Directory.GetFiles(PathMng.FullPathFromData("XML"), "*.xml", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                
            }

        }
    }
}

using CommonUtility;
using DataProperty;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibTest
{
    class Program
    {
        static void InitPath(string path = "")
        {
            string rootPath = string.Empty;
            if (string.IsNullOrEmpty(path))
            {
                DirectoryInfo tempPath = new DirectoryInfo(Application.StartupPath);
                if (tempPath.Parent.Exists)
                {
                    rootPath = tempPath.Parent.FullName;
                }
                else
                {
                    Console.WriteLine("Path is error!Please check the input path!");
                }
            }
            else
            {
                rootPath = path;
            }
            PathMng.SetPath(rootPath);
        }
        static void InitData()
        {
            string[] files = Directory.GetFiles(PathMng.FullPathFromData("XML"), "*.xml", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                if (DataListManager.Inst.Parse(file))
                {
                    //解析成功
                }
                else
                {
                    //解析失败
                    Console.WriteLine("Fail:{0}", file);
                }
            }
        }

        static void InitConfig()
        {
            Data globalData;
            DataList clusterConfig = DataListManager.Inst.GetDataList("ClusterConfig");
            if (clusterConfig != null)
            {
                globalData = clusterConfig.GetDataByName("GlobalServer");
            }
        }
        static void Main(string[] args)
        {
            InitPath();
            InitData();
            InitConfig();
        }
            
       
    }
}

using System;

using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

namespace BarcodeCollect
{
    class FileAct
    {
        public static Dictionary<String, String> TypeList()
        {
            Dictionary<String, String> ret = new Dictionary<string, string>();

            string[] type;

            StreamReader myStream = new StreamReader(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName) + "\\ItemType.txt",Encoding.UTF8);
            string stringLine = myStream.ReadLine();
            while (stringLine != null)
            {
                type = stringLine.Split(',');

                ret[type[0]] = type[1];

                stringLine = myStream.ReadLine();
            }
            myStream.Close();

            return ret;
        }

        public static int SaveToFile(string dirPath,string filename,Dictionary<String,String> ds)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(dirPath + filename))
                {
                    foreach(KeyValuePair<string,string>kvp in ds)
                    {
                        sw.WriteLine(kvp.Key + "," + kvp.Value);
                    }
                    sw.Close();
                    MessageBox.Show("文件保存成功，路径：" + dirPath + filename);
                    return -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return 1;
            }
        }
    }
}

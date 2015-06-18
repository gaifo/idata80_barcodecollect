using System;

using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using System.Xml;

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

                ret[type[0]] = type[1]+"_"+type[2];

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

        public static int SaveToXML(string dirPath, string filename, Dictionary<String, String> ds)
        {
            XmlDocument doc = new XmlDocument();

            XmlDeclaration xmldecl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);

            doc.AppendChild(xmldecl);

            XmlElement rootElement = doc.CreateElement("序列号");

            foreach (KeyValuePair<string, string> kvp in ds)
            {
                //sw.WriteLine(kvp.Key + "," + kvp.Value);

                string[] s = kvp.Value.Split('_');

                XmlElement line = doc.CreateElement("行");

                line.SetAttribute("存货编码", s[1]);
                line.SetAttribute("存货名称", s[0]);
                line.SetAttribute("序列号", kvp.Key);
                line.SetAttribute("备注", "");
                line.SetAttribute("序列号属性1", "");
                line.SetAttribute("序列号属性2", "");
                line.SetAttribute("序列号属性3", "");
                line.SetAttribute("序列号属性4", "");
                line.SetAttribute("序列号属性5", "");
                line.SetAttribute("序列号属性6", "");
                line.SetAttribute("序列号属性7", "");
                line.SetAttribute("序列号属性8", "");
                line.SetAttribute("序列号属性9", "");
                line.SetAttribute("序列号属性10", "");

                rootElement.AppendChild(line);

            }

            doc.AppendChild(rootElement);

            doc.Save(dirPath + filename);
            MessageBox.Show("文件保存成功，路径：" + dirPath + filename);
            return -1;
        }
    }
}

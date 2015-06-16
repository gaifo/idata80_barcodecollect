using System;

using System.Collections.Generic;
using System.Windows.Forms;

namespace BarcodeCollect
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [MTAThread]
        static void Main()
        {
            
            // 初始化 iScanDevLib.dll

            if (!DLL.Startup())
            {
                MessageBox.Show("Fail to startup iScanDevLib.dll", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                DLL.Shutdown();
                return;
            }
            
            try
            {
                Application.Run(new BarcodeCollect());
            }
            catch (Exception E)
            {
                // TODO: 根据实际情况处理这个异常
                MessageBox.Show(E.Message, "Exception");
            }
            finally
            {
                // 终止 iScanDevLib.dll 并 释放资源
                DLL.Shutdown();
            }
        }
    }
}
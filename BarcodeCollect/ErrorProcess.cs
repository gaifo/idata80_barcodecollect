using System;

using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace BarcodeCollect
{
    static class ErrorProcess
    {
        static public void ShowError(string Message, Exception ex)
        {
            string strMsg = string.Empty;
            if (Message.Length > 0)
                strMsg += "Message:\r\n" + Message + "\r\n";

            if (ex != null)
            {
                if (ex.InnerException == null)
                    strMsg += "Error:\r\n" + ex.Message + "\r\nStack:\r\n" + ex.StackTrace;
                else
                    strMsg += "Error:\r\n" + ex.InnerException.Message + "\r\nStack:\r\n" + ex.InnerException.StackTrace;
            }

            MessageBox.Show(strMsg);
        }
    }
}

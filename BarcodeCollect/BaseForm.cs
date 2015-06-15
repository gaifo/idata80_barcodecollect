using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.WindowsCE.Forms;

namespace BarcodeCollect
{
    public partial class BaseForm : Form
    {
        public BaseForm()
        {
            InitializeComponent();

            this.m_hBarCodeDev = IntPtr.Zero;
            this.m_hRFidDev = IntPtr.Zero;
            this.m_CurrentBarCodeType = BarCodeType.BARCODE_UNKNOWN;
            this.m_msgAgent = new CustomMessageHandler(this);
        }

        /// <summary>
        /// 重载这个函数实现当接收到条码内容时的处理过程
        /// </summary>
        public virtual void OnBarCodeNotify(byte[] BarCodeData, int nLength)
        {
            /* NOTICE：
             *
             * 1. 假如当前扫描设备扫描的条码是一维条码（CODE93、CODE128、UPC、EAN-8、EAN-13、ITF25...），
             *    Encoding.ASCII.GetString( BarCodeData, 0, nLength ) 可得到 string形式的条码内容。
             *
             * 2. 假如扫描的是二维条码（PDF417、QR、DataMatrix...），非ASCII字符（譬如简体中文）存在编码的问题，
             *    需要根据实际情况选用编码转换的CodePage参数，常用的可能是系统默认的 CP936/gb2312 或者 UTF8.
             *    Encoding.GetEncoding("gb2312").GetString(BarCodeData, 0, nLength ) 或
             *    Encoding.GetEncoding("UTF8").GetString(BarCodeData, 0, nLength )
             *    选择不正确的Encoding可能导致解码内容是乱码。
             *
             * 3. 调用GetCurrentBarCodeType() 可以获得当前条码的码制类型。
             *
             */
        }

        /// <summary>
        /// 重载这个函数实现当检测到有RFID标签出现在扫描域
        /// </summary>
        public virtual void OnRFidTagNotify(uint nTagIdLen)
        {
            /* NOTICE:
             *
             * 1. 每张标准的RFID标签都有一个由厂家在出厂前就固化好的唯一ID (TagId)，不能修改。
             *
             * 2. 假如需要知道当前检测到的RFID标签的唯一ID 或者 标签类型
             *    调用 GetRFidTagUinqueId( out byte[] TagId, ref int nLength, ref RFID_TAG_TYPE TagType )
             *
             * 3. 假如只是需要简单的读取当前RFID标签的唯一ID ( RFID TagId )
             *    调用 GetRFidTagUinqueId() 可获得 string形式的TID，事实上就是一个十六进制的字符串
             *
             * 4. 后续可以调用RFID相关函数进行读/写业务数据到RFID标签的UserData区域。
             *
             * !!! WARNING !!!
             *    由于 RFID模块 和 RFID标签 之间是通过无线电波传递数据，有可能读写失败，注意检查返回值 !!!
             */
        }

        // 设置在扫描解码成功后是否发出声音通知
        public void EnableBeeper(Boolean bEnable)
        {
            DLL.SetBeeperEnable(bEnable);
        }

        // 设置在扫描解码成功后是否允许马达震动通知
        public void EnableVibrater(Boolean bEnable)
        {
            DLL.SetVibrateEnable(bEnable);
        }

        // 获得当前条码的码制类型( CODE39, CODE128, EAN13 ... )
        public BarCodeType GetCurrentBarCodeType()
        {
            return this.m_CurrentBarCodeType;
        }

        public bool OpenBarCode()
        {
            if (this.m_hBarCodeDev != IntPtr.Zero)
                return true;

            Cursor.Current = Cursors.WaitCursor;
            Cursor.Show();

            this.m_hBarCodeDev = DLL.OpenBarCodeScanner(m_msgAgent.Hwnd, m_msgAgent.GetBarCodeNotifyMessageId());

            Cursor.Current = Cursors.Default;
            Cursor.Hide();

            return (this.m_hBarCodeDev != IntPtr.Zero) ? true : false;
        }

        public bool CloseBarCode()
        {
            if (this.m_hBarCodeDev == IntPtr.Zero)
                return true;

            if (DLL.CloseBarCodeScanner(this.m_hBarCodeDev))
            {
                this.m_hBarCodeDev = IntPtr.Zero;
                return true;
            }

            return false;
        }

        public string GetBarCodeDebugMessage()
        {
            if (this.m_hBarCodeDev != IntPtr.Zero)
            {
                int nLen = (int)DLL.DevLibGetDebugMessage(this.m_hBarCodeDev, null, 0);
                if (nLen > 0)
                {
                    StringBuilder _Builder = new StringBuilder(nLen + 1);
                    nLen = (int)DLL.DevLibGetDebugMessage(this.m_hBarCodeDev, _Builder, (uint)_Builder.Capacity);
                    return _Builder.ToString(0, nLen);
                }
            }

            return "";
        }

        // 打开BarCode设备后，临时禁用/启用控制扫描触发
        public void EnableBarCode(Boolean bEnable)
        {
            if (this.m_hBarCodeDev != IntPtr.Zero)
            {
                DLL.DevLibEnableBarCodeScanner(this.m_hBarCodeDev, bEnable);
            }
        }

        // 获取条码扫描设备发送过来的条码编码原始内容
        // 一般不需要主动调用这个函数，因为虚函数OnBarCodeNotify的参数已经传递带过去了。
        public bool GetBarCodeRawData(out byte[] BarCodeData, ref int nLength)
        {
            if (this.m_hBarCodeDev == IntPtr.Zero || nLength <= 0)
            {
                BarCodeData = null;
                nLength = 0;

                return false;
            }

            UInt32 dwRetVal = 0;
            UInt32 dwSizeRet = 0;
            UInt32 dwBufferSize = 64;   // 预留64个字节，对于一维条码来说完全绰绰有余了，这样可以不需要两次调用DevLibReadBarCode，提高效率

            BarCodeData = new byte[dwBufferSize];

            dwRetVal = DLL.DevLibReadBarCode(m_hBarCodeDev, BarCodeData, dwBufferSize, ref dwSizeRet);
            if (dwRetVal == 234 /*ERROR_MORE_DATA*/ )  // Buffer (64 bytes) not enough
            {
                dwBufferSize = (dwSizeRet / 32 + 1) * 32;   // aligned base 32
                BarCodeData = new byte[dwBufferSize];
                dwRetVal = DLL.DevLibReadBarCode(m_hBarCodeDev, BarCodeData, dwBufferSize, ref dwSizeRet);
            }

            if (dwRetVal == 0/*ERROR_SUCCESS*/ && dwSizeRet > 0)
            {
                nLength = (int)dwSizeRet;
                return true;
            }
            else
            {
                BarCodeData = null;
                nLength = 0;
                return false;
            }
        }


        public bool OpenRFID()
        {
            if (this.m_hRFidDev != IntPtr.Zero)
                return true;

            Cursor.Current = Cursors.WaitCursor;
            Cursor.Show();

            this.m_hRFidDev = DLL.DevLibRFidOpen(m_msgAgent.Hwnd, m_msgAgent.GetRFidTagNotifyMessageId(), 0);

            Cursor.Current = Cursors.Default;
            Cursor.Hide();

            return (this.m_hRFidDev != IntPtr.Zero) ? true : false;
        }

        public bool CloseRFID()
        {
            if (this.m_hRFidDev == IntPtr.Zero)
                return true;

            if (DLL.DevLibRFidClose(this.m_hRFidDev))
            {
                this.m_hRFidDev = IntPtr.Zero;
                return true;
            }

            return false;
        }

        // 打开RFID设备后，临时禁用/启用控制扫描触发
        // 注意：仅影响按键的触发控制，不影响RFID标签读写(TagReadString/TagWriteString ... )；
        public void EnableRFIDScan(Boolean bEnable)
        {
            if (this.m_hRFidDev != IntPtr.Zero)
            {
                DLL.DevLibEnableRFIDScan(this.m_hRFidDev, bEnable);
            }
        }

        public bool DetectTag()
        {
            if (this.m_hRFidDev == IntPtr.Zero)
                return false;

            return DLL.DevLibRFidDetectTag(this.m_hRFidDev);
        }

        // 读取RFID模块返回的响应值，有助于判断操作失败的原因
        public byte GetResponseCode()
        {
            if (this.m_hRFidDev == IntPtr.Zero)
                return 0;
            else
                return (byte)DLL.DevLibGetResponseCode(this.m_hRFidDev);
        }

        public string GetRFidDebugMessage()
        {
            if (this.m_hRFidDev != IntPtr.Zero)
            {
                int nLen = (int)DLL.DevLibGetDebugMessage(this.m_hRFidDev, null, 0);
                if (nLen > 0)
                {
                    StringBuilder _Builder = new StringBuilder(nLen + 1);
                    nLen = (int)DLL.DevLibGetDebugMessage(this.m_hRFidDev, _Builder, (uint)_Builder.Capacity);
                    return _Builder.ToString(0, nLen);
                }
            }

            return "";
        }

        public string GetRFidDataTransfer()
        {
            if (this.m_hRFidDev != IntPtr.Zero)
            {
                int nLen = (int)DLL.DevLibGetDataTransfer(this.m_hRFidDev, null, 0);
                if (nLen > 0)
                {
                    StringBuilder _Builder = new StringBuilder(nLen + 1);
                    nLen = (int)DLL.DevLibGetDataTransfer(this.m_hRFidDev, _Builder, (uint)_Builder.Capacity);
                    return _Builder.ToString(0, nLen);
                }
            }

            return "";
        }

        // 获取当前RFID标签的ID和类型
        public bool GetRFidTagUinqueId(out byte[] TagId, ref int nLength, ref RFID_TAG_TYPE TagType)
        {
            if (this.m_hRFidDev == IntPtr.Zero)
            {
                TagId = null;
                nLength = 0;
                TagType = RFID_TAG_TYPE.TAG_TYPE_UNKNOWN;
                return false;
            }

            UInt32 dwSizeRet = 0;
            UInt32 dwRetVal = 0;
            dwRetVal = DLL.DevLibReadTagId(m_hRFidDev, null, 0, ref dwSizeRet);
            if (dwSizeRet > 0)
            {
                UInt32 dwSize = (dwSizeRet / 32 + 1) * 32;   // 预留稍微大一点的空间
                TagId = new byte[dwSize];
                dwRetVal = DLL.DevLibReadTagId(m_hRFidDev, TagId, dwSize, ref dwSizeRet);
                if (dwRetVal > 0)
                {
                    TagType = (RFID_TAG_TYPE)TagId[0];
                    nLength = (int)(dwRetVal - 1);
                    // 第一个字节是TagType，处理一下方便后续的使用
                    for (int i = 0; i < nLength; i++)
                        TagId[i] = TagId[i + 1];

                    return true;
                }
            }

            TagId = null;
            nLength = 0;
            TagType = RFID_TAG_TYPE.TAG_TYPE_UNKNOWN;
            return false;
        }

        // 获取当前RFID标签的ID
        public string GetRFidTagUinqueId()
        {
            byte[] TagId;
            int nLength = 128;
            RFID_TAG_TYPE TagType = RFID_TAG_TYPE.TAG_TYPE_UNKNOWN;

            if (GetRFidTagUinqueId(out TagId, ref nLength, ref TagType))
            {
                StringBuilder _Builder = new StringBuilder((nLength + 1) * 2);
                for (int i = 0; i < nLength; i++)
                {
                    _Builder.Append(TagId[i].ToString("X2"));
                }

                return _Builder.ToString();
            }

            return "";
        }

        // 读取当前RFID标签里面的用户数据（char*形式，故最后一个字节是NULL终止符）
        public bool TagReadString(out byte[] UserData, ref int nLength)
        {
            if (this.m_hRFidDev == IntPtr.Zero)
            {
                UserData = null;
                nLength = 0;

                return false;
            }

            byte[] Buffer = new byte[512];
            int nSize = 512;
            if (DLL.DevLibTagReadString(this.m_hRFidDev, Buffer, nSize))
            {
                nLength = 0;
                while (Buffer[nLength++] != 0) ;

                UserData = new byte[nLength + 1];
                for (int i = 0; i < nLength; i++)
                    UserData[i] = Buffer[i];
                UserData[nLength] = 0;

                return true;
            }
            else
            {
                UserData = null;
                nLength = 0;

                return false;
            }
        }

        public string TagReadString()
        {
            return TagReadString(Encoding.GetEncoding("gb2312"));
        }

        public string TagReadString(Encoding decoder)
        {
            byte[] UserData;
            int nLength = 0;
            if (TagReadString(out UserData, ref nLength))
            {
                return decoder.GetString(UserData, 0, nLength);
            }
            else
            {
                return "";
            }
        }

        // 把用户数据写到当前RFID标签
        public bool TagWriteString(byte[] UserData, int nLength)
        {
            if (this.m_hRFidDev == IntPtr.Zero)
                return false;

            if (UserData == null || nLength <= 0)
                throw new ArgumentException();

            return DLL.DevLibTagWriteString(this.m_hRFidDev, UserData, nLength);
        }

        public bool TagWriteString(string UserData)
        {
            return TagWriteString(UserData, Encoding.GetEncoding("gb2312"));
        }

        public bool TagWriteString(string UserData, Encoding encoder)
        {
            byte[] Data = encoder.GetBytes(UserData);
            return TagWriteString(Data, Data.Length);
        }

        public bool TagReadBlock(Byte TagType, byte[] lpTagId, Byte nTagIdLen, Byte nStartBlcokAddr, Byte nBlockCount, byte[] lpBuffer, int nLength)
        {
            if (this.m_hRFidDev == IntPtr.Zero)
                return false;

            if (lpBuffer == null || nLength <= 0)
                throw new ArgumentException();

            return DLL.DevLibTagReadBlock(this.m_hRFidDev, TagType, lpTagId, nTagIdLen, nStartBlcokAddr, nBlockCount, lpBuffer, nLength);
        }

        public bool TagWriteBlock(Byte TagType, byte[] lpTagId, Byte nTagIdLen, Byte nStartBlcokAddr, Byte nBlockCount, byte[] lpBuffer, int nLength)
        {
            if (this.m_hRFidDev == IntPtr.Zero)
                return false;

            if (lpBuffer == null || nLength <= 0)
                throw new ArgumentException();

            return DLL.DevLibTagWriteBlock(this.m_hRFidDev, TagType, lpTagId, nTagIdLen, nStartBlcokAddr, nBlockCount, lpBuffer, nLength);
        }

        public bool TagLockBlock(Byte TagType, byte[] lpTagId, Byte nTagIdLen, Byte nStartBlcokAddr, Byte nBlockCount)
        {
            if (this.m_hRFidDev == IntPtr.Zero)
                return false;

            return DLL.DevLibTagLockBlock(this.m_hRFidDev, TagType, lpTagId, nTagIdLen, nStartBlcokAddr, nBlockCount);
        }


        private void BarCodeForm_Load(object sender, EventArgs e)
        {
            // 继承类一般在这里调用打开设备的相关函数
            OpenBarCode();
        }

        private void BarCodeForm_Closed(object sender, EventArgs e)
        {
            // 不管继承类是否有正确的配对调用打开和关闭，我们都调用一下关闭确保安全；
            CloseBarCode();
            CloseRFID();
        }

        #region private variables
        private CustomMessageHandler m_msgAgent;
        private IntPtr m_hBarCodeDev;
        private IntPtr m_hRFidDev;
        #endregion
        public BarCodeType m_CurrentBarCodeType;
    }

    #region class - CustomMessageHandler
    public class CustomMessageHandler : Microsoft.WindowsCE.Forms.MessageWindow
    {
        // Assign integers to messages.
        // Note that custom Window messages start at WM_USER = 0x400.
        public const UInt32 WM_USER = 0x0400;
        public const UInt32 WM_RFID_TAG_NOTIFY = WM_USER + 11;
        public const UInt32 WM_BARCODE_NOTIFY = WM_USER + 12;

        private BaseForm _BaseForm;

        public int GetBarCodeNotifyMessageId()
        {
            return (int)WM_BARCODE_NOTIFY;
        }

        public int GetRFidTagNotifyMessageId()
        {
            return (int)WM_RFID_TAG_NOTIFY;
        }

        // Save a reference to the form so it can
        // be notified when messages are received.
        public CustomMessageHandler(BaseForm Frm)
        {
            this._BaseForm = Frm;
        }

        // Override the default WndProc behavior to examine messages.
        protected override void WndProc(ref Message msg)
        {
            if (msg.Msg == WM_BARCODE_NOTIFY)   // BarCode的自定义消息通知
            {
                int nDataLen = (int)msg.WParam;
                uint nBarCodeType = (uint)msg.LParam;
                byte[] BarCodeData;

                if (this._BaseForm.GetBarCodeRawData(out BarCodeData, ref nDataLen))
                {
                    this._BaseForm.m_CurrentBarCodeType = (BarCodeType)nBarCodeType;
                    this._BaseForm.OnBarCodeNotify(BarCodeData, nDataLen);
                }

                msg.Result = IntPtr.Zero;
            }
            else if (msg.Msg == WM_RFID_TAG_NOTIFY) // RFID的自定义消息通知
            {
                uint nTagIdLen = (uint)msg.WParam;
                this._BaseForm.OnRFidTagNotify(nTagIdLen);
                msg.Result = IntPtr.Zero;
            }
            else
            {
                base.WndProc(ref msg);
            }
        }
    }
    #endregion
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime;
using System.Runtime.InteropServices;
/**
 * NOTICE:
 * (1) 切记在项目工程添加引用“Microsoft.WindowsCE.Form”；
 * (2) .NET的string是UNICODE字符串，而设备的数据都是 BYTE 或 ANSI字符，所以记得需要进行编码转换；
 * (3) 别忘记调用相关API完成DLL的初始化和清理；
 */
namespace BarcodeCollect
{
    public enum RFID_TAG_TYPE
    {
        TAG_TYPE_UNKNOWN = 0,
        TAG_TYPE_ISO15693,
        TAG_TYPE_I_CODE1,
        TAG_TYPE_TAG_IT_HF,
        TAG_TYPE_ISO14443A,
        TAG_TYPE_ISO14443B,
        TAG_TYPE_PICOTAG,
        TAG_TYPE_GEMWAVE
    };

    [Flags]
    public enum BarCodeType : uint
    {
        BARCODE_UNKNOWN = 0,		// Invalid
        BARCODE_CODE39,				// Code39
        BARCODE_CODE128,			// CODE128 / EAN128 / UCC128 / AIM128
        BARCODE_EAN_UPC,			// EAN8 / EAN13 / UPC-A / UPC -E
        BARCODE_CODABAR,			// Codabar
        BARCODE_CODE93,				// Code 93
        BARCODE_CODE11,				// Code 11
        BARCODE_I_2_OF_5,			// Interleaved 2 of 5
        BARCODE_PDF417,				// PDF417 / Micro PDF417
        BARCODE_MSI_PLESSEY,		// MSI-Plessey
        BARCODE_PLESSEY,			// Plessey
        BARCODE_QR,					// QR Code
        BARCODE_STD_2_OF_5,			// Standard 25
        BARCODE_D_2_OF_5,			// Industrial 25 / IATA 2 of 5
        BARCODE_DATAMATRIX,			// DataMatrix
        BARCODE_GS1_DATABAR,		// GS1 databar / RSS
        BARCODE_AZTEC,				// Aztec
        BARCODE_EXTEND				// Extend ...
    };

    class DLL
    {
        //
        //      Misc
        //////////////////////////////////////////////////////////////////////////

        // 初始化动态库（主程序开始运行时调用）
        public static Boolean Startup()
        {
            return DevLibStartup(0);
        }

        // 关闭和释放资源（主程序终止运行前调用）
        public static Boolean Shutdown()
        {
            return DevLibCleanup();
        }

        // 获取DLL版本
        public static string GetVersion()
        {
            StringBuilder _Builder = new StringBuilder(128);
            int nLen = (int)DLL.DevLibGetVersion(_Builder, (uint)_Builder.Capacity);
            if (nLen > _Builder.Capacity)
            {
                _Builder.Capacity = nLen + 1;
                nLen = (int)DLL.DevLibGetVersion(_Builder, (uint)_Builder.Capacity);
            }

            return _Builder.ToString(0, nLen);
        }

        // 设置在扫描条码成功解码后是否响声提示，bEnable = true 允许响声提示
        public static Boolean SetBeeperEnable(Boolean bEnable)
        {
            return DevLibConfigNotifications(NOTIFY_OBJECT_BEEPER, bEnable);
        }

        // 设置在扫描条码成功解码后是否震动提示，bEnable = true 允许震动
        public static Boolean SetVibrateEnable(Boolean bEnable)
        {
            return DevLibConfigNotifications(NOTIFY_OBJECT_VIBRATE, bEnable);
        }


        //
        //      BarCode
        //////////////////////////////////////////////////////////////////////////

        public static IntPtr OpenBarCodeScanner(IntPtr hWnd, int uMsgId)
        {
            return DevLibOpenBarCodeScanner(hWnd, uMsgId, 0);
        }

        public static Boolean CloseBarCodeScanner(IntPtr hBarCodeDev)
        {
            return DevLibCloseBarCodeScanner(hBarCodeDev);
        }

        public static Boolean EnableBarCodeScanner(IntPtr hBarCodeDev, Boolean bEnable)
        {
            return DevLibEnableBarCodeScanner(hBarCodeDev, bEnable);
        }

        //
        //      RFID
        //////////////////////////////////////////////////////////////////////////

        public static IntPtr OpenRFID(IntPtr hWnd, int uMsgId)
        {
            return DevLibRFidOpen(hWnd, uMsgId, 0);
        }

        public static Boolean CloseRFID(IntPtr hRFidDev)
        {
            return DevLibRFidClose(hRFidDev);
        }

        public static Boolean EnableRFIDScan(IntPtr hRFidDev, Boolean bEnable)
        {
            return DevLibEnableRFIDScan(hRFidDev, bEnable);
        }


        /************************************************************************/
        /*          iScanDevLib.dll                                             */
        /************************************************************************/

        //
        //        Misc
        //////////////////////////////////////////////////////////////////////////

        [DllImport("iScanDevLib.dll", EntryPoint = "DevLibStartup", SetLastError = true)]
        private static extern Boolean DevLibStartup(UInt32 dwFlags);

        [DllImport("iScanDevLib.dll", EntryPoint = "DevLibCleanup", SetLastError = true)]
        private static extern Boolean DevLibCleanup();

        [DllImport("iScanDevLib.dll", CharSet = CharSet.Unicode, EntryPoint = "DevLibGetVersion", SetLastError = true)]
        private static extern UInt32 DevLibGetVersion([Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder lpBuffer, UInt32 cchSize);

        const UInt32 NOTIFY_OBJECT_BEEPER = 1;
        const UInt32 NOTIFY_OBJECT_VIBRATE = 2;
        [DllImport("iScanDevLib.dll", EntryPoint = "DevLibConfigNotifications", SetLastError = true)]
        private static extern Boolean DevLibConfigNotifications(UInt32 dwNotifyObject, Boolean bEnable);

        [DllImport("iScanDevLib.dll", CharSet = CharSet.Unicode, EntryPoint = "DevLibGetDebugMessage", SetLastError = true)]
        public static extern UInt32 DevLibGetDebugMessage(IntPtr hDevice, [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder lpBuffer, UInt32 cchSize);

        [DllImport("iScanDevLib.dll", CharSet = CharSet.Unicode, EntryPoint = "DevLibGetDataTransfer", SetLastError = true)]
        public static extern UInt32 DevLibGetDataTransfer(IntPtr hDevice, [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder lpBuffer, UInt32 cchSize);


        //
        //      BarCode
        //////////////////////////////////////////////////////////////////////////

        [DllImport("iScanDevLib.dll", EntryPoint = "DevLibOpenBarCodeScanner", SetLastError = true)]
        public static extern IntPtr DevLibOpenBarCodeScanner(IntPtr hWnd, int uMsgId, UInt32 dwFlags);

        [DllImport("iScanDevLib.dll", EntryPoint = "DevLibCloseBarCodeScanner", SetLastError = true)]
        public static extern Boolean DevLibCloseBarCodeScanner(IntPtr hBarCodeDev);

        [DllImport("iScanDevLib.dll", EntryPoint = "DevLibEnableBarCodeScanner", SetLastError = true)]
        public static extern Boolean DevLibEnableBarCodeScanner(IntPtr hBarCodeDev, Boolean bEnable);

        [DllImport("iScanDevLib.dll", EntryPoint = "DevLibReadBarCode", SetLastError = true)]
        public static extern UInt32 DevLibReadBarCode(IntPtr hBarCodeDev, byte[] lpBuffer, UInt32 cbSize, ref UInt32 dwSizeRet);

        //[DllImport("iScanDevLib.dll", EntryPoint = "DevLibReadBarCodeEx", SetLastError = true)]
        //public static extern UInt32 DevLibReadBarCodeEx(IntPtr hBarCodeDev, byte[] lpBuffer, UInt32 cbSize, ref UInt32 dwSizeRet, ref UInt32 dwBarCodeType );

        //
        //      RFID
        //////////////////////////////////////////////////////////////////////////
        [DllImport("iScanDevLib.dll", EntryPoint = "DevLibRFidOpen", SetLastError = true)]
        public static extern IntPtr DevLibRFidOpen(IntPtr hWnd, int uMsgId, UInt32 dwFlags);

        [DllImport("iScanDevLib.dll", EntryPoint = "DevLibRFidClose", SetLastError = true)]
        public static extern Boolean DevLibRFidClose(IntPtr hRFidDev);

        [DllImport("iScanDevLib.dll", EntryPoint = "DevLibEnableRFIDScan", SetLastError = true)]
        public static extern Boolean DevLibEnableRFIDScan(IntPtr hRFidDev, Boolean bEnable);

        // 软件方式触发RFID标签检测，注意定时自动检测的时间间隔不能太小
        // 硬件方式则是手工按键盘正面中间黄色大扫描键
        [DllImport("iScanDevLib.dll", EntryPoint = "DevLibRFidDetectTag", SetLastError = true)]
        public static extern Boolean DevLibRFidDetectTag(IntPtr hRFidDev);

        // 读取当前检索到的RFID标签的唯一ID标识，第一个字节是标签的类型(TagType)，剩余字节才是标签ID标识
        [DllImport("iScanDevLib.dll", EntryPoint = "DevLibReadTagId", SetLastError = true)]
        public static extern UInt32 DevLibReadTagId(IntPtr hRFidDev, byte[] lpBuffer, UInt32 cbSize, ref UInt32 dwSizeRet);

        // 读取RFID模块返回的响应值，有助于判断操作失败的原因
        [DllImport("iScanDevLib.dll", EntryPoint = "DevLibGetResponseCode", SetLastError = true)]
        public static extern UInt32 DevLibGetResponseCode(IntPtr hRFidDev);

        [DllImport("iScanDevLib.dll", EntryPoint = "DevLibTagReadString", SetLastError = true)]
        public static extern Boolean DevLibTagReadString(IntPtr hRFidDev, byte[] lpBuffer, int nBufferSize);

        [DllImport("iScanDevLib.dll", EntryPoint = "DevLibTagWriteString", SetLastError = true)]
        public static extern Boolean DevLibTagWriteString(IntPtr hRFidDev, byte[] lpString, int nLength);

        [DllImport("iScanDevLib.dll", EntryPoint = "DevLibTagReadBlock", SetLastError = true)]
        public static extern Boolean DevLibTagReadBlock(IntPtr hRFidDev, byte TagType, byte[] lpTagId, byte nTagIdLen, byte nStartBlcokAddr, byte nBlockCount, byte[] lpBuffer, int nLength);

        [DllImport("iScanDevLib.dll", EntryPoint = "DevLibTagWriteBlock", SetLastError = true)]
        public static extern Boolean DevLibTagWriteBlock(IntPtr hRFidDev, byte TagType, byte[] lpTagId, byte nTagIdLen, byte nStartBlcokAddr, byte nBlockCount, byte[] lpBuffer, int nLength);

        [DllImport("iScanDevLib.dll", EntryPoint = "DevLibTagLockBlock", SetLastError = true)]
        public static extern Boolean DevLibTagLockBlock(IntPtr hRFidDev, byte TagType, byte[] lpTagId, byte nTagIdLen, byte nStartBlcokAddr, byte nBlockCount);

        //
        //      Native Win32 API
        //////////////////////////////////////////////////////////////////////////
        [DllImport("coredll.dll", EntryPoint = "GetTickCount", SetLastError = true)]
        public static extern UInt32 GetTickCount();

        [DllImport("coredll.dll", EntryPoint = "MessageBeep", SetLastError = true)]
        public static extern Boolean MessageBeep(UInt32 uType);
    }
}

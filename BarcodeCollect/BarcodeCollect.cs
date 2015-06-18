using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace BarcodeCollect
{
    public partial class BarcodeCollect : BaseForm
    {
        private BindingSource bsTemp = new BindingSource();
        private TempDS tempDS = new TempDS();

        private Dictionary<String, String> TYPES;

        private int linenum = 0;

        public BarcodeCollect()
        {
            InitializeComponent();
            this.BindDataSoureToGrid();
            this.SetGridStyle();
        }

        private void BindDataSoureToGrid()
        {
            this.bsTemp.DataSource = this.tempDS;
            this.bsTemp.DataMember = this.tempDS.Temp.TableName;
            this.dataGrid1.DataSource = this.bsTemp;
        }

        private void SetGridStyle()
        {
            DataGridTableStyle ts = new DataGridTableStyle();
            ts.MappingName = "Temp";
            DataGridColumnStyle cs;

            cs = new DataGridTextBoxColumn();
            cs.MappingName = "LineNum";
            cs.HeaderText = "#";
            cs.Width = 30;
            ts.GridColumnStyles.Add(cs);

            cs = new DataGridTextBoxColumn();
            cs.MappingName = "ItemCode";
            cs.HeaderText = "存货编码";
            cs.Width = 130;
            ts.GridColumnStyles.Add(cs);

            cs = new DataGridTextBoxColumn();
            cs.MappingName = "Type";
            cs.HeaderText = "型号";
            cs.Width = 130;
            ts.GridColumnStyles.Add(cs);

            cs = new DataGridTextBoxColumn();
            cs.MappingName = "Barcode";
            cs.HeaderText = "条码";
            cs.Width = 130;
            ts.GridColumnStyles.Add(cs);

            this.dataGrid1.TableStyles.Clear();
            this.dataGrid1.TableStyles.Add(ts);
        }
        
        // 重载 基类BaseForm的这个方法
        public override void OnBarCodeNotify(byte[] barcodedata, int nlength)
        {
            /*
             * notice:
             * 
             * 1. barcodedata 是条形码扫描设备解码成功后发送过来的原始数据
             * 
             * 2. 假如在获得条形码后需要很长时间处理，可以先暂时禁用扫描设备 enablebarcode(false)
             *     以免客户多次扫描，处理完了之后 enablebarcode(true) 重新允许扫描。
             */
            EnableBarCode(false);

            try
            {
                string sBarcode = Encoding.UTF8.GetString(barcodedata, 0, nlength); // encoding.ascii.getstring(barcodedata, 0, nlength);

                // 假如需要加上 自定义的前缀 或者 自定义的后缀
                // 建议把这两个参数设计为可以通过参数设置形式让客户在运行时改变。
                // string barcodeprefix = ""; 
                // string barcodepostfix = "";
                // string barcodeperfect = barcodeprefix + barcode + barcodepostfix;

                // 这里只是简单把条码显示到界面上而已
                //this.tboxbarcode.text = sbarcode;
                //this.lblbarcodelength.text = "len = " + nlength.tostring() + ", " + getcurrentbarcodetype().tostring();


                scanResult(sBarcode);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                EnableBarCode(true);
            }
        }

        private void scanResult(string barcode)
        {
            try
            {
                bool has = false;


                foreach (TempDS.TempRow row in this.tempDS.Temp.Rows)
                {
                    if (string.Compare(row.Barcode, barcode, true) == 0)
                    {
                        has = true;
                        break;
                    }
                }

                if (has)
                {
                    MessageBox.Show("物料扫描重复，请重新扫描！");
                }
                else
                {
                    linenum++;

                    TempDS.TempRow addrow = tempDS.Temp.NewTempRow();

                    addrow.LineNum = linenum;
                    addrow.Barcode = barcode;
                    try
                    {
                        string s = TYPES[barcode.Substring(0, 6)];
                        string[] d = s.Split('_');
                        addrow.ItemCode = d[0];
                        addrow.Type = d[1];
                    }
                    catch (KeyNotFoundException ex)
                    {
                        addrow.Type = "";
                        addrow.ItemCode = "";
                    }

                    this.tempDS.Temp.AddTempRow(addrow);
                }
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                ErrorProcess.ShowError("操作异常，请重试！", ex);
            }
        }

        private void form_Load(object sender, EventArgs e)
        {
            // 打开BarCode扫描设备

            if (!OpenBarCode())
            {
                int ErrCode = Marshal.GetLastWin32Error();
                MessageBox.Show("Fail to Open BarCode :(, ErrCode = " + ErrCode.ToString(), "Error");
            }
            

            TYPES = FileAct.TypeList();
        }

        private void form_Closing(object sender, CancelEventArgs e)
        {
            // 关闭BarCode扫描设备
            CloseBarCode();
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            //新建采集单，如Form上有数据，则清除
            linenum = 0;
            this.textBox1.Text = "";
            this.tempDS.Clear();
            /*
            linenum++;

            TempDS.TempRow addrow = tempDS.Temp.NewTempRow();

            addrow.LineNum = linenum;
            addrow.Barcode = "12345678";
            string s = "ABCDEFGHKI_abcdefghi";
            string[] d = s.Split('_');
            addrow.ItemCode = d[0];
            addrow.Type = d[1];
            this.tempDS.Temp.AddTempRow(addrow);
             * */
        }

        private void menuItem2_Click(object sender, EventArgs e)
        {
            //保存采集单，并清除Form上数据
            if (this.textBox1.Text == "")
            {
                MessageBox.Show("请填写收货单号。");
                return;
            }

            if (this.tempDS.Temp.Rows.Count == 0)
            {
                MessageBox.Show("请扫描条码。");
                return;
            }

            string dirPath = "/My Documents/" + DateTime.Now.ToString("yyyy-MM-dd") + "/";

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            

            string filename = this.textBox1.Text.Trim() + ".xml";
            Dictionary<String, String> ds = new Dictionary<string, string>();

            foreach (TempDS.TempRow row in tempDS.Temp.Rows)
            {
                ds.Add(row.Barcode, row.Type+"_"+row.ItemCode);
            }

            int ret = FileAct.SaveToXML(dirPath, filename, ds);

            if (ret == -1)
            {
                linenum = 0;
                this.textBox1.Text = "";
                this.tempDS.Clear();
            }
            else
            {
                MessageBox.Show("保存出错，请联系管理员。");
            }
        }
    }
}
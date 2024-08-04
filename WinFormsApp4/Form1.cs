using Oven.Entity;
using Oven.Service;
using Oven.Utils.Modbus;
using System.IO.Ports;
using System.Net;
using WindowsFormsApp5;
using static Oven.Entity.DTME08Comm;
using static Oven.Entity.THM06Comm;

namespace WinFormsApp4
{
    public partial class Form1 : Form
    {
        private DataManager dataManager;
        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataManager = DataManager.Instance;
            dataManager.SingletonInit("192.168.1.101", 1025);
            var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();


            //SerialPort serialPort = new SerialPort("COM5", 19200, Parity.None, 8, StopBits.One)
            //{
            //    ReadTimeout = 3000, // 讀取超時時間設定為3秒
            //    WriteTimeout = 3000 // 寫入超時時間設定為3秒
            //};
            //serialPort.Open();

            //ModbusRtuMaster modbusMaster = new ModbusRtuMaster(serialPort);


            //// Modbus Data
            //THM06 tHM06 = new THM06();
            //DTME08 Dtme08 = new DTME08();
            //var task = dataManager.collectDTME08Data();

            //var task2 = dataManager.collectTHM06Data();

            //task.ContinueWith(x =>
            //{
            //    try
            //    {

            //        byte[] datas = x.Result;
            //        foreach (var data in datas)
            //        {
            //            richTextBox1.Text += $"{data:X2} ";
            //        }
            //        richTextBox1.Text += "\n";
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show(ex.Message);
            //    }
            //}, uiScheduler);


            //task2.ContinueWith(x =>
            //{
            //    try
            //    {
            //        byte[] datas = x.Result;
            //        foreach (var data in datas)
            //        {
            //            richTextBox2.Text += $"{data:X2} ";
            //        }
            //        richTextBox2.Text += "\n";
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show(ex.Message);
            //    }
            //}, uiScheduler);

        }

    }
}

using Oven.Utils.Modbus;
using System.IO.Ports;
using System.Net;
using static WinFormsApp4.Dao.Impl.DTME08Comm;
using static WinFormsApp4.Dao.Impl.THM06Comm;

namespace WinFormsApp4
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private DevcieService deviceDataService;
        private void Form1_Load(object sender, EventArgs e)
        {
            // modbus / Serial Init
            SerialPort serialPort = new SerialPort("COM5", 19200, Parity.None, 8, StopBits.One)
            {
                ReadTimeout = 3000, // 讀取超時時間設定為3秒
                WriteTimeout = 3000 // 寫入超時時間設定為3秒
            };
            serialPort.Open();
            var device1 = new THM06Device("THM06", serialPort);
            var device2 = new DTME08Devcie("DTME08", serialPort);
            var device3 = new PLCDevcie("PLC", "192.168.1.101", 1025);
            //var ethernetDevice = new EthernetDevice("EthernetDevice");

            //var devices = new List<IDevice> { device1, device2, ethernetDevice };
            var devices = new List<IDevice> { device1, device2, device3 };
            deviceDataService = new DevcieService();
            deviceDataService.AddDevice(device1);
            deviceDataService.AddDevice(device2);
            deviceDataService.AddDevice(device3);

        }

        private void DeviceDataService_DataFetched(object? sender, DataFetchedEventArgs e)
        {
            //var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            //Task.Run(() =>
            //{ },uiScheduler
            //);
            DataFetchedEventArgs args = e;
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateRichTextBox(args)));
            }
            else
            {
                UpdateRichTextBox(e);
            }
        }

        private void UpdateRichTextBox(DataFetchedEventArgs e)
        {
            List<IDeviceEntity> deviceEntities = e.Data;
            DateTime dateTime = e.Timestamp;
            foreach (IDeviceEntity deviceEntity in deviceEntities)
            {
                if (deviceEntity is THM06Entity)
                {
                    THM06Entity tHM06Entity = (THM06Entity)deviceEntity;
                    textBox1.Text = tHM06Entity.relativeHumidity.ToString();
                }
                if (deviceEntity is DTME08Entity)
                {
                    DTME08Entity dTME08Entity = (DTME08Entity)deviceEntity;
                    textBox2.Text = dTME08Entity.PV1.ToString();
                }
                if(deviceEntity is PLCEntity)
                {
                    PLCEntity PLCEntity = (PLCEntity)deviceEntity;
                    textBox3.Text = PLCEntity.Id1.ToString("X2");
                    textBox4.Text = PLCEntity.Id2.ToString("X2");
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            deviceDataService.DataFetched -= DeviceDataService_DataFetched;
            deviceDataService.DataFetched += DeviceDataService_DataFetched;
            deviceDataService.StartCollectingData();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            deviceDataService.DataFetched -= DeviceDataService_DataFetched;
            deviceDataService.StopCollectingData();
        }
    }
}

using Mitsubishi.Communication.MC.Mitsubishi;
using Mitsubishi.Communication.MC.Mitsubishi.Base;
using Oven.Entity;
using Oven.Utils;
using Oven.Utils.Modbus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp5;
using static Oven.Entity.DTME08;
using static Oven.Entity.THM06;

namespace Oven.Service
{
    public enum DeviceSlaveAddress
    {
        DTME08 = 2,
        THM06 = 3,
        DPMC530 = 4,
    }

    public class DataManager
    {
        //private static volatile MitsubishiClient? client;
        private static volatile DataManager? instance;
        private SerialPort serialPort;
        private static readonly object syncRoot = new object();
        private static System.Threading.Timer timer;
        private TaskManager taskManager;
        public ushort temp { get; set; }
        public string TMH6Str { get; set; }

        public static DataManager Instance
        {
            get
            {
                //「雙重檢查加鎖」（Double-Checked Locking）

                // 如果實例為 null，則進入同步區域進行實例化
                if (instance == null)
                {
                    lock (syncRoot)  // 鎖定同步根對象，確保只有一個執行緒能進入此區塊
                    {
                        // 再次檢查實例是否為 null，以防止多個執行緒同時進入第一個 if 判斷
                        if (instance == null)
                        {
                            instance = new DataManager();  // 實例化 PLCService
                        }
                    }
                }
                return instance;  // 返回單例實例
            }
        }
        private ModbusRtuMaster modbusMaster;

        public async Task<byte[]> collectTHM06Data()
        {
            List<byte> data = new List<byte>();

            THM06 tHM06 = new THM06();
            foreach (THM06ParameterAddress address in Enum.GetValues(typeof(THM06ParameterAddress)))
            {
                byte[] rtValue = await modbusMaster.ReadHoldingRegistersAsync(3, (ushort)address, (ushort)tHM06.commandsType[address]);
                switch (rtValue.Length)
                {
                    case (int)THM06DataType.String5 * 2:
                    case (int)THM06DataType.String8 * 2:
                        ASCIIEncoding ascii = new ASCIIEncoding();
                        // 將小端排序的byte陣列轉換成字串
                        TMH6Str = DataHandler.LittleEndianBytesToString(rtValue);
                        break;
                    case (int)THM06DataType.UShort * 2:
                        StringBuilder sb = new StringBuilder();
                        foreach (byte b in rtValue)
                        {
                            sb.Append($"{b:X2}");
                        }
                        Logger.Instance.writeMessage("", $"{Process.GetCurrentProcess().Threads.Count}", sb.ToString());
                        break;
                    case (int)THM06DataType.Float * 2:
                        byte[] bytes = new byte[4]; // 1W = 2bytes = 2char
                        ushort[] floatbytes = DataHandler.ByteArrayToWordArray(rtValue);

                        // Word Array 轉換成 Bytes Array
                        Buffer.BlockCopy(floatbytes, 0, bytes, 0, bytes.Length);
                        float a = BitConverter.ToSingle(bytes, 0);
                        Logger.Instance.writeMessage("", $"{Process.GetCurrentProcess().Threads.Count}", $"{a}");
                        break;
                }
            }
            return data.ToArray();
        }

        public async Task<byte[]> collectDTME08Data()
        {
            List<byte> data = new List<byte>();
            DTME08 Dtme08 = new DTME08();
            //8通道
            for (int i = 0; i < 8; i++)
            {
                byte[] rtValue = await modbusMaster.ReadHoldingRegistersAsync(2, (ushort)((ushort)DTMParameterAddress.PV + i), (ushort)Dtme08.commandsType[DTMParameterAddress.PV]);

                switch (rtValue.Length)
                {

                    case (int)DTME08DataType.UShort * 2:
                        temp = (ushort)((rtValue[0] << 8) + rtValue[1]);

                        Logger.Instance.writeMessage("", $"{Process.GetCurrentProcess().Threads.Count}", $"{temp}");
                        break;
                }
            }
            return data.ToArray();
        }

        public void SingletonInit(string IpAddress, int port)
        {
            // PLC Init
            A3E qNA3E = new A3E(IpAddress, (short)port);

            // modbus / Serial Init
            SerialPort serialPort = new SerialPort("COM5", 19200, Parity.None, 8, StopBits.One)
            {
                ReadTimeout = 3000, // 讀取超時時間設定為3秒
                WriteTimeout = 3000 // 寫入超時時間設定為3秒
            };
            serialPort.Open();

            modbusMaster = new ModbusRtuMaster(serialPort);


            // Modbus Data
            THM06 tHM06 = new THM06();
            DTME08 Dtme08 = new DTME08();

            //foreach (THM06ParameterAddress address in Enum.GetValues(typeof(THM06ParameterAddress)))
            //{

            //    taskManager.AddTask(() => modbusMaster.ReadHoldingRegistersAsync(3, (ushort)address, (ushort)tHM06.commandsType[address]));

            //}

            ////8通道
            //for (int i = 0; i < 8; i++)
            //{
            //    taskManager.AddTask(() => modbusMaster.ReadHoldingRegistersAsync(2, (ushort)((ushort)DTMParameterAddress.PV + i), (ushort)Dtme08.commandsType[DTMParameterAddress.PV]));
            //}
            taskManager = new TaskManager();
            taskManager.AddTask(async () =>
            {
                byte[] rtn = await collectTHM06Data();
                StringBuilder sb = new StringBuilder();
                foreach (byte b in rtn)
                {
                    sb.Append($"{b:X2}");
                }
                Logger.Instance.writeMessage("", $"{Process.GetCurrentProcess().Threads.Count}", sb.ToString());
                await Task.Delay(10);

                byte[] rtn2 = await collectDTME08Data();
                StringBuilder sb2 = new StringBuilder();
                foreach (byte b in rtn2)
                {
                    sb2.Append(b);
                }
                Logger.Instance.writeMessage("", $"{Process.GetCurrentProcess().Threads.Count}", sb2.ToString());
                await Task.Delay(10);


                Result<ushort> res = qNA3E.Read<ushort>("D1000", 3);
                if (res.IsSuccessed)
                {
                    StringBuilder sb3 = new StringBuilder();
                    foreach (ushort data in res.Datas)
                    {
                        sb3.Append(data);
                    }
                    Logger.Instance.writeMessage("", $"{Process.GetCurrentProcess().Threads.Count}", sb3.ToString());
                }
                else
                {
                    Logger.Instance.writeMessage("", $"{Process.GetCurrentProcess().Threads.Count}", res.Message);
                }
            });

            //taskManager.AddTask(async () => {
            //    byte[] rtn2 = await collectDTME08Data();
            //    StringBuilder sb = new StringBuilder();
            //    foreach (byte b in rtn2)
            //    {
            //        sb.Append(b);
            //    }
            //    Logger.Instance.writeMessage("", "", sb.ToString());
            //    await Task.Delay(10);
            //});
            taskManager.Start();

            //timer = new System.Threading.Timer((obj) =>
            //{
            //    // PLC Data
            //    Result<ushort> res = qNA3E.Read<ushort>("D1000", 3);
            //    if (res.IsSuccessed)
            //    {
            //        foreach (ushort data in res.Datas)
            //        {
            //            Console.WriteLine(data);
            //        }
            //    }
            //    else
            //    {

            //    }

            //    // Modbus Data
            //    THM06 tHM06 = new THM06();
            //    DTME08 Dtme08 = new DTME08();

            //    foreach (THM06ParameterAddress address in Enum.GetValues(typeof(THM06ParameterAddress)))
            //    {
            //        Task.Run(async () =>
            //        {

            //            byte[] rtValue = await modbusMaster.ReadHoldingRegistersAsync(3, (ushort)address, (ushort)tHM06.commandsType[address]);
            //            switch (rtValue.Length)
            //            {
            //                case (int)THM06DataType.String5 * 2:
            //                case (int)THM06DataType.String8 * 2:
            //                    ASCIIEncoding ascii = new ASCIIEncoding();
            //                    // 將小端排序的byte陣列轉換成字串
            //                    TMH6Str = DataHandler.LittleEndianBytesToString(rtValue);
            //                    break;
            //                case (int)THM06DataType.UShort * 2:
            //                    foreach (byte b in rtValue)
            //                    {
            //                        Console.Write($"{b:X2} ");
            //                    }
            //                    break;
            //                case (int)THM06DataType.Float * 2:
            //                    byte[] bytes = new byte[4]; // 1W = 2bytes = 2char
            //                    ushort[] floatbytes = DataHandler.ByteArrayToWordArray(rtValue);

            //                    // Word Array 轉換成 Bytes Array
            //                    Buffer.BlockCopy(floatbytes, 0, bytes, 0, bytes.Length);
            //                    float a = BitConverter.ToSingle(bytes, 0);
            //                    break;
            //            }
            //        });
            //    }

            //    Thread.Sleep(10);

            //    Task.Run(async () =>
            //    {
            //        // 8通道
            //        for (int i = 0; i < 8; i++)
            //        {
            //            byte[] rtValue = await modbusMaster.ReadHoldingRegistersAsync(2, (ushort)((ushort)DTMParameterAddress.PV + i), (ushort)Dtme08.commandsType[DTMParameterAddress.PV]);

            //            switch (rtValue.Length)
            //            {

            //                case (int)DTME08DataType.UShort * 2:
            //                    temp = (ushort)((rtValue[0] << 8) + rtValue[1]);

            //                    //foreach (byte b in rtValue)
            //                    //{
            //                    //    Console.Write($"{b:X2} ");
            //                    //}
            //                    break;
            //            }
            //        }
            //    });


            //}, null, 300, 500);
        }

        public bool isConnect()
        {
            bool result = false;
            //client?.Open();
            //if (client != null)
            //{
            //    result = client.Connected;
            //}
            return result;
        }

        public void SendTestMessage()
        {
            ////2、Write operation
            //if (client != null)
            //{
            //    client.Open();
            //    client.Write("M100", false);
            //    client.Write("D200", short.MaxValue);
            //    client.Write("D210", 33);
            //}
        }

        public short ReadTemp()
        {
            short result = 0;

            short response = (short)ReadAI("D1020");
            result = (short)(response * 1);
            return result;
        }

        public int ReadP1()
        {
            int result = 0;

            uint response = (uint)ReadAI("D2004");
            result = (int)(response * 1);
            return result;
        }

        private object ReadAI(string address)
        {
            uint result = uint.MaxValue;
            //if (client == null) return result;

            //client.Open();
            //var respone = client.ReadUInt32(address);
            //if (respone != null)
            //{
            //    result = respone.Value;
            //}
            return result;
        }
    }
}

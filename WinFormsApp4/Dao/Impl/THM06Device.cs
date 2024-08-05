using Oven.Utils;
using Oven.Utils.Modbus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinFormsApp4.Dao.Impl;
using static WinFormsApp4.Dao.Impl.THM06Comm;

public class THM06Device : IDevice
{
    private static THM06Comm tHM06 = new THM06Comm();
    private readonly SerialPort serialPort;
    private readonly ModbusRtuMaster modbusMaster;

    public string DeviceName { get; set; }

    public THM06Device(string deviceName, SerialPort serialPort)
    {
        DeviceName = deviceName;
        this.serialPort = serialPort;
        modbusMaster = new ModbusRtuMaster(serialPort);
    }
    public void Connect()
    {
        if (!serialPort.IsOpen)
        {
            serialPort.Open();
        }
    }

    public void Disconnect()
    {
        if (serialPort.IsOpen)
        {
            serialPort.Close();
        }
    }

    private object ProcessData(byte[] data)
    {
        object rtn = 0;
        switch (data.Length)
        {
            case (int)THM06DataType.String5 * 2:
            case (int)THM06DataType.String8 * 2:
                ASCIIEncoding ascii = new ASCIIEncoding();
                // 將小端排序的byte陣列轉換成字串
                rtn = DataHandler.LittleEndianBytesToString(data);

                break;
            case (int)THM06DataType.UShort * 2:
                StringBuilder sb = new StringBuilder();
                foreach (byte b in data)
                {
                    sb.Append($"{b:X2}");
                }
                rtn = sb.ToString();
                break;
            case (int)THM06DataType.Float * 2:
                byte[] bytes = new byte[4]; // 1W = 2bytes = 2char
                ushort[] floatbytes = DataHandler.ByteArrayToWordArray(data);

                // Word Array 轉換成 Bytes Array
                Buffer.BlockCopy(floatbytes, 0, bytes, 0, bytes.Length);
                rtn = BitConverter.ToSingle(bytes, 0);
                break;
        }
        Logger.Instance.writeMessage("THM06", $"{Process.GetCurrentProcess().Threads.Count}", rtn?.ToString());
        return rtn;
    }

    public async Task<IDeviceEntity> FetchDataAsync()
    {
        THM06Entity entity = new THM06Entity();
        try
        {
            // Relative Humi
            byte[] rtValue = await modbusMaster.ReadHoldingRegistersAsync(3, (ushort)THM06ParameterAddress.RelativeHumidity, (ushort)tHM06.commandsType[THM06ParameterAddress.RelativeHumidity]);
            entity.relativeHumidity = (float)ProcessData(rtValue);
        }
        catch (Exception ex)
        {
            Logger.Instance.writeMessage("COM", "THM06Devcie.FetchDataAsync", ex.Message);
        }
        finally
        {
            await Task.Delay(100);
        }
        return entity;
    }
}

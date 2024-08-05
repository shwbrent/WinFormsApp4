using Oven.Utils.Modbus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinFormsApp4.Dao.Impl;
using static WinFormsApp4.Dao.Impl.DTME08Comm;

public class DTME08Devcie : IDevice
{
    private static DTME08Comm Dtme08 = new DTME08Comm();
    private readonly SerialPort serialPort;
    private readonly ModbusRtuMaster modbusMaster;
    public string DeviceName { get; set; }

    public DTME08Devcie(string deviceName, SerialPort serialPort)
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

    public async Task<IDeviceEntity> FetchDataAsync()
    {
        DTME08Entity entity = new DTME08Entity();
        //8通道
        try
        {
            byte[] rtValue = await modbusMaster.ReadHoldingRegistersAsync(2, (ushort)DTMParameterAddress.PV, (ushort)Dtme08.commandsType[DTMParameterAddress.PV]);
            switch (rtValue.Length)
            {

                case (int)DTME08DataType.UShort * 2:
                    ushort temp = (ushort)((rtValue[0] << 8) + rtValue[1]);
                    entity.PV1 = temp / 10;
                    Logger.Instance.writeMessage("DTME08", $"{Process.GetCurrentProcess().Threads.Count}", $"{temp}");
                    break;
            }
        }
        catch (Exception ex) 
        {
            Logger.Instance.writeMessage("COM", "DTME08Devcie.FetchDataAsync", ex.Message);
        }
        finally
        {
            await Task.Delay(100);
        }
        return entity;
    }
}

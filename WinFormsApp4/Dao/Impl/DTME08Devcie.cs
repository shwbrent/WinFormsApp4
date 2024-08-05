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
    public Type DeviceType => typeof(DTME08Entity);

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

    private object processData(byte[] values)
    {
        float rt = float.NaN;
        switch (values.Length)
        {

            case (int)DTME08DataType.UShort * 2:
                ushort temp = (ushort)((values[0] << 8) + values[1]);
                if(rt != 8002)
                {
                    rt = temp / 10;
                }
                Logger.Instance.writeMessage("DTME08", $"{Process.GetCurrentProcess().Threads.Count}", $"{temp}");
                break;
        }
        return rt;
    }

    public async Task<IDeviceEntity> FetchDataAsync()
    {
        DTME08Entity entity = new DTME08Entity();
        //8通道
        try
        {
                byte[] PV1 = await modbusMaster.ReadHoldingRegistersAsync(2, (ushort)DTMParameterAddress.PV, (ushort)Dtme08.commandsType[DTMParameterAddress.PV]);
                entity.PV1 = (float)processData(PV1);
                byte[] PV2 = await modbusMaster.ReadHoldingRegistersAsync(2, (ushort)DTMParameterAddress.PV + 1, (ushort)Dtme08.commandsType[DTMParameterAddress.PV]);
                entity.PV2 = (float)processData(PV2);
                byte[] PV3 = await modbusMaster.ReadHoldingRegistersAsync(2, (ushort)DTMParameterAddress.PV + 2, (ushort)Dtme08.commandsType[DTMParameterAddress.PV]);
                entity.PV3 = (float)processData(PV3);

                byte[] SV1 = await modbusMaster.ReadHoldingRegistersAsync(2, (ushort)DTMParameterAddress.SV, (ushort)Dtme08.commandsType[DTMParameterAddress.SV]);
                entity.SV1 = (float)processData(SV1);
                byte[] SV2 = await modbusMaster.ReadHoldingRegistersAsync(2, (ushort)DTMParameterAddress.SV + 1, (ushort)Dtme08.commandsType[DTMParameterAddress.SV]);
                entity.SV2 = (float)processData(SV2);
                byte[] SV3 = await modbusMaster.ReadHoldingRegistersAsync(2, (ushort)DTMParameterAddress.SV + 2, (ushort)Dtme08.commandsType[DTMParameterAddress.SV]);
                entity.SV3 = (float)processData(SV3);
            
        }
        catch (Exception ex)
        {
            Logger.Instance.writeMessage("COM", "DTME08Devcie.FetchDataAsync", ex.Message);
        }
        finally
        {
            Thread.Sleep(10);
        }
        return entity;
    }
}

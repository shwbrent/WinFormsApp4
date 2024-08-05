using Mitsubishi.Communication.MC.Mitsubishi;
using Mitsubishi.Communication.MC.Mitsubishi.Base;
using Oven.Utils.Modbus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

public class PLCDevcie : IDevice
{
    public string DeviceName { get; set; }
    public Type DeviceType => typeof(PLCEntity);
    private A3E qNA3E;

    public PLCDevcie(string deviceName, string IpAddress, int port)
    {
        DeviceName = deviceName;
        // PLC Init
        qNA3E = new A3E(IpAddress, (short)port);
    }

    public void Connect()
    {
        throw new NotImplementedException();
    }

    public void Disconnect()
    {
        throw new NotImplementedException();
    }

    public async Task<IDeviceEntity> FetchDataAsync()
    {
        PLCEntity entity = new PLCEntity();
        Result<ushort> res = await qNA3E.Read<ushort>("D1000", 3);
        if (res.IsSuccessed)
        {
            StringBuilder sb3 = new StringBuilder();
            sb3.Append(res.Datas[0]);
            sb3.Append(res.Datas[1]);
            sb3.Append(res.Datas[2]);

            entity.Id1 = res.Datas[0];
            entity.Id2 = res.Datas[1];
            entity.Id3 = res.Datas[2];

            Logger.Instance.writeMessage("", $"{Process.GetCurrentProcess().Threads.Count}", sb3.ToString());
        }
        else
        {
            Logger.Instance.writeMessage("", $"{Process.GetCurrentProcess().Threads.Count}", res.Message);
        }
        Thread.Sleep(10);
        return entity;
    }
}

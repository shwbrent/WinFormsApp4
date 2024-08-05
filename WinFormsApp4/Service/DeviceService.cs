using Mitsubishi.Communication.MC.Mitsubishi.Base;
using Oven.Utils;
using Oven.Utils.Modbus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WinFormsApp4.Dao.Impl.DTME08Comm;
using static WinFormsApp4.Dao.Impl.THM06Comm;

public class DeviceService : IDeviceService
{
    // 私有靜態字段保存單例實例
    private static readonly Lazy<DeviceService> instance = new Lazy<DeviceService>(() => new DeviceService());

    public volatile List<IDevice> devices = new List<IDevice>();
    private TaskManager taskManager;
    private volatile bool isCollectingData;

    public DeviceService()
    {
        isCollectingData = false;
    }

    // 公共靜態屬性返回單例實例
    public static DeviceService Instance
    {
        get
        {
            return instance.Value;
        }
    }

    public void AddDevice(IDevice device)
    {
        devices.Add(device);
    }

    public void RemoveDevice(IDevice device)
    {
        devices.Remove(device);
    }

    public void StartCollectingData()
    {
        if (!isCollectingData)
        {
            taskManager = new TaskManager(this);
            foreach (var device in devices)
            {
                taskManager.AddTask(async() => await device.FetchDataAsync());
            }
            taskManager.Start();
        }
    }

    public void StopCollectingData()
    {
        if (isCollectingData)
        {
            taskManager.Stop();
            isCollectingData = false;
        }
    }

    public void ProcessResults(List<IDeviceEntity> results)
    {
        List<IDeviceEntity> datas = new List<IDeviceEntity>();

        foreach (var result in results)
        {
            if(result is THM06Entity)
            {
                THM06Entity entity = (THM06Entity)result;
                datas.Add(entity);
            }
            else if(result is DTME08Entity)
            {
                DTME08Entity entity = (DTME08Entity)result;
                datas.Add(entity);
            }
            else if(result is PLCEntity)
            {
                PLCEntity entity = (PLCEntity)result;
                datas.Add(entity);
            }
        }
        
        OnDataFetched(datas);
    }

    public event EventHandler<DataFetchedEventArgs> DataFetched;

    protected virtual void OnDataFetched(List<IDeviceEntity> data)
    {
        DataFetched?.Invoke(this, new DataFetchedEventArgs(data, DateTime.Now));
    }
}

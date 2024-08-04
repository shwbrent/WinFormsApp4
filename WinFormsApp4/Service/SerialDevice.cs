using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;


public class SerialDevice : IDevice
{
    public string DeviceName { get; private set; }
    private readonly SerialPort serialPort;

    public SerialDevice(string deviceName, SerialPort serialPort)
    {
        DeviceName = deviceName;
        this.serialPort = serialPort;
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

    public async Task<string> FetchDataAsync()
    {
        // 模擬數據提取
        await Task.Delay(1000);
        return "RS485 data";
    }

    public event EventHandler<DataFetchedEventArgs> DataFetched;

    protected virtual void OnDataFetched(string data)
    {
        DataFetched?.Invoke(this, new DataFetchedEventArgs(data, DateTime.Now));
    }
}

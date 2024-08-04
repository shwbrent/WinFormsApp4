using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DevcieService : IDeviceService
{
    private readonly List<IDevice> devices = new List<IDevice>();
    private readonly SerialPort serialPort;

    public DevcieService(string portName, int baudRate)
    {
        serialPort = new SerialPort(portName, baudRate);
    }

    public void AddDevice(IDevice device)
    {
        devices.Add(device);
    }

    public void RemoveDevice(IDevice device)
    {
        devices.Remove(device);
    }

    public async Task FetchDataAsync()
    {
        if (!serialPort.IsOpen)
        {
            serialPort.Open();
        }

        foreach (var device in devices)
        {
            // 切換到當前設備
            SelectDevice(device);

            var data = await device.FetchDataAsync();
            Console.WriteLine($"{device.DeviceName} fetched data: {data} at {DateTime.Now}");
        }

        serialPort.Close();
    }

    private void SelectDevice(IDevice device)
    {
        // 選擇設備的邏輯，例如發送特定命令來選擇設備
    }

    public event EventHandler<DataFetchedEventArgs> DataFetched;

    protected virtual void OnDataFetched(string data)
    {
        DataFetched?.Invoke(this, new DataFetchedEventArgs(data, DateTime.Now));
    }
}

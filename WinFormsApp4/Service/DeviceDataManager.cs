using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DeviceDataManager
{
    private readonly List<IDevice> devices;
    private readonly string csvFilePath;
    private Timer timer;
    private bool isCollectingData;

    public DeviceDataManager(List<IDevice> devices, string csvFilePath)
    {
        this.devices = devices;
        this.csvFilePath = csvFilePath;
        isCollectingData = false;

        // 寫入CSV標題行
        WriteCsvHeader();
    }

    public void StartCollectingData(int interval)
    {
        if (!isCollectingData)
        {
            timer = new Timer(OnTimerElapsed, null, 0, interval);
            isCollectingData = true;
        }
    }

    public void StopCollectingData()
    {
        if (isCollectingData)
        {
            timer?.Change(Timeout.Infinite, Timeout.Infinite);
            timer = null;
            isCollectingData = false;
        }
    }

    private async void OnTimerElapsed(object state)
    {
        var timestamp = DateTime.Now;
        var dataList = new List<string> { timestamp.ToString() };

        foreach (var device in devices)
        {
            var data = await device.FetchDataAsync();
            dataList.Add(data);
        }

        SaveDataToCsv(dataList);
    }

    private void WriteCsvHeader()
    {
        var header = "Timestamp";
        foreach (var device in devices)
        {
            header += $", {device.DeviceName}";
        }
        File.AppendAllText(csvFilePath, header + Environment.NewLine);
    }

    private void SaveDataToCsv(List<string> dataList)
    {
        var csvLine = string.Join(",", dataList);
        File.AppendAllText(csvFilePath, csvLine + Environment.NewLine);
    }

    public IEnumerable<string> ReadDataFromCsv()
    {
        if (File.Exists(csvFilePath))
        {
            return File.ReadLines(csvFilePath);
        }
        else
        {
            return new List<string>();
        }
    }
}
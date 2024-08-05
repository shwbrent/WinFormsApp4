
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Timer = System.Threading.Timer;

public class DataCollector
{
    private static string FilePath { get; set; } 
    private DeviceService DeviceService { get; set; }
    private List<IDevice> Devices { get; set; }
    private System.Threading.Timer timer;
    private static volatile List<DataFetchedEventArgs> InComingDatas;
    private List<Type> DeviceTypes;

    public DataCollector(string filePath) 
    {
        FilePath = filePath;
        InComingDatas = new List<DataFetchedEventArgs>();
        DeviceService = DeviceService.Instance; 
        Devices = DeviceService.devices;
    }

    private void WriteCsvHeader()
    {
        StringBuilder header = new StringBuilder("Timestamp");

        foreach(IDevice device in Devices)
        {
            foreach (var property in device.DeviceType.GetProperties())
            {
                // 取得自定義屬性
                var displayNameAttribute = property.GetCustomAttribute<DisplayNameAttribute>();

                if (displayNameAttribute != null)
                {
                    header.Append($",{displayNameAttribute.DisplayName}");
                }
            }
        }

        File.AppendAllText(FilePath, header + Environment.NewLine, Encoding.UTF8);
    }

    private void SaveDataToCsv(List<string> dataList)
    {
        var csvLine = string.Join(",", dataList);
        File.AppendAllText(FilePath, csvLine + Environment.NewLine, Encoding.UTF8);
    }

    private void DevcieService_DataFetched(object? sender, DataFetchedEventArgs e)
    {
        lock (InComingDatas)
        {
            InComingDatas.Add(e);
        }
    }

    public void StartCollector()
    {
        DeviceService.DataFetched -= DevcieService_DataFetched;
        DeviceService.DataFetched += DevcieService_DataFetched;
        // 寫入CSV標題行
        WriteCsvHeader();

        // 創建計時器，設置回調方法，初始延遲500ms，間隔時間2000ms
        if(timer != null)
        {
            timer.Dispose();
        }
        timer = new Timer(TimerCallback, null, 50, 500);
    }

    public void StopCollector()
    {
        // 停止計時器
        if (timer != null)
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }

    private static void TimerCallback(Object o)
    {
        
        lock (InComingDatas)
        {
            List<DataFetchedEventArgs> datas = InComingDatas;

            foreach (DataFetchedEventArgs data in datas) 
            {
                if(data.Data.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(data.Timestamp.ToString("yyyy/MM/dd HH:mm:ss.fff")).Append(",");
                    foreach (IDeviceEntity deviceEntity in data.Data)
                    {
                        Type type = typeof(DTME08Entity);

                        if (deviceEntity is THM06Entity)
                        {
                            type = typeof(THM06Entity);
                        }
                        else if (deviceEntity is PLCEntity)
                        {
                            type = typeof(PLCEntity);
                        }

                        // 取得所有屬性
                        PropertyInfo[] properties = type.GetProperties();

                        foreach (var property in properties)
                        {
                            // 取得屬性名稱
                            string propertyName = property.Name;

                            // 取得屬性值
                            object propertyValue = property.GetValue(deviceEntity);

                            sb.Append(propertyValue);
                            sb.Append(",");
                        }
                    }
                    File.AppendAllText(FilePath, sb.ToString() + Environment.NewLine, Encoding.UTF8);
                }
            }
            InComingDatas.Clear();
        }
    }
}


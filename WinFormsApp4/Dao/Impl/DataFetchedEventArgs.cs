using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DataFetchedEventArgs : EventArgs
{
    public List<IDeviceEntity> Data { get; }
    public DateTime Timestamp { get; }

    public DataFetchedEventArgs(List<IDeviceEntity> data, DateTime timestamp)
    {
        Data = data;
        Timestamp = timestamp;
    }
}

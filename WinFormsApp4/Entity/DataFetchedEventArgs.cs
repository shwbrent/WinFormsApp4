using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DataFetchedEventArgs : EventArgs
{
    public string Data { get; }
    public DateTime Timestamp { get; }

    public DataFetchedEventArgs(string data, DateTime timestamp)
    {
        Data = data;
        Timestamp = timestamp;
    }
}

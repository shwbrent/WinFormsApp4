using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IDevice
{
    string DeviceName { get; }
    void Connect();
    void Disconnect();
    Task<string> FetchDataAsync();
}

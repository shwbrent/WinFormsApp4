using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IDeviceService
{
    void AddDevice(IDevice device);
    void RemoveDevice(IDevice device);
    void ProcessResults(List<IDeviceEntity> results);
    //Task FetchDataAsync();
    event EventHandler<DataFetchedEventArgs> DataFetched;
}

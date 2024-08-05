using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class THM06Entity : IDeviceEntity
{
    public float relativeHumidity { get; set; }
    public float temp { get; set; }
    public string ModelName { get; set; }
}

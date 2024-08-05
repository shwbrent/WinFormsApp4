using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class THM06Entity : IDeviceEntity
{
    [DisplayName("相對溼度")]
    public float relativeHumidity { get; set; }
}

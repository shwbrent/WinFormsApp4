using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PLCEntity : IDeviceEntity
{
    [DisplayName("D1000")]
    public ushort Id1 { get; set; }
    [DisplayName("D1001")]
    public ushort Id2 { get; set; }
    [DisplayName("D1002")]
    public ushort Id3 { get; set; }
}
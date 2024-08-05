using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DTME08Entity : IDeviceEntity
{
    public float PV1 { get; set; }
    public float PV2 { get; set; }
    public float PV3 { get; set; }
    public float SV1 { get; set; }
    public float SV2 { get; set; }
    public float SV3 { get; set; }
}
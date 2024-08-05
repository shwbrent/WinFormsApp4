using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DTME08Entity : IDeviceEntity
{
    [DisplayName("溫度1")]
    public float PV1 { get; set; }
    [DisplayName("溫度2")]
    public float PV2 { get; set; }
    [DisplayName("溫度3")]
    public float PV3 { get; set; }
    [DisplayName("設定溫度1")]
    public float SV1 { get; set; }
    [DisplayName("設定溫度2")]
    public float SV2 { get; set; }
    [DisplayName("設定溫度3")]
    public float SV3 { get; set; }
}
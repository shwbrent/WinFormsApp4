using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oven.Entity
{
    public class DTME08Comm
    {
        public readonly Dictionary<DTMParameterAddress, DTME08DataType> commandsType;

        public DTME08Comm()
        {
            commandsType = new Dictionary<DTMParameterAddress, DTME08DataType>{
                { DTMParameterAddress.SV, DTME08DataType.UShort },
                { DTMParameterAddress.SVUpperLimit, DTME08DataType.UShort },
                { DTMParameterAddress.SVLowerLimit, DTME08DataType.UShort },
                { DTMParameterAddress.InputErrorAdjustment, DTME08DataType.UShort },
                { DTMParameterAddress.InputErrorGain, DTME08DataType.UShort },
                { DTMParameterAddress.SensorType, DTME08DataType.UShort },
                { DTMParameterAddress.TemperatureFilterFactor, DTME08DataType.UShort },
                { DTMParameterAddress.TemperatureFilterRange, DTME08DataType.UShort },
                { DTMParameterAddress.Alarm1Mode, DTME08DataType.UShort },
                { DTMParameterAddress.Alarm1Delay, DTME08DataType.UShort },
                { DTMParameterAddress.Alarm1Function, DTME08DataType.UShort },
                { DTMParameterAddress.Alarm2Mode, DTME08DataType.UShort },
                { DTMParameterAddress.Alarm2Delay, DTME08DataType.UShort },
                { DTMParameterAddress.Alarm2Function, DTME08DataType.UShort },
                { DTMParameterAddress.Alarm3Mode, DTME08DataType.UShort },
                { DTMParameterAddress.Alarm3Delay, DTME08DataType.UShort },
                { DTMParameterAddress.Alarm3Function, DTME08DataType.UShort },
                { DTMParameterAddress.ControlMode, DTME08DataType.UShort },
                { DTMParameterAddress.ManualSwitch, DTME08DataType.UShort },
                { DTMParameterAddress.Output1Control, DTME08DataType.UShort },
                { DTMParameterAddress.Output2Control, DTME08DataType.UShort },
                { DTMParameterAddress.Output1Sensitivity, DTME08DataType.UShort },
                { DTMParameterAddress.ManualOutput1, DTME08DataType.UShort },
                { DTMParameterAddress.Output1UpperLimit, DTME08DataType.UShort },
                { DTMParameterAddress.Output1LowerLimit, DTME08DataType.UShort },
                { DTMParameterAddress.Output1Cycle, DTME08DataType.UShort },
                { DTMParameterAddress.Output1ErrorOperation, DTME08DataType.UShort },
                { DTMParameterAddress.Output2Sensitivity, DTME08DataType.UShort },
                { DTMParameterAddress.ManualOutput2, DTME08DataType.UShort },
                { DTMParameterAddress.Output2UpperLimit, DTME08DataType.UShort },
                { DTMParameterAddress.Output2LowerLimit, DTME08DataType.UShort },
                { DTMParameterAddress.Output2Cycle, DTME08DataType.UShort },
                { DTMParameterAddress.Output2ErrorOperation, DTME08DataType.UShort },
                { DTMParameterAddress.PControlErrorCompensation, DTME08DataType.UShort },
                { DTMParameterAddress.DeadZone, DTME08DataType.UShort },
                { DTMParameterAddress.CoolingMode, DTME08DataType.UShort },
                { DTMParameterAddress.Output1Mapping, DTME08DataType.UShort },
                { DTMParameterAddress.Output2Mapping, DTME08DataType.UShort },
                { DTMParameterAddress.Alarm1Mapping, DTME08DataType.UShort },
                { DTMParameterAddress.Alarm2Mapping, DTME08DataType.UShort },
                { DTMParameterAddress.Alarm3Mapping, DTME08DataType.UShort },
                { DTMParameterAddress.CT1Mapping, DTME08DataType.UShort },
                { DTMParameterAddress.CT2Mapping, DTME08DataType.UShort },
                { DTMParameterAddress.OperationControl, DTME08DataType.UShort },
                { DTMParameterAddress.AutoTuning, DTME08DataType.UShort },
                { DTMParameterAddress.PV, DTME08DataType.UTemp },
                { DTMParameterAddress.SVReadOnly, DTME08DataType.UShort },
                { DTMParameterAddress.ReadOutput1Operation, DTME08DataType.UShort },
                { DTMParameterAddress.ReadOutput2Operation, DTME08DataType.UShort },
                { DTMParameterAddress.InputChannelStatus, DTME08DataType.UShort },
                { DTMParameterAddress.CT1AlarmCurrent, DTME08DataType.UShort },
                { DTMParameterAddress.CT2AlarmCurrent, DTME08DataType.UShort },
                { DTMParameterAddress.ProportionalBand, DTME08DataType.UShort },
                { DTMParameterAddress.IntegrationTime, DTME08DataType.UShort },
                { DTMParameterAddress.DifferentiationTime, DTME08DataType.UShort },
                { DTMParameterAddress.CoolingProportionalBand, DTME08DataType.UShort },
                { DTMParameterAddress.CoolingIntegrationTime, DTME08DataType.UShort },
                { DTMParameterAddress.CoolingDifferentiationTime, DTME08DataType.UShort },
                { DTMParameterAddress.PIDGroupSwitch, DTME08DataType.UShort },
                { DTMParameterAddress.SlopeSetting, DTME08DataType.UShort },
                { DTMParameterAddress.Alarm1PeakValue, DTME08DataType.UShort },
                { DTMParameterAddress.Alarm1LowestValue, DTME08DataType.UShort },
                { DTMParameterAddress.Alarm2PeakValue, DTME08DataType.UShort },
                { DTMParameterAddress.Alarm2LowestValue, DTME08DataType.UShort },
                { DTMParameterAddress.Alarm3PeakValue, DTME08DataType.UShort },
                { DTMParameterAddress.Alarm3LowestValue, DTME08DataType.UShort },
                { DTMParameterAddress.ChannelDisable, DTME08DataType.UShort },
                { DTMParameterAddress.TemperatureUnit, DTME08DataType.UShort },
                { DTMParameterAddress.ColdJunctionCompensation, DTME08DataType.UShort }
            };
        }

        public enum DTMParameterAddress : ushort
        {
            [Description("SV 值(讀寫)")]
            SV = 0x0000,

            [Description("SV 設定值上限")]
            SVUpperLimit = 0x0008,

            [Description("SV 設定值下限")]
            SVLowerLimit = 0x0010,

            [Description("輸入誤差調整值")]
            InputErrorAdjustment = 0x0018,

            [Description("輸入誤差增益值")]
            InputErrorGain = 0x0020,

            [Description("輸入感測器")]
            SensorType = 0x0028,

            [Description("溫度濾波因數")]
            TemperatureFilterFactor = 0x0030,

            [Description("溫度濾波範圍")]
            TemperatureFilterRange = 0x0038,

            [Description("警報 1 模式")]
            Alarm1Mode = 0x0040,

            [Description("警報 1 延遲")]
            Alarm1Delay = 0x0048,

            [Description("警報 1 功能")]
            Alarm1Function = 0x0050,

            [Description("警報 2 模式")]
            Alarm2Mode = 0x0058,

            [Description("警報 2 延遲")]
            Alarm2Delay = 0x0060,

            [Description("警報 2 功能")]
            Alarm2Function = 0x0068,

            [Description("警報 3 模式")]
            Alarm3Mode = 0x0070,

            [Description("警報 3 延遲")]
            Alarm3Delay = 0x0078,

            [Description("警報 3 功能")]
            Alarm3Function = 0x0080,

            [Description("控制方式")]
            ControlMode = 0x00B8,

            [Description("手動開關切換")]
            ManualSwitch = 0x00C0,

            [Description("輸出 1 控制選擇")]
            Output1Control = 0x00C8,

            [Description("輸出 2 控制選擇")]
            Output2Control = 0x00D0,

            [Description("輸出 1 感度調整")]
            Output1Sensitivity = 0x00D8,

            [Description("讀寫手動輸出 1 操作量")]
            ManualOutput1 = 0x00E0,

            [Description("輸出 1 上限")]
            Output1UpperLimit = 0x00E8,

            [Description("輸出 1 下限")]
            Output1LowerLimit = 0x00F0,

            [Description("輸出 1 控制週期")]
            Output1Cycle = 0x00F8,

            [Description("PV 異常時輸出 1 操作量")]
            Output1ErrorOperation = 0x0100,

            [Description("輸出 2 感度調整")]
            Output2Sensitivity = 0x0118,

            [Description("讀寫手動輸出 2 操作量")]
            ManualOutput2 = 0x0120,

            [Description("輸出 2 上限")]
            Output2UpperLimit = 0x0128,

            [Description("輸出 2 下限")]
            Output2LowerLimit = 0x0130,

            [Description("輸出 2 控制週期")]
            Output2Cycle = 0x0138,

            [Description("PV 異常時輸出 2 操作量")]
            Output2ErrorOperation = 0x0140,

            [Description("比例控制誤差補償")]
            PControlErrorCompensation = 0x0170,

            [Description("不動作死區")]
            DeadZone = 0x0178,

            [Description("冷卻方式")]
            CoolingMode = 0x0180,

            [Description("輸出 1 對應站號通道")]
            Output1Mapping = 0x0190,

            [Description("輸出 2 對應站號通道")]
            Output2Mapping = 0x0198,

            [Description("警報 1 對應站號通道")]
            Alarm1Mapping = 0x01A0,

            [Description("警報 2 對應站號通道")]
            Alarm2Mapping = 0x01A8,

            [Description("警報 3 對應站號通道")]
            Alarm3Mapping = 0x01B0,

            [Description("CT1 對應站號通道")]
            CT1Mapping = 0x01C0,

            [Description("CT2 對應站號通道")]
            CT2Mapping = 0x01C8,

            [Description("執行/停止")]
            OperationControl = 0x0248,

            [Description("自整定")]
            AutoTuning = 0x0250,

            [Description("PV 值")]
            PV = 0x0268,

            [Description("SV 值(讀取)")]
            SVReadOnly = 0x0270,

            [Description("讀取輸出 1 操作量")]
            ReadOutput1Operation = 0x0278,

            [Description("讀取輸出 2 操作量")]
            ReadOutput2Operation = 0x0280,

            [Description("輸入通道狀態")]
            InputChannelStatus = 0x0288,

            [Description("CT1 警報電流值")]
            CT1AlarmCurrent = 0x02C8,

            [Description("CT2 警報電流值")]
            CT2AlarmCurrent = 0x02D0,

            [Description("比例帶")]
            ProportionalBand = 0x02E1,

            [Description("積分時間")]
            IntegrationTime = 0x02E2,

            [Description("微分時間")]
            DifferentiationTime = 0x02E3,

            [Description("冷卻側 比例帶")]
            CoolingProportionalBand = 0x02E4,

            [Description("冷卻側 積分時間")]
            CoolingIntegrationTime = 0x02E5,

            [Description("冷卻側 微分時間")]
            CoolingDifferentiationTime = 0x02E6,

            [Description("PID 群組切換")]
            PIDGroupSwitch = 0x03E8,

            [Description("斜率設定")]
            SlopeSetting = 0x03F0,

            [Description("警報 1 最高峰值")]
            Alarm1PeakValue = 0x0980,

            [Description("警報 1 最低峰值")]
            Alarm1LowestValue = 0x0988,

            [Description("警報 2 最高峰值")]
            Alarm2PeakValue = 0x0990,

            [Description("警報 2 最低峰值")]
            Alarm2LowestValue = 0x0998,

            [Description("警報 3 最高峰值")]
            Alarm3PeakValue = 0x09A0,

            [Description("警報 3 最低峰值")]
            Alarm3LowestValue = 0x09A8,

            [Description("通道禁能")]
            ChannelDisable = 0x0258,

            [Description("溫度單位")]
            TemperatureUnit = 0x0259,

            [Description("冷接點補償選擇")]
            ColdJunctionCompensation = 0x0260
        }

        public enum DTME08DataType
        {
            UShort = 1,
            UTemp = 1,
        }
    }
}

using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp4.Dao.Impl
{
    public class THM06Comm
    {
        public readonly Dictionary<THM06ParameterAddress, THM06DataType> commandsType;
        public THM06Comm()
        {
            commandsType = new Dictionary<THM06ParameterAddress, THM06DataType>
            {
                { THM06ParameterAddress.Firmware, THM06DataType.String5 },
                { THM06ParameterAddress.Serial, THM06DataType.String8 },
                { THM06ParameterAddress.ModelName, THM06DataType.String8 },
                { THM06ParameterAddress.SlaveAddress, THM06DataType.UShort },
                { THM06ParameterAddress.BaudRate, THM06DataType.UShort },
                { THM06ParameterAddress.DataType, THM06DataType.UShort },
                { THM06ParameterAddress.OUT1, THM06DataType.Float },
                { THM06ParameterAddress.OUT2, THM06DataType.Float },
                { THM06ParameterAddress.Temperature, THM06DataType.Float },
                { THM06ParameterAddress.RelativeHumidity, THM06DataType.Float },
                { THM06ParameterAddress.DewpointTemperature, THM06DataType.Float },
                { THM06ParameterAddress.ForstPointTemperature, THM06DataType.Float },
                { THM06ParameterAddress.WetBulbTemperature, THM06DataType.Float },
                { THM06ParameterAddress.SaturationVapourPressure, THM06DataType.Float },
                { THM06ParameterAddress.VapourPressure, THM06DataType.Float },
                { THM06ParameterAddress.MixtureRatio, THM06DataType.Float },
                { THM06ParameterAddress.AbsoluteHumidity, THM06DataType.Float },
                { THM06ParameterAddress.SpecificEnthaply, THM06DataType.Float },
                { THM06ParameterAddress.PPMOnWeight, THM06DataType.Float },
                { THM06ParameterAddress.PPMOnVolume, THM06DataType.Float },
            };
        }

        public enum THM06ParameterAddress : ushort
        {
            Firmware = 0x0010,
            Serial = 0x0020,
            ModelName = 0x0644,
            SlaveAddress = 0x0030,
            BaudRate = 0x0032,
            DataType = 0x0034,
            OUT1 = 0x0000,
            OUT2 = 0x0004,
            Temperature = 0x0400,
            RelativeHumidity = 0x0404,
            DewpointTemperature = 0x0408,
            ForstPointTemperature = 0x040C,
            WetBulbTemperature = 0x0410,
            SaturationVapourPressure = 0x0414,
            VapourPressure = 0x0418,
            MixtureRatio = 0x041C,
            AbsoluteHumidity = 0x0420,
            SpecificEnthaply = 0x0424,
            PPMOnWeight = 0x0428,
            PPMOnVolume = 0x042C,
        }


        public enum THM06DataType
        {
            String5 = 5,
            String8 = 8,
            UShort = 1,
            Float = 2,
        }
    }
}

using System.IO.Ports;
using System.Runtime.CompilerServices;

namespace Oven.Utils.Modbus
{
    public class ModbusRtuMaster
    {
        private readonly SerialPort _serialPort;
        private readonly object _lock = new object(); // 用於同步的鎖對象

        public ModbusRtuMaster(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits)
            {
                ReadTimeout = 3000, // 讀取超時時間設定為3秒
                WriteTimeout = 3000 // 寫入超時時間設定為3秒
            };
            _serialPort.Open();
        }

        public ModbusRtuMaster(SerialPort serial)
        {
            _serialPort = serial;
        }

        public void Close()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        public byte[] ReadCoils(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
        {
            byte[] frame = BuildReadRequestFrame(slaveAddress, 0x01, startAddress, numberOfPoints);
            byte[] response = SendAndReceive(frame);
            return ParseReadResponse(response);
        }

        public byte[] ReadDiscreteInputs(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
        {
            byte[] frame = BuildReadRequestFrame(slaveAddress, 0x02, startAddress, numberOfPoints);
            byte[] response = SendAndReceive(frame);
            return ParseReadResponse(response);
        }

        public byte[] ReadHoldingRegisters(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
        {
            byte[] frame = BuildReadRequestFrame(slaveAddress, 0x03, startAddress, numberOfPoints);
            byte[] response = SendAndReceive(frame);
            return ParseReadResponse(response);
        }

        public byte[] ReadHoldingRegistersSync(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
        {
            byte[] frame = BuildReadRequestFrame(slaveAddress, 0x03, startAddress, numberOfPoints);
            //byte[] response = SendAndReceive(frame);
            byte[] response = SendAndReceiveNoDelay(frame);
            return ParseReadResponse(response);
        }

        public async Task<byte[]> ReadHoldingRegistersAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
        {
            byte[] frame = BuildReadRequestFrame(slaveAddress, 0x03, startAddress, numberOfPoints);
            byte[] response = await SendAndReceiveAsync(frame).ConfigureAwait(false);
            return ParseReadResponse(response);
        }

        public byte[] ReadInputRegisters(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
        {
            byte[] frame = BuildReadRequestFrame(slaveAddress, 0x04, startAddress, numberOfPoints);
            byte[] response = SendAndReceive(frame);
            return ParseReadResponse(response);
        }

        public void WriteSingleCoil(byte slaveAddress, ushort coilAddress, bool value)
        {
            byte[] frame = BuildWriteSingleCoilFrame(slaveAddress, coilAddress, value);
            SendAndReceive(frame);
        }

        public void WriteSingleRegister(byte slaveAddress, ushort registerAddress, ushort value)
        {
            byte[] frame = BuildWriteSingleRegisterFrame(slaveAddress, registerAddress, value);
            SendAndReceive(frame);
        }

        public void WriteMultipleCoils(byte slaveAddress, ushort startAddress, bool[] values)
        {
            byte[] frame = BuildWriteMultipleCoilsFrame(slaveAddress, startAddress, values);
            SendAndReceive(frame);
        }

        public void WriteMultipleRegisters(byte slaveAddress, ushort startAddress, ushort[] values)
        {
            byte[] frame = BuildWriteMultipleRegistersFrame(slaveAddress, startAddress, values);
            SendAndReceive(frame);
        }

        private byte[] BuildReadRequestFrame(byte slaveAddress, byte functionCode, ushort startAddress, ushort numberOfPoints)
        {
            byte[] frame = new byte[8];
            frame[0] = slaveAddress;
            frame[1] = functionCode;
            frame[2] = (byte)(startAddress >> 8);
            frame[3] = (byte)(startAddress & 0xFF);
            frame[4] = (byte)(numberOfPoints >> 8);
            frame[5] = (byte)(numberOfPoints & 0xFF);
            ushort crc = CalculateCrc(frame, 6);
            frame[6] = (byte)(crc & 0xFF);
            frame[7] = (byte)(crc >> 8);
            return frame;
        }

        private byte[] BuildWriteSingleCoilFrame(byte slaveAddress, ushort coilAddress, bool value)
        {
            byte[] frame = new byte[8];
            frame[0] = slaveAddress;
            frame[1] = 0x05;
            frame[2] = (byte)(coilAddress >> 8);
            frame[3] = (byte)(coilAddress & 0xFF);
            frame[4] = value ? (byte)0xFF : (byte)0x00;
            frame[5] = 0x00;
            ushort crc = CalculateCrc(frame, 6);
            frame[6] = (byte)(crc & 0xFF);
            frame[7] = (byte)(crc >> 8);
            return frame;
        }

        private byte[] BuildWriteSingleRegisterFrame(byte slaveAddress, ushort registerAddress, ushort value)
        {
            byte[] frame = new byte[8];
            frame[0] = slaveAddress;
            frame[1] = 0x06;
            frame[2] = (byte)(registerAddress >> 8);
            frame[3] = (byte)(registerAddress & 0xFF);
            frame[4] = (byte)(value >> 8);
            frame[5] = (byte)(value & 0xFF);
            ushort crc = CalculateCrc(frame, 6);
            frame[6] = (byte)(crc & 0xFF);
            frame[7] = (byte)(crc >> 8);
            return frame;
        }

        private byte[] BuildWriteMultipleCoilsFrame(byte slaveAddress, ushort startAddress, bool[] values)
        {
            int byteCount = (values.Length + 7) / 8;
            byte[] frame = new byte[9 + byteCount];
            frame[0] = slaveAddress;
            frame[1] = 0x0F;
            frame[2] = (byte)(startAddress >> 8);
            frame[3] = (byte)(startAddress & 0xFF);
            frame[4] = (byte)(values.Length >> 8);
            frame[5] = (byte)(values.Length & 0xFF);
            frame[6] = (byte)byteCount;

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i])
                    frame[7 + i / 8] |= (byte)(1 << i % 8);
            }

            ushort crc = CalculateCrc(frame, 7 + byteCount);
            frame[7 + byteCount] = (byte)(crc & 0xFF);
            frame[8 + byteCount] = (byte)(crc >> 8);
            return frame;
        }

        private byte[] BuildWriteMultipleRegistersFrame(byte slaveAddress, ushort startAddress, ushort[] values)
        {
            int byteCount = values.Length * 2;
            byte[] frame = new byte[9 + byteCount];
            frame[0] = slaveAddress;
            frame[1] = 0x10;
            frame[2] = (byte)(startAddress >> 8);
            frame[3] = (byte)(startAddress & 0xFF);
            frame[4] = (byte)(values.Length >> 8);
            frame[5] = (byte)(values.Length & 0xFF);
            frame[6] = (byte)byteCount;

            for (int i = 0; i < values.Length; i++)
            {
                frame[7 + i * 2] = (byte)(values[i] >> 8);
                frame[8 + i * 2] = (byte)(values[i] & 0xFF);
            }

            ushort crc = CalculateCrc(frame, 7 + byteCount);
            frame[7 + byteCount] = (byte)(crc & 0xFF);
            frame[8 + byteCount] = (byte)(crc >> 8);
            return frame;
        }

        private byte[] SendAndReceive(byte[] frame)
        {
            lock (_lock)
            {
                _serialPort.DiscardInBuffer();
                _serialPort.Write(frame, 0, frame.Length);

                Thread.Sleep(50); // 等待設備回應

                byte[] buffer = new byte[256];
                int bytesRead = 0;

                try
                {
                    bytesRead = _serialPort.Read(buffer, 0, buffer.Length);
                }
                catch (TimeoutException)
                {
                    throw new Exception("接收數據超時");
                }

                byte[] response = new byte[bytesRead];
                Array.Copy(buffer, response, bytesRead);

                return response;
            }
        }

        private byte[] SendAndReceiveNoDelay(byte[] frame)
        {
            int responseLength = GetExpectedResponseLength(frame);
            byte[] buffer = new byte[responseLength];
            //lock (_lock)
            //{
            // 確保不會同時進行發送和接收操作
            //Console.WriteLine("鎖定以發送資料...");
            _serialPort.DiscardInBuffer();
            _serialPort.Write(frame, 0, frame.Length);
            //Console.WriteLine("資料已發送");


            int bytesRead = 0;

            try
            {
                //Console.WriteLine("等待接收資料...");
                while (bytesRead < responseLength)
                {
                    int read = _serialPort.BaseStream.Read(buffer, bytesRead, responseLength - bytesRead);
                    //if (read == 0)
                    //{
                    //    Console.WriteLine("讀取超時");
                    //}
                    bytesRead += read;
                    //Console.WriteLine($"已讀取 {bytesRead}/{responseLength} 字節");
                }
            }
            catch (TimeoutException ex)
            {
                //Console.WriteLine($"接收資料時超時: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"接收資料時發生錯誤: {ex.Message}");
                throw;
            }
            //}
            return buffer;
        }

        private async Task<byte[]> SendAndReceiveAsync(byte[] frame)
        {

            int responseLength = GetExpectedResponseLength(frame);
            byte[] buffer = new byte[responseLength];
            lock (_lock)
            {
                try
                {
                    // 確保不會同時進行發送和接收操作
                    _serialPort.DiscardInBuffer();
                    _serialPort.Write(frame, 0, frame.Length);

                    int bytesRead = 0;

                    while (bytesRead < responseLength)
                    {
                        int read = _serialPort.BaseStream.Read(buffer, bytesRead, responseLength - bytesRead);
                        //if (read == 0)
                        //{
                        //    Console.WriteLine("讀取超時");
                        //}
                        bytesRead += read;
                    }
                }
                catch (TimeoutException ex)
                {
                    Logger.Instance.writeMessage("","", ex.Message);
                }
                catch (Exception ex)
                {
                    Logger.Instance.writeMessage("", "", ex.Message);
                }
            }
            return buffer;
        }

        private int GetExpectedResponseLength(byte[] frame)
        {
            byte functionCode = frame[1];
            int baseLength = 5; // 包含地址、功能碼、CRC和數據長度
            int dataLength;

            switch (functionCode)
            {
                case 0x01: // 讀取線圈
                case 0x02: // 讀取離散輸入
                    dataLength = (frame[4] << 8) + frame[5];
                    return baseLength + (dataLength + 7) / 8; // 加上數據的字節數
                case 0x03: // 讀取保持寄存器
                case 0x04: // 讀取輸入寄存器
                    dataLength = (frame[4] << 8) + frame[5];
                    return baseLength + dataLength * 2; // 加上數據的字節數
                default:
                    throw new InvalidOperationException("不支持的功能碼");
            }
        }


        private byte[] ParseReadResponse(byte[] response)
        {
            // 檢查CRC
            ushort crc = BitConverter.ToUInt16(response, response.Length - 2);
            if (crc != CalculateCrc(response, response.Length - 2))
            {
                throw new InvalidOperationException("CRC校驗失敗");
            }

            byte byteCount = response[2];
            byte[] data = new byte[byteCount];
            Array.Copy(response, 3, data, 0, byteCount);
            return data;
        }

        private ushort CalculateCrc(byte[] data, int length)
        {
            ushort crc = 0xFFFF;

            for (int pos = 0; pos < length; pos++)
            {
                crc ^= data[pos];

                for (int i = 8; i != 0; i--)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }

            return crc;
        }
    }
}

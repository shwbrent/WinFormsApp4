using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class ModbusASCIIMaster
    {
        private readonly SerialPort _serialPort;
        private readonly object _lock = new object(); // 用於同步的鎖對象

        public ModbusASCIIMaster(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits)
            {
                ReadTimeout = 3000, // 讀取超時時間設定為3秒
                WriteTimeout = 3000 // 寫入超時時間設定為3秒
            };
            _serialPort.Open();
        }

        public void Close()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        public byte[] ReadHoldingRegisters(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
        {
            byte[] frame = BuildReadRequestFrame(slaveAddress, 0x03, startAddress, numberOfPoints);
            byte[] response = SendAndReceive(frame);
            return ParseReadResponse(response);
        }

        private byte[] BuildReadRequestFrame(byte slaveAddress, byte functionCode, ushort startAddress, ushort numberOfPoints)
        {
            byte[] frame = new byte[6];
            frame[0] = slaveAddress;
            frame[1] = functionCode;
            frame[2] = (byte)(startAddress >> 8);
            frame[3] = (byte)(startAddress & 0xFF);
            frame[4] = (byte)(numberOfPoints >> 8);
            frame[5] = (byte)(numberOfPoints & 0xFF);

            return AddLRCAndConvertToAscii(frame);
        }

        private byte[] AddLRCAndConvertToAscii(byte[] frame)
        {
            byte lrc = CalculateLrc(frame);
            byte[] frameWithLrc = new byte[frame.Length + 1];
            Array.Copy(frame, frameWithLrc, frame.Length);
            frameWithLrc[frame.Length] = lrc;

            StringBuilder asciiFrame = new StringBuilder(":");
            foreach (byte b in frameWithLrc)
            {
                asciiFrame.AppendFormat("{0:X2}", b);
            }
            asciiFrame.Append("\r\n");

            return Encoding.ASCII.GetBytes(asciiFrame.ToString());
        }

        private byte CalculateLrc(byte[] data)
        {
            byte lrc = 0;
            foreach (byte b in data)
            {
                lrc += b;
            }
            lrc = (byte)((~lrc + 1) & 0xFF);
            return lrc;
        }

        private byte[] SendAndReceive(byte[] frame)
        {
            lock (_lock)
            {
                _serialPort.DiscardInBuffer();
                _serialPort.Write(frame, 0, frame.Length);

                Thread.Sleep(100); // 等待設備回應

                StringBuilder responseBuilder = new StringBuilder();
                bool frameStarted = false;

                while (true)
                {
                    try
                    {
                        char receivedChar = (char)_serialPort.ReadChar();
                        if (receivedChar == ':')
                        {
                            frameStarted = true;
                            responseBuilder.Clear();
                        }

                        if (frameStarted)
                        {
                            responseBuilder.Append(receivedChar);
                            if (receivedChar == '\r' || receivedChar == '\n')
                            {
                                break;
                            }
                        }
                    }
                    catch (TimeoutException)
                    {
                        throw new Exception("接收數據超時");
                    }
                }

                string responseString = responseBuilder.ToString().Trim();
                if (responseString.StartsWith(":"))
                {
                    responseString = responseString.Substring(1, responseString.Length - 1); // 去掉開始的":"
                    return ConvertAsciiToByteArray(responseString);
                }

                throw new Exception("無效的回應格式");
            }
        }

        private byte[] ConvertAsciiToByteArray(string ascii)
        {
            byte[] bytes = new byte[ascii.Length / 2];
            for (int i = 0; i < ascii.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(ascii.Substring(i, 2), 16);
            }
            return bytes;
        }

        private byte[] ParseReadResponse(byte[] response)
        {
            // 簡單解析，未進行詳細錯誤處理
            if (response.Length < 3)
            {
                throw new Exception("回應長度無效");
            }

            byte lrc = CalculateLrc(response, response.Length - 1);
            if (lrc != response[response.Length - 1])
            {
                throw new Exception("LRC校驗失敗");
            }

            byte byteCount = response[2];
            byte[] data = new byte[byteCount];
            Array.Copy(response, 3, data, 0, byteCount);
            return data;
        }

        private byte CalculateLrc(byte[] data, int length)
        {
            byte lrc = 0;
            for (int i = 0; i < length; i++)
            {
                lrc += data[i];
            }
            lrc = (byte)((~lrc + 1) & 0xFF);
            return lrc;
        }
    }

}

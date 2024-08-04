﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Oven.Utils.Modbus
{
    public class ModbusTcpServer
    {
        private readonly TcpListener _listener;
        private readonly bool[] _coils;
        private readonly bool[] _discreteInputs;
        private readonly ushort[] _holdingRegisters;
        private readonly ushort[] _inputRegisters;
        private readonly byte _unitId;

        public ModbusTcpServer(string ipAddress, int port, byte unitId)
        {
            _listener = new TcpListener(IPAddress.Parse(ipAddress), port);
            _unitId = unitId;
            _coils = new bool[10000]; // 假設我們有10000個線圈
            _discreteInputs = new bool[10000]; // 假設我們有10000個離散輸入
            _holdingRegisters = new ushort[10000]; // 假設我們有10000個保持寄存器
            _inputRegisters = new ushort[10000]; // 假設我們有10000個輸入寄存器
        }

        public void Start()
        {
            _listener.Start();
            Console.WriteLine("Modbus TCP Server is running...");
            while (true)
            {
                try
                {
                    var client = _listener.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(HandleClient, client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting client: {ex.Message}");
                }
            }
        }

        private void HandleClient(object obj)
        {
            var client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();

            // 設置Keep-Alive選項
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            byte[] buffer = new byte[256];
            int bufferOffset = 0;

            while (client.Connected)
            {
                try
                {
                    int bytesRead = stream.Read(buffer, bufferOffset, buffer.Length - bufferOffset);
                    if (bytesRead > 0)
                    {
                        bufferOffset += bytesRead;
                        ProcessBuffer(stream, buffer, ref bufferOffset);
                    }
                    else
                    {
                        Thread.Sleep(1000); // 檢查連線狀態，每秒鐘檢查一次
                    }
                }
                catch (IOException ioEx)
                {
                    Console.WriteLine($"IOException: {ioEx.Message}");
                    break;
                }
                catch (SocketException sockEx)
                {
                    Console.WriteLine($"SocketException: {sockEx.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                    break;
                }
            }

            client.Close();
        }

        private void ProcessBuffer(NetworkStream stream, byte[] buffer, ref int bufferOffset)
        {
            int processedOffset = 0;

            while (bufferOffset - processedOffset >= 6)
            {
                int length = (buffer[processedOffset + 4] << 8) + buffer[processedOffset + 5];
                if (bufferOffset - processedOffset >= length + 6)
                {
                    byte[] request = new byte[length + 6];
                    Buffer.BlockCopy(buffer, processedOffset, request, 0, length + 6);
                    processedOffset += length + 6;

                    byte[] response = ProcessRequest(request, request.Length);
                    if (response != null)
                    {
                        stream.Write(response, 0, response.Length);
                    }
                }
                else
                {
                    break;
                }
            }

            if (processedOffset > 0)
            {
                Buffer.BlockCopy(buffer, processedOffset, buffer, 0, bufferOffset - processedOffset);
                bufferOffset -= processedOffset;
            }
        }

        private byte[]? ProcessRequest(byte[] request, int bytesRead)
        {
            // 簡單的檢查請求格式是否正確
            if (bytesRead < 12)
                return BuildErrorResponse(request, 0x01); // 非法功能

            byte unitId = request[6];
            byte functionCode = request[7];
            ushort startAddress = (ushort)((request[8] << 8) + request[9]);
            ushort quantityOfRegisters = (ushort)((request[10] << 8) + request[11]);

            if (unitId != _unitId)
                return null; // Unit ID不匹配，忽略請求

            switch (functionCode)
            {
                case 0x01: // 讀取線圈
                    return BuildReadCoilsResponse(request, unitId, startAddress, quantityOfRegisters);
                case 0x02: // 讀取離散輸入
                    return BuildReadDiscreteInputsResponse(request, unitId, startAddress, quantityOfRegisters);
                case 0x03: // 讀取保持寄存器
                    return BuildReadHoldingRegistersResponse(request, unitId, startAddress, quantityOfRegisters);
                case 0x04: // 讀取輸入寄存器
                    return BuildReadInputRegistersResponse(request, unitId, startAddress, quantityOfRegisters);
                case 0x05: // 寫單個線圈
                    return BuildWriteSingleCoilResponse(request, unitId, startAddress);
                case 0x06: // 寫單個保持寄存器
                    return BuildWriteSingleRegisterResponse(request, unitId, startAddress);
                case 0x0F: // 寫多個線圈
                    return BuildWriteMultipleCoilsResponse(request, unitId, startAddress, quantityOfRegisters);
                case 0x10: // 寫多個保持寄存器
                    return BuildWriteMultipleRegistersResponse(request, unitId, startAddress, quantityOfRegisters);
                default:
                    return BuildErrorResponse(request, 0x01); // 非法功能
            }
        }

        private byte[] BuildReadCoilsResponse(byte[] request, byte unitId, ushort startAddress, ushort quantityOfCoils)
        {
            int byteCount = (quantityOfCoils + 7) / 8;
            byte[] response = new byte[9 + byteCount];

            // Transaction Identifier
            Buffer.BlockCopy(request, 0, response, 0, 4);

            // Length
            response[4] = 0x00;
            response[5] = (byte)(3 + byteCount);

            // Unit Identifier
            response[6] = unitId;

            // Function Code
            response[7] = 0x01;

            // Byte Count
            response[8] = (byte)byteCount;

            // Coil Status
            for (int i = 0; i < quantityOfCoils; i++)
            {
                if (_coils[startAddress + i])
                    response[9 + i / 8] |= (byte)(1 << i % 8);
            }

            return response;
        }

        private byte[] BuildReadDiscreteInputsResponse(byte[] request, byte unitId, ushort startAddress, ushort quantityOfInputs)
        {
            int byteCount = (quantityOfInputs + 7) / 8;
            byte[] response = new byte[9 + byteCount];

            // Transaction Identifier
            Buffer.BlockCopy(request, 0, response, 0, 4);

            // Length
            response[4] = 0x00;
            response[5] = (byte)(3 + byteCount);

            // Unit Identifier
            response[6] = unitId;

            // Function Code
            response[7] = 0x02;

            // Byte Count
            response[8] = (byte)byteCount;

            // Discrete Input Status
            for (int i = 0; i < quantityOfInputs; i++)
            {
                if (_discreteInputs[startAddress + i])
                    response[9 + i / 8] |= (byte)(1 << i % 8);
            }

            return response;
        }

        private byte[] BuildReadHoldingRegistersResponse(byte[] request, byte unitId, ushort startAddress, ushort quantityOfRegisters)
        {
            byte[] response = new byte[9 + quantityOfRegisters * 2];

            // Transaction Identifier
            Buffer.BlockCopy(request, 0, response, 0, 4);

            // Length
            response[4] = 0x00;
            response[5] = (byte)(3 + quantityOfRegisters * 2);

            // Unit Identifier
            response[6] = unitId;

            // Function Code
            response[7] = 0x03;

            // Byte Count
            response[8] = (byte)(quantityOfRegisters * 2);

            // Register Values
            for (int i = 0; i < quantityOfRegisters; i++)
            {
                ushort value = _holdingRegisters[startAddress + i];
                response[9 + i * 2] = (byte)(value >> 8);
                response[10 + i * 2] = (byte)(value & 0xFF);
            }

            return response;
        }

        private byte[] BuildReadInputRegistersResponse(byte[] request, byte unitId, ushort startAddress, ushort quantityOfRegisters)
        {
            byte[] response = new byte[9 + quantityOfRegisters * 2];

            // Transaction Identifier
            Buffer.BlockCopy(request, 0, response, 0, 4);

            // Length
            response[4] = 0x00;
            response[5] = (byte)(3 + quantityOfRegisters * 2);

            // Unit Identifier
            response[6] = unitId;

            // Function Code
            response[7] = 0x04;

            // Byte Count
            response[8] = (byte)(quantityOfRegisters * 2);

            // Register Values
            for (int i = 0; i < quantityOfRegisters; i++)
            {
                ushort value = _inputRegisters[startAddress + i];
                response[9 + i * 2] = (byte)(value >> 8);
                response[10 + i * 2] = (byte)(value & 0xFF);
            }

            return response;
        }

        private byte[] BuildWriteSingleCoilResponse(byte[] request, byte unitId, ushort coilAddress)
        {
            bool coilValue = request[10] == 0xFF;

            _coils[coilAddress] = coilValue;

            byte[] response = new byte[12];

            // Transaction Identifier
            Buffer.BlockCopy(request, 0, response, 0, 12);

            return response;
        }

        private byte[] BuildWriteSingleRegisterResponse(byte[] request, byte unitId, ushort registerAddress)
        {
            ushort registerValue = (ushort)((request[10] << 8) + request[11]);

            _holdingRegisters[registerAddress] = registerValue;

            byte[] response = new byte[12];

            // Transaction Identifier
            Buffer.BlockCopy(request, 0, response, 0, 12);

            return response;
        }

        private byte[] BuildWriteMultipleCoilsResponse(byte[] request, byte unitId, ushort startAddress, ushort quantityOfCoils)
        {
            int byteCount = request[12];
            byte[] response = new byte[12];

            // Transaction Identifier
            Buffer.BlockCopy(request, 0, response, 0, 4);

            // Length
            response[4] = 0x00;
            response[5] = 0x06;

            // Unit Identifier
            response[6] = unitId;

            // Function Code
            response[7] = 0x0F;

            // Starting Address
            response[8] = request[8];
            response[9] = request[9];

            // Quantity of Coils
            response[10] = request[10];
            response[11] = request[11];

            // Update Coils
            for (int i = 0; i < quantityOfCoils; i++)
            {
                bool coilValue = (request[13 + i / 8] & 1 << i % 8) != 0;
                _coils[startAddress + i] = coilValue;
            }

            return response;
        }

        private byte[] BuildWriteMultipleRegistersResponse(byte[] request, byte unitId, ushort startAddress, ushort quantityOfRegisters)
        {
            byte[] response = new byte[12];

            // Transaction Identifier
            Buffer.BlockCopy(request, 0, response, 0, 4);

            // Length
            response[4] = 0x00;
            response[5] = 0x06;

            // Unit Identifier
            response[6] = unitId;

            // Function Code
            response[7] = 0x10;

            // Starting Address
            response[8] = request[8];
            response[9] = request[9];

            // Quantity of Registers
            response[10] = request[10];
            response[11] = request[11];

            // Update Registers
            for (int i = 0; i < quantityOfRegisters; i++)
            {
                ushort value = (ushort)((request[13 + i * 2] << 8) + request[14 + i * 2]);
                _holdingRegisters[startAddress + i] = value;
            }

            return response;
        }

        private byte[] BuildErrorResponse(byte[] request, byte errorCode)
        {
            byte[] response = new byte[9];

            // Transaction Identifier
            Buffer.BlockCopy(request, 0, response, 0, 4);

            // Length
            response[4] = 0x00;
            response[5] = 0x03;

            // Unit Identifier
            response[6] = request[6];

            // Function Code (with error flag)
            response[7] = (byte)(request[7] | 0x80);

            // Error Code
            response[8] = errorCode;

            return response;
        }

        public void UpdateInputRegister(int address, ushort value)
        {
            if (address >= 0 && address < _inputRegisters.Length)
            {
                _inputRegisters[address] = value;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Address out of range");
            }
        }

        public void UpdateCoil(int address, bool value)
        {
            if (address >= 0 && address < _coils.Length)
            {
                _coils[address] = value;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Address out of range");
            }
        }

        public void UpdateDiscreteInput(int address, bool value)
        {
            if (address >= 0 && address < _discreteInputs.Length)
            {
                _discreteInputs[address] = value;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Address out of range");
            }
        }

        public void UpdateHoldingRegister(int address, ushort value)
        {
            if (address >= 0 && address < _holdingRegisters.Length)
            {
                _holdingRegisters[address] = value;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Address out of range");
            }
        }

        public ushort ReadInputRegister(int address)
        {
            ushort rst;
            if (address >= 0 && address < _inputRegisters.Length)
            {
                rst = _inputRegisters[address];
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Address out of range");
            }
            return rst;
        }

        public bool ReadCoil(int address)
        {
            bool rst;
            if (address >= 0 && address < _coils.Length)
            {
                rst = _coils[address];
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Address out of range");
            }
            return rst;
        }

        public bool ReadDiscreteInput(int address)
        {
            bool rst;
            if (address >= 0 && address < _discreteInputs.Length)
            {
                rst = _discreteInputs[address];
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Address out of range");
            }
            return rst;
        }

        public ushort ReadHoldingRegister(int address)
        {
            ushort rst = 0;
            if (address >= 0 && address < _holdingRegisters.Length)
            {
                rst = _holdingRegisters[address];
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Address out of range");
            }
            return rst;
        }
    }

}

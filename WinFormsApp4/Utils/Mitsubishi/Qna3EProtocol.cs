using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Qna3EProtocol
    {
        private string ipAddress;
        private int port;
        private TcpClient client;
        private NetworkStream stream;

        public Qna3EProtocol(string ipAddress, int port)
        {
            this.ipAddress = ipAddress;
            this.port = port;
        }

        public void Connect()
        {
            client = new TcpClient(ipAddress, port);
            stream = client.GetStream();
        }

        public void Disconnect()
        {
            stream.Close();
            client.Close();
        }

        private byte[] BuildFrame(byte[] command)
        {
            // 建立3E協定的訊息框架
            byte[] frameHeader = new byte[] { 0x50, 0x00 };
            byte[] networkNumber = new byte[] { 0x00 };
            byte[] pcNumber = new byte[] { 0xFF };
            byte[] requestDestinationModuleIONumber = new byte[] { 0x03, 0xFF };
            byte[] requestDestinationModuleStationNumber = new byte[] { 0x00 };
            byte[] monitoringTimer = new byte[] { 0x10, 0x00 };
            byte[] commandLength = BitConverter.GetBytes((ushort)command.Length);

            byte[] frame = new byte[frameHeader.Length + networkNumber.Length + pcNumber.Length + requestDestinationModuleIONumber.Length +
                                    requestDestinationModuleStationNumber.Length + monitoringTimer.Length + commandLength.Length + command.Length];

            Buffer.BlockCopy(frameHeader, 0, frame, 0, frameHeader.Length);
            Buffer.BlockCopy(networkNumber, 0, frame, frameHeader.Length, networkNumber.Length);
            Buffer.BlockCopy(pcNumber, 0, frame, frameHeader.Length + networkNumber.Length, pcNumber.Length);
            Buffer.BlockCopy(requestDestinationModuleIONumber, 0, frame, frameHeader.Length + networkNumber.Length + pcNumber.Length, requestDestinationModuleIONumber.Length);
            Buffer.BlockCopy(requestDestinationModuleStationNumber, 0, frame, frameHeader.Length + networkNumber.Length + pcNumber.Length + requestDestinationModuleIONumber.Length, requestDestinationModuleStationNumber.Length);
            Buffer.BlockCopy(monitoringTimer, 0, frame, frameHeader.Length + networkNumber.Length + pcNumber.Length + requestDestinationModuleIONumber.Length + requestDestinationModuleStationNumber.Length, monitoringTimer.Length);
            Buffer.BlockCopy(commandLength, 0, frame, frameHeader.Length + networkNumber.Length + pcNumber.Length + requestDestinationModuleIONumber.Length + requestDestinationModuleStationNumber.Length + monitoringTimer.Length, commandLength.Length);
            Buffer.BlockCopy(command, 0, frame, frameHeader.Length + networkNumber.Length + pcNumber.Length + requestDestinationModuleIONumber.Length + requestDestinationModuleStationNumber.Length + monitoringTimer.Length + commandLength.Length, command.Length);

            return frame;
        }

        private byte[] BuildReadCommand(string address, ushort points)
        {
            byte[] subheader = new byte[] { 0x04, 0x01 };
            byte[] dataCode = Encoding.ASCII.GetBytes(address.Substring(0, 1));
            ushort dataAddress = Convert.ToUInt16(address.Substring(1));
            byte[] dataAddressBytes = BitConverter.GetBytes(dataAddress);
            byte[] pointsBytes = BitConverter.GetBytes(points);

            byte[] command = new byte[subheader.Length + dataCode.Length + dataAddressBytes.Length + pointsBytes.Length];
            Buffer.BlockCopy(subheader, 0, command, 0, subheader.Length);
            Buffer.BlockCopy(dataCode, 0, command, subheader.Length, dataCode.Length);
            Buffer.BlockCopy(dataAddressBytes, 0, command, subheader.Length + dataCode.Length, dataAddressBytes.Length);
            Buffer.BlockCopy(pointsBytes, 0, command, subheader.Length + dataCode.Length + dataAddressBytes.Length, pointsBytes.Length);

            return command;
        }

        public byte[] SendCommand(string address, ushort points)
        {
            byte[] command = BuildReadCommand(address, points);
            byte[] frame = BuildFrame(command);
            stream.Write(frame, 0, frame.Length);

            byte[] response = new byte[256];
            int bytesRead = stream.Read(response, 0, response.Length);

            byte[] result = new byte[bytesRead];
            Buffer.BlockCopy(response, 0, result, 0, bytesRead);
            return result;
        }
    }
}

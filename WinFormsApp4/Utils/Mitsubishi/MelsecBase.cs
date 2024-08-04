using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mitsubishi.Communication.MC.Mitsubishi.Base
{
    /// <summary>
    /// mc協議基類
    /// </summary>
    public class MelsecBase
    {
        /// <summary>
        /// plc的ip地址
        /// </summary>
        public string _ip;
        /// <summary>
        /// plc的端口號
        /// </summary>
        public int _port;
        /// <summary>
        /// socket對象
        /// </summary>
        public Socket socket = null;
        /// <summary>
        /// 超時事件
        /// </summary>
        ManualResetEvent TimeoutObject = new ManualResetEvent(false);
        /// <summary>
        /// 連接狀態
        /// </summary>
        bool connectState = false;

        /// <summary>
        /// 構造方法
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public MelsecBase(string ip, short port)
        {
            _ip = ip;
            _port = port;
            // 初始化一個通信對象
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// 連接PLC
        /// </summary>
        /// <param name="timeout">超時時間</param>
        /// <returns></returns>
        public Result Connect(int timeout = 50)
        {
            TimeoutObject.Reset();
            Result result = new Result();
            try
            {
                if (socket == null)
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
                int count = 0;
                while (count < timeout)
                {
                    if (!(!socket.Connected || (socket.Poll(200, SelectMode.SelectRead) && (socket.Available == 0))))
                    {
                        return result;
                    }
                    try
                    {
                        socket?.Close();
                        socket.Dispose();
                        socket = null;

                        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        //異步連接 
                        socket.BeginConnect(_ip, _port, callback =>
                        {
                            connectState = false;
                            var cbSocket = callback.AsyncState as Socket;
                            if (cbSocket != null)
                            {
                                connectState = cbSocket.Connected;
                                if (cbSocket.Connected)
                                {
                                    cbSocket.EndConnect(callback);
                                }
                            }
                            TimeoutObject.Set();
                        }, socket);
                        TimeoutObject.WaitOne(2000, false);
                        if (!connectState)
                        {
                            throw new Exception();
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (SocketException ex)
                    {
                        if (ex.ErrorCode == 10060)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        count++;
                    }
                }
                if (socket == null || !socket.Connected || ((socket.Poll(200, SelectMode.SelectRead) && (socket.Available == 0))))
                {
                    throw new Exception("網絡連接失敗");
                }
            }
            catch (Exception ex)
            {
                result.IsSuccessed = false;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 構建開始地址
        /// </summary>
        /// <param name="areaCode">存儲區</param>
        /// <param name="startAddr">開始地址</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<byte> StartToBytes(AreaCode areaCode, string startAddr)
        {
            List<byte> startBytes = new List<byte>();
            if (areaCode == AreaCode.X || areaCode == AreaCode.Y)
            {
                string str = startAddr.ToString().PadLeft(8, '0');
                for (int i = str.Length - 2; i >= 0; i -= 2)
                {
                    string v = str[i].ToString() + str[i + 1].ToString();
                    startBytes.Add(Convert.ToByte(v, 16));
                }
            }
            else
            {
                int addr = 0;
                if (!int.TryParse(startAddr, out addr))
                {
                    throw new Exception("軟元件地址不支持！");
                }
                startBytes.Add((byte)(addr & 0xFF));
                startBytes.Add((byte)((addr >> 8) & 0xFF));
                startBytes.Add((byte)((addr >> 16) & 0xFF));
                startBytes.Add((byte)((addr >> 24) & 0xFF));
            }

            return startBytes;
        }

        /// <summary>
        /// 發送報文
        /// </summary>
        /// <param name="reqBytes">字節集合</param>
        /// <param name="count">字節長度</param>
        /// <returns></returns>
        public virtual List<byte> Send(List<byte> reqBytes, int count)
        {
            return null;
        }

        /// <summary>
        /// 數據解析
        /// </summary>
        /// <typeparam name="T">讀取的數據類型</typeparam>
        /// <param name="datas">數據列表</param>
        /// <param name="typeLen">類型長度</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<T> AnalysisDatas<T>(List<byte> datas, int typeLen)
        {
            List<T> resultDatas = new List<T>();
            if (typeof(T) == typeof(bool))//bool類型
            {
                for (int i = 0; i < datas.Count; i++)
                {
                    // 10 10 10 10 10
                    string binaryStr = Convert.ToString(datas[i], 2).PadLeft(8, '0');
                    dynamic state = binaryStr.Substring(0, 4) == "0001";
                    resultDatas.Add(state);
                    state = binaryStr.Substring(4) == "0001";
                    resultDatas.Add(state);
                }
            }
            else//其他類型：ushort,short,float
            {
                for (int i = 0; i < datas.Count;)
                {
                    List<byte> valueByte = new List<byte>();
                    for (int sit = 0; sit < typeLen * 2; sit++)
                    {
                        valueByte.Add(datas[i++]);
                    }
                    Type tBitConverter = typeof(BitConverter);
                    MethodInfo method = tBitConverter.GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(mi => mi.ReturnType == typeof(T)) as MethodInfo;
                    if (method == null)
                    {
                        throw new Exception("未找到匹配的數據類型轉換方法");
                    }
                    resultDatas.Add((T)method?.Invoke(tBitConverter, new object[] { valueByte.ToArray(), 0 }));
                }
            }

            return resultDatas;
        }

        /// <summary>
        /// 計算長度
        /// </summary>
        /// <typeparam name="T">讀取的數據類型</typeparam>
        /// <returns></returns>
        public int CalculatLength<T>()
        {
            int typeLen = 1;
            if (!typeof(T).Equals(typeof(bool)))
            {
                typeLen = Marshal.SizeOf<T>() / 2;// 每一個數據需要多少個寄存器
            }
            return typeLen;
        }

        /// <summary>
        /// 獲取數據的字節列表
        /// </summary>
        /// <typeparam name="T">數據類型</typeparam>
        /// <param name="values">數據列表</param>
        /// <returns></returns>
        public List<byte> GetDataBytes<T>(List<T> values)
        {
            List<byte> datas = new List<byte>();
            int count = values.Count;
            if (typeof(T) == typeof(bool))//bool類型的數據
            {
                dynamic value = false;
                // 添加一個填充數據，保存一個完整字節
                if (values.Count % 2 > 0)
                {
                    values.Add(value);
                }

                for (int i = 0; i < values.Count; i += 2)
                {
                    byte valueByte = 0;
                    if (bool.Parse(values[i].ToString()))
                    {
                        valueByte |= 16;
                    }
                    if (bool.Parse(values[i + 1].ToString()))
                    {
                        valueByte |= 1;
                    }
                    datas.Add(valueByte);
                }
            }
            else //其他類型：float，short，int16
            {
                for (int i = 0; i < values.Count; i++)
                {
                    dynamic value = values[i];
                    datas.AddRange(BitConverter.GetBytes(value)); // MC不需要字節的顛倒
                }
            }

            return datas;
        }
    }
}
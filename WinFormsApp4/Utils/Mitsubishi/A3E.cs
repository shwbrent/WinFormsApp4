using Mitsubishi.Communication.MC.Mitsubishi.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace Mitsubishi.Communication.MC.Mitsubishi
{
    /// <summary>
    /// A3E報文通訊庫
    /// </summary>
    public class A3E : MelsecBase
    {
        byte _netCode = 0x00, _stationCode = 0x00;

        public A3E(string ip, short port, byte net_code = 0x00, byte station_code = 0x00) : base(ip, port)
        {
            _netCode = net_code;
            _stationCode = station_code;
        }

        #region 讀資料
        /// <summary>
        /// 讀取資料
        /// </summary>
        /// <typeparam name="T">讀取的資料型態</typeparam>
        /// <param name="address">存儲區地址</param>
        /// <param name="count">讀取長度</param>
        /// <returns></returns>
        public async Task<Result<T>> Read<T>(string address, short count)
        {
            AreaCode areaCode; string start;
            (areaCode, start) = this.AnalysisAddress(address);
            return Read<T>(areaCode, start, count);
        }

        /// <summary>
        /// 讀取資料
        /// </summary>
        /// <typeparam name="T">讀取的資料型態</typeparam>
        /// <param name="areaCode">存儲區代碼</param>
        /// <param name="startAddr">開始地址</param>
        /// <param name="count">讀取長度</param>
        /// <returns></returns>
        public Result<T> Read<T>(AreaCode areaCode, string startAddr, short count)
        {
            Result<T> result = new Result<T>();
            try
            {
                // 連接
                var connectState = this.Connect();
                if (!connectState.IsSuccessed)
                {
                    throw new Exception(connectState.Message);
                }
                // 子命令（位/字）
                byte readCode = (byte)(typeof(T) == typeof(bool) ? 0x01 : 0x00);
                //開始地址
                List<byte> startBytes = this.StartToBytes(areaCode, startAddr);
                // 讀取長度
                int typeLen = this.CalculatLength<T>();

                // 讀取報文
                List<byte> bytes = new List<byte>
            {
                0x50,0x00,//請求副頭部,固定50 00
                _netCode,// 網絡號，根據PLC的設置
                0xFF,//PLC編號,固定值
                0xFF,0x03,//目標模組IO編號，固定FF 03
                _stationCode,// 目標模組站號 
                0x0C,0x00,  // 剩餘字節長度
                0x0A,0x00,//PLC響應超時時間，以250ms為單位計算 
                0x01,0x04,// 成批讀取
                readCode,0x00,// 字操作  0x0001 
                startBytes[0],startBytes[1],startBytes[2],// 起始地址
                (byte)areaCode,// 區域代碼 
                (byte)((typeLen*count) & 0xFF),
                (byte)(((typeLen*count) >> 8) & 0xFF) //長度
            };
                //發送報文
                List<byte> dataBytes = this.Send(bytes, 0);
                //資料解析
                result.Datas = this.AnalysisDatas<T>(dataBytes, typeLen);
            }
            catch (Exception ex)
            {
                result = new Result<T>(false, ex.Message);
            }
            return result;
        }
        #endregion

        #region 寫資料

        /// <summary>
        /// 寫入資料
        /// </summary>
        /// <typeparam name="T">寫入的資料型態</typeparam>
        /// <param name="values">寫入的資料列表</param>
        /// <param name="address">開始地址</param>
        /// <returns></returns>
        public Result Write<T>(List<T> values, string address)
        {
            AreaCode areaCode; string start;
            (areaCode, start) = this.AnalysisAddress(address);
            return this.Write<T>(values, areaCode, start);
        }

        /// <summary>
        /// 寫入資料
        /// </summary>
        /// <typeparam name="T">寫入的資料型態</typeparam>
        /// <param name="values">寫入的資料列表</param>
        /// <param name="areaCode">存儲區代碼</param>
        /// <param name="address">開始地址</param>
        /// <returns></returns>
        public Result Write<T>(List<T> values, AreaCode areaCode, string startAddr)
        {
            Result result = new Result();

            try
            {
                // 連接
                var connectState = this.Connect();
                if (!connectState.IsSuccessed)
                {
                    throw new Exception(connectState.Message);
                }
                // 子命令（位/字）
                byte writeCode = (byte)(typeof(T) == typeof(bool) ? 0x01 : 0x00);

                // 起始地址  XY    直接翻譯  100   00 01 00    D100  64 00 00
                List<byte> startBytes = this.StartToBytes(areaCode, startAddr);
                //計算資料型態的長度
                int typeLen = this.CalculatLength<T>();
                int count = values.Count;
                //計算資料的字節列表
                List<byte> datas = this.GetDataBytes<T>(values);
                List<byte> baseBytes = new List<byte>
            {
                0x50,0x00,
                this._netCode,// 可變，根據PLC的設置
                0xFF,//PLC編號,固定值
                0xFF,0x03,//目標模組IO編號，固定FF 03
                this._stationCode,// 可變,目標模組站號
            };
                //0x0E,0x00,  // 剩餘字節長度
                List<byte> commandBytes = new List<byte> {
                0x0A,0x00,//超時時間
                0x01,0x14,// 成批寫入
                writeCode,0x00,// 字操作
                startBytes[0],startBytes[1],startBytes[2],// 起始地址
                (byte)areaCode,// 區域代碼 
                (byte)(typeLen*count%256),
                (byte)(typeLen*count/256%256), //長度
            };
                commandBytes.AddRange(datas);
                baseBytes.Add((byte)(commandBytes.Count % 256));
                baseBytes.Add((byte)(commandBytes.Count / 256 % 256));
                baseBytes.AddRange(commandBytes);
                socket.Send(baseBytes.ToArray());

                // 解析響應
                byte[] respBytes = new byte[11];
                socket.Receive(respBytes, 0, 11, SocketFlags.None);

                // 狀態
                if ((respBytes[9] | respBytes[10]) != 0x00)
                {
                    throw new Exception("響應異常。" + respBytes[9].ToString() + respBytes[10].ToString());
                }
            }
            catch (Exception ex)
            {
                result.IsSuccessed = false;
                result.Message = ex.Message;
            }

            return result;
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 地址解析
        /// </summary>
        /// <param name="address">地址字串</param>
        /// <returns></returns>
        public Tuple<AreaCode, string> AnalysisAddress(string address)
        {
            // 取兩個字元
            string area = address.Substring(0, 2);
            if (!new string[] { "TN", "TS", "CS", "CN" }.Contains(area))
            {
                area = address.Substring(0, 1);
            }
            string start = address.Substring(area.Length);

            // 返回一個元組對象 
            return new Tuple<AreaCode, string>((AreaCode)Enum.Parse(typeof(AreaCode), area), start);
        }

        /// <summary>
        /// 發送報文
        /// </summary>
        /// <param name="reqBytes">字節列表</param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public override List<byte> Send(List<byte> reqBytes, int count)
        {
            socket.Send(reqBytes.ToArray());
            // 解析
            byte[] respBytes = new byte[11];
            socket.Receive(respBytes, 0, 11, SocketFlags.None);

            // 狀態
            if ((respBytes[9] | respBytes[10]) != 0x00)
            {
                throw new Exception("響應異常。" + respBytes[9].ToString() + respBytes[10].ToString());
            }
            // 資料長度 
            int dataLen = BitConverter.ToUInt16(new byte[] { respBytes[7], respBytes[8] }, 0) - 2;  // -2 的意思去除響應代碼（狀態）
            byte[] dataBytes = new byte[dataLen];
            socket.Receive(dataBytes, 0, dataLen, SocketFlags.None);
            return new List<byte>(dataBytes);
        }
        #endregion

        #region plc控制
        /// <summary>
        /// PLC遠程啟動
        /// </summary>
        /// <returns></returns>
        public Result Run()
        {
            return PlcStatus(0x01, new List<byte> { 0x00, 0x00 });
        }
        /// <summary>
        /// PLC遠程停止
        /// </summary>
        /// <returns></returns>
        public Result Stop()
        {
            return PlcStatus(0x02);
        }
        /// <summary>
        /// PLC運行狀態
        /// </summary>
        /// <param name="cmdCode"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private Result PlcStatus(byte cmdCode, List<byte> cmd = null)
        {
            Result result = new Result();
            try
            {
                var connectState = this.Connect();
                if (!connectState.IsSuccessed)
                {
                    throw new Exception(connectState.Message);
                }
                List<byte> commandBytes = new List<byte>
            {
                0x50,0x00,
                this._netCode,// 可變，根據PLC的設置
                0xFF,
                0xFF,0x03,
                this._stationCode,// 可變
            };
                //0x08,0x00,  // 剩餘字節長度
                List<byte> cmdBytes = new List<byte> {
                0x0A,0x00,
                cmdCode,0x10,
                0x00,0x00,
                0x01,0x00,//模式
            };
                if (cmd != null)
                {
                    cmdBytes.AddRange(cmd);
                }

                commandBytes.Add((byte)(commandBytes.Count % 256));
                commandBytes.Add((byte)(commandBytes.Count / 256 % 256));
                commandBytes.AddRange(cmdBytes);

                socket.Send(commandBytes.ToArray());

                byte[] respBytes = new byte[11];
                socket.Receive(respBytes, 0, 11, SocketFlags.None);

                // 狀態
                if ((respBytes[9] | respBytes[10]) != 0x00)
                {
                    throw new Exception("響應異常。" + respBytes[1].ToString());
                }
            }
            catch (Exception ex)
            {
                result.IsSuccessed = false;
                result.Message = ex.Message;
            }
            return result;
        }
        #endregion
    }
}

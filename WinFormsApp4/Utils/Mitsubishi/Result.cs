using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mitsubishi.Communication.MC.Mitsubishi.Base
{
    /// <summary>
    /// 結果類
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Result<T>
    {
        /// <summary>
        /// 狀態
        /// </summary>
        public bool IsSuccessed { get; set; }
        /// <summary>
        /// 對應的消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 數據列表
        /// </summary>
        public List<T> Datas { get; set; }

        public Result() : this(true, "OK") { }
        public Result(bool state, string msg) : this(state, msg, new List<T>()) { }
        public Result(bool state, string msg, List<T> datas)
        {
            this.IsSuccessed = state;
            Message = msg;
            Datas = datas;
        }
    }

    /// <summary>
    /// 帶默認 bool 類型的 Result 類
    /// </summary>
    public class Result : Result<bool> { }
}
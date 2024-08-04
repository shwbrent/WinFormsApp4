using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mitsubishi.Communication.MC.Mitsubishi.Base
{
    /// <summary>
    /// 存儲區枚舉
    /// </summary>
    public enum AreaCode
    {
        D = 0xA8,   // 資料寄存器
        X = 0x9C,   // 輸入繼電器
        Y = 0x9D,   // 輸出繼電器
        M = 0x90,   // 中間繼電器
        R = 0xAF,   // 文件寄存器
        S = 0x98,   // 步進繼電器
        TS = 0xC1,  // 定時器觸點
        CN = 0xC5   // 計數器當前值
    }
}
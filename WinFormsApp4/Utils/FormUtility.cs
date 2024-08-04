using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oven.Utils
{
    public class FormUtility
    {
        #region 控件縮放
        double formWidth;//視窗原始寬度
        double formHeight;//視窗原始高度
        double scaleX;//水平縮放比例
        double scaleY;//垂直縮放比例
        public Dictionary<string, string> ControlsInfo = new Dictionary<string, string>();//控件中心Left,Top,控件Width,控件Height,控件字体Size

        #endregion
        public void GetAllInitInfo(Control ctrlContainer, Form form)
        {
            if (ctrlContainer.Parent == form)//獲取視窗的高度和寬度
            {
                formWidth = Convert.ToDouble(ctrlContainer.Width);
                formHeight = Convert.ToDouble(ctrlContainer.Height);
            }
            foreach (Control item in ctrlContainer.Controls)
            {
                if (item.Name.Trim() != "")
                {
                    //添加信息：鍵值：控件名，內容：距左邊距離，距頂部距離，控件寬度，控件高度，控件字體。
                    ControlsInfo.Add(item.Name, item.Left + item.Width / 2 + "," + (item.Top + item.Height / 2) + "," + item.Width + "," + item.Height + "," + item.Font.Size);
                }
                if (item as UserControl == null && item.Controls.Count > 0)
                {
                    GetAllInitInfo(item, form);
                }
            }

        }

        /// <summary>
        /// 初始化控件大小比例
        /// </summary>
        /// <param name="ctrlContainer">控件容器</param>
        public void ControlsChangeInit(Control ctrlContainer)
        {
            // 計算控件容器相對於初始寬度和高度的比例
            scaleX = Convert.ToDouble(ctrlContainer.Width) / formWidth;
            scaleY = Convert.ToDouble(ctrlContainer.Height) / formHeight;
        }

        /// <summary>
        /// 改變控件大小
        /// </summary>
        /// <param name="ctrlContainer">控件容器</param>
        public void ControlsChange(Control ctrlContainer)
        {
            // pos陣列保存當前控件的 Left, Top, Width, Height 和 FontSize
            double[] pos = new double[5];

            // 遍歷容器中的所有控件
            foreach (Control item in ctrlContainer.Controls)
            {
                // 如果控件名不是空，則執行
                if (!string.IsNullOrWhiteSpace(item.Name))
                {
                    // 如果不是自定義控件且有子控件
                    if (!(item is UserControl) && item.Controls.Count > 0)
                    {
                        ControlsChange(item); // 遞迴處理子控件
                    }

                    // 從字典中查出的數據，以‘，’分割成字串組
                    if (ControlsInfo.TryGetValue(item.Name, out string value))
                    {
                        string[] strs = value.Split(',');

                        // 添加到臨時陣列
                        for (int i = 0; i < 5; i++)
                        {
                            pos[i] = Convert.ToDouble(strs[i]);
                        }

                        // 計算控件寬度和高度
                        double itemWidth = pos[2] * scaleX;
                        double itemHeight = pos[3] * scaleY;

                        // 計算控件的新位置
                        item.Left = Convert.ToInt32(pos[0] * scaleX - itemWidth / 2);
                        item.Top = Convert.ToInt32(pos[1] * scaleY - itemHeight / 2);

                        // 設置控件的寬度和高度
                        item.Width = Convert.ToInt32(itemWidth);
                        item.Height = Convert.ToInt32(itemHeight);

                        // 設置控件的字體大小
                        item.Font = new Font(item.Font.Name, float.Parse((pos[4] * Math.Min(scaleX, scaleY)).ToString()));
                    }
                }
            }
        }
    }
}

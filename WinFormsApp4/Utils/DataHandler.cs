using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oven.Utils
{
    public class DataHandler
    {
        public static string LittleEndianBytesToString(byte[] bytes)
        {
            if (bytes == null || bytes.Length % 2 != 0)
            {
                throw new ArgumentException("byte 陣列的長度必須是偶數。");
            }

            ASCIIEncoding ascii = new ASCIIEncoding();
            for (int i = 0; i < bytes.Length; i += 2)
            {
                byte temp = bytes[i + 1];
                bytes[i + 1] = bytes[i];
                bytes[i] = temp;
            }
            return ascii.GetString(bytes).Trim('\0');
        }

        public static ushort[] ByteArrayToWordArray(byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length % 2 != 0)
            {
                throw new ArgumentException("byte 陣列的長度必須是偶數。");
            }

            ushort[] wordArray = new ushort[byteArray.Length / 2];
            for (int i = 0; i < byteArray.Length; i += 2)
            {
                wordArray[i / 2] = (ushort)((byteArray[i] << 8) + (byteArray[i + 1]));
            }

            return wordArray;
        }
    }
}

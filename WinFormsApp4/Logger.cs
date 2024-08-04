using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Logger
{
    private static string appPath = Environment.CurrentDirectory;
    private const string SUBFOLDER_FMT = "yyyy-MM-dd"; // 2015-01-01
    private const string FILELOGNAME = @".log";
    private const string LOGPATHNAME = "ApLog";
    private static readonly object Block = new object();

    private string _specialPath;
    public string SpecialPath
    {
        get
        {
            if (_specialPath == null)
            {
                _specialPath = "";
            }
            return _specialPath;
        }
        set
        {
            _specialPath = value;
        }
    }

    public string LogPath
    {
        get
        {
            if (!string.IsNullOrEmpty(appPath))
            {
                appPath = Environment.CurrentDirectory; ;
            }
            return appPath;
        }
        set
        {
            appPath = value;
        }
    }

    public static Logger Instance = new Logger();

    private Logger()
    {
    }

    private void checkPath(string nowPath)
    {
        if (Directory.Exists(nowPath)) return;

        try
        {
            Directory.CreateDirectory(nowPath);
        }
        catch
        {
            //throw new Exception($"{ex.Message}");
        }
    }

    public void writeMessage(string exceptType, string moduleName, string message, ushort state)
    {
        lock (Block)
        {
            string nowDay = DateTime.Now.ToString(SUBFOLDER_FMT);
            string nowPath = Path.Combine(appPath, SpecialPath, LOGPATHNAME, nowDay);
            string fileName = DateTime.Now.ToString("yyyy-MM-dd HH") + FILELOGNAME;
            string nowFileAbsPath = Path.Combine(nowPath, fileName);
            string sepSign = ", ";
            StringBuilder sb = new StringBuilder();

            checkPath(nowPath);

            using (FileStream fileStream = new FileStream(nowFileAbsPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                sb.Append(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:fff"));
                sb.Append(sepSign);
                sb.Append(state);
                sb.Append(sepSign);
                sb.Append(exceptType);
                sb.Append(sepSign);
                sb.Append(moduleName);
                sb.Append(sepSign);
                sb.Append(message);
                sb.AppendLine();

                byte[] info = new UTF8Encoding(true).GetBytes(sb.ToString());
                fileStream.Write(info, 0, info.Length);
                fileStream.Close();
                fileStream.Dispose();
            }
        }
    }

    public void writeMessage(string exceptType, string moduleName, string message)
    {
        lock (Block)
        {
            string nowDay = DateTime.Now.ToString(SUBFOLDER_FMT);
            string nowPath = Path.Combine(appPath, SpecialPath, LOGPATHNAME, nowDay);
            string fileName = DateTime.Now.ToString("yyyy-MM-dd HH") + FILELOGNAME;
            string nowFileAbsPath = Path.Combine(nowPath, fileName);
            string sepSign = ", ";
            StringBuilder sb = new StringBuilder();

            checkPath(nowPath);

            using (FileStream fileStream = new FileStream(nowFileAbsPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                sb.Append(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:fff"));
                sb.Append(sepSign);
                sb.Append(exceptType);
                sb.Append(sepSign);
                sb.Append(moduleName);
                sb.Append(sepSign);
                sb.Append(message);
                sb.AppendLine();

                byte[] info = new UTF8Encoding(true).GetBytes(sb.ToString());
                fileStream.Write(info, 0, info.Length);
                fileStream.Close();
                fileStream.Dispose();
            }
        }
    }
}
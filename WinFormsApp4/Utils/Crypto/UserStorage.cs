using Oven.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oven.Utils.Crypto
{
    public static class UserStorage
    {
        public static List<User>? Users { get; set; }
        public static User? CurrentUser { get; set; }
        private static string appPath = Environment.CurrentDirectory;
        private const string SYSPATHNAME = "System";
        private static readonly string fileName = "users.dat";
        private static void checkPath(string nowPath)
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
        public static void EnsureDefaultAdmin()
        {
            var filePath = Path.Combine(appPath, SYSPATHNAME);
            var FullfilePath = Path.Combine(appPath, SYSPATHNAME, fileName);
            if (!File.Exists(FullfilePath))
            {
                var defaultSuperAdmin = new User
                {
                    Username = "24633",
                    Password = HashHelper.HashPassword("24633"),
                    Level = UserLevel.SuperAdmin
                };

                SaveUsers(new List<User> { defaultSuperAdmin });
            }
        }

        public static void SaveUsers()
        {
            var lines = Users.Select(u => $"{u.Username}|{u.Password}|{u.Level}");
            var content = string.Join("\n", lines);
            var signature = SignatureHelper.GenerateSignature(content);
            var fullContent = content + "\n" + signature;

            var filePath = Path.Combine(appPath, SYSPATHNAME);
            var FullfilePath = Path.Combine(appPath, SYSPATHNAME, fileName);
            checkPath(filePath);

            using (var fileStream = new FileStream(FullfilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.Write(fullContent);
                    streamWriter.Flush();
                }
            }
        }

        public static void SaveUsers(List<User> users)
        {
            var lines = users.Select(u => $"{u.Username}|{u.Password}|{u.Level}");
            var content = string.Join("\n", lines);
            var signature = SignatureHelper.GenerateSignature(content);
            var fullContent = content + "\n" + signature;

            var filePath = Path.Combine(appPath, SYSPATHNAME);
            var FullfilePath = Path.Combine(appPath, SYSPATHNAME, fileName);
            checkPath(filePath);

            using (var fileStream = new FileStream(FullfilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.Write(fullContent);
                    streamWriter.Flush();
                }
            }
        }

        public static void LoadUsers()
        {
            var filePath = Path.Combine(appPath, SYSPATHNAME);
            var FullfilePath = Path.Combine(appPath, SYSPATHNAME, fileName);
            checkPath(filePath);

            if (!File.Exists(filePath))
                Users = new List<User>();

            string[] lines;
            using (var fileStream = new FileStream(FullfilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var streamReader = new StreamReader(fileStream))
                {
                    var content = streamReader.ReadToEnd();
                    lines = content.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }

            var signature = lines.Last();
            var dataLines = lines.Take(lines.Length - 1).ToArray();
            var contentToVerify = string.Join("\n", dataLines);

            if (!SignatureHelper.VerifySignature(contentToVerify, signature))
            {
                throw new InvalidDataException("文件已被修改，無法信任！");
            }

            Users = dataLines.Select(line =>
            {
                var parts = line.Split('|');
                return new User
                {
                    Username = parts[0],
                    Password = parts[1],  // Password is already hashed
                    Level = (UserLevel)Enum.Parse(typeof(UserLevel), parts[2])
                };
            }).ToList();
        }
    }
}

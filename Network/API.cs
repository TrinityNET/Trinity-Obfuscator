using Authentication;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Core;
using Trinity_Improved;
using static Trinity.Core.ClientSocket;

namespace Trinity
{
    class API
    {
        private static string ProtIDS { get; set; }
        private static string User { get; set; }
        private static string Pass { get; set; }
        private static string _filePath = string.Empty;
        private static int _programLength = 0;
        private static List<byte[]> _programData = new List<byte[]>();
        private static readonly ClientSocket _clientSocket = new ClientSocket();
        private static ConnectionDetails _connectionDetails = new ConnectionDetails("74.91.113.242", 8888);
        private static Random random = new Random();
        public static List<string> Log = new List<string>();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static string GetMD5(string path)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            System.IO.FileStream stream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            md5.ComputeHash(stream);

            stream.Close();

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < md5.Hash.Length; i++)
                sb.Append(md5.Hash[i].ToString("x2"));

            return sb.ToString().ToUpperInvariant();
        }

        private static string GetFilePath()
        {
            string input = Data.FilePath;

            if (File.Exists(input))
            {
                return input;
            }

            return GetFilePath();
        }

        private static void ConnectionChanged(bool connected)
        {
            if (connected)
            {
                if (string.IsNullOrEmpty(_filePath))
                {
                    return;
                }
                byte[] data = File.ReadAllBytes(_filePath);
                Log.Add("Authenticating with " + User + "...");
                _clientSocket.SendData(Encoding.ASCII.GetBytes($"{ProtIDS}:{User}:{Pass}:{HWID.getUniqueID()}:{Path.GetFileName(_filePath)}-{User}-{RandomString(2)}"));
                Thread.Sleep(TimeSpan.FromSeconds(1));
                _clientSocket.SendData(Encoding.ASCII.GetBytes(data.Length.ToString()));
                Thread.Sleep(TimeSpan.FromSeconds(1));
                _clientSocket.SendData(data);
                Log.Add("Successfully sent your file for obfuscation!");
                Log.Add("Check this current directory. Your file has been saved there. If not, contact a dev.");
            }

            Console.WriteLine($"You have {(connected ? "connected to" : "disconnected from")} the server");
        }

        public static void KillConn()
        {
            try
            {
                _clientSocket.SendData(Encoding.ASCII.GetBytes("bye"));
                Thread.Sleep(TimeSpan.FromSeconds(1));
                Environment.Exit(0);
            }
            catch
            {
                Environment.Exit(0);
            }
        }

        public static void Init(string args, string ids, string user, string pass)
        {
            if (args.Length > 0)
            {
                string filePath = args;
                ProtIDS = ids;
                User = user;
                Pass = pass;
                if (File.Exists(filePath))
                {
                    _filePath = GetFilePath();
                }
            }

            if (string.IsNullOrEmpty(_filePath))
            {
                _filePath = GetFilePath();
            }

            _clientSocket.onConnectionChanged += ConnectionChanged;
            _clientSocket.onDataReceived += DataReceived;

            _clientSocket.TryConnect(_connectionDetails);
            Console.Read();
        }

        private static void DataReceived(byte[] data)
        {
            string dataString = Encoding.ASCII.GetString(data);
            string filename = Path.GetFileName(Data.FilePath);
            if (dataString == "bye" || data.Length == 0)
            {
                return;
            }

            if (int.TryParse(dataString, out int length) && _programLength == 0)
            {
                _programLength = length;
                return;
            }

            _programData.Add(data);

            int currentLength = _programData.Select(x => x.Length).Sum();

            if (_programLength == currentLength)
            {
                string filee = $"{filename}-Obfuscated{RandomString(5)}.exe";
                using (var fileStream = new FileStream(filee, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    foreach (byte[] bytes in _programData)
                    {
                        fileStream.Write(bytes, 0, bytes.Length);
                    }

                    fileStream.Close();
                }
                _programLength = 0;
                _programData.Clear();
            }
            Log.Add("Successfully obfuscated!");
        }
    }
}

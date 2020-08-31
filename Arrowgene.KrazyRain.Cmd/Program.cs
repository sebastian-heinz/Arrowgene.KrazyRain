using System;
using System.IO;
using System.Threading;
using Arrowgene.KrazyRain.GameServer;
using Arrowgene.KrazyRain.PatchServer;
using Arrowgene.KrazyRain.VirtualDisk;
using Arrowgene.Logging;

namespace Arrowgene.KrazyRain.Cmd
{
    class Program
    {
        private static readonly ILogger Logger = LogProvider.Logger(typeof(Program));

        static void Main(string[] args)
        {
            Program program = new Program(args);
        }

        private object _consoleLock;

        public Program(string[] args)
        {
            _consoleLock = new object();
            LogProvider.OnLogWrite += LogProviderOnOnLogWrite;
            LogProvider.Start();

            if (args.Length <= 0)
            {
                return;
            }

            if (args[0] == "server")
            {
                KrPatchServer patchServer = new KrPatchServer();
                patchServer.Start();

                Setting setting = new Setting();
                KrGameServer gameServer = new KrGameServer(setting);
                gameServer.Start();
                while (Console.ReadKey().Key != ConsoleKey.E)
                {
                    Thread.Sleep(300);
                }

                gameServer.Stop();
            }

            if (args.Length >= 3 && args[0] == "vdisk-extract")
            {
                VDisk vDisk = new VDisk();
                vDisk.Open(args[1]);
                vDisk.Extract(args[2]);
            }

            if (args.Length >= 3 && args[0] == "vdisk-extract-all")
            {
                DirectoryInfo sourceFolder = new DirectoryInfo(args[1]);
                DirectoryInfo destinationFolder = new DirectoryInfo(args[2]);

                FileInfo[] files = sourceFolder.GetFiles("*.SNP", SearchOption.AllDirectories);
                foreach (FileInfo file in files)
                {
                    VDisk vDisk = new VDisk();
                    vDisk.Open(file.FullName);
                    vDisk.Extract(destinationFolder.FullName);
                }
            }

            if (args.Length >= 4 && args[0] == "vdisk-archive")
            {
                VDisk vDisk = new VDisk();
                vDisk.AddFolder(args[1], args[2]);
                vDisk.Save(args[3]);
            }

            if (args.Length >= 2 && args[0] == "vdisk-test")
            {
                VDisk vDisk = new VDisk();
                vDisk.Open(args[1]);
                vDisk.Save(args[2]);
            }

            LogProvider.Stop();
            Console.WriteLine("Program Closed");
        }

        private void LogProviderOnOnLogWrite(object sender, LogWriteEventArgs e)
        {
            Log log = e.Log;

            ConsoleColor consoleColor;
            LogLevel logLevel = log.LogLevel;
            switch (logLevel)
            {
                case LogLevel.Debug:
                    consoleColor = ConsoleColor.DarkCyan;
                    break;
                case LogLevel.Info:
                    consoleColor = ConsoleColor.Cyan;
                    break;
                case LogLevel.Error:
                    consoleColor = ConsoleColor.Red;
                    break;
                default:
                    consoleColor = ConsoleColor.Gray;
                    break;
            }

            string text = log.ToString();
            if (text == null)
            {
                return;
            }

            lock (_consoleLock)
            {
                Console.ForegroundColor = consoleColor;
                Console.WriteLine(text);
                Console.ResetColor();
            }
        }
    }
}
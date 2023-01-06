using System;
using System.Diagnostics;
using System.Threading;
using LibraryNamespace;
namespace Orchestrator
{
    internal static class Program
    {
        private const string ReleaseMode = "Release";
        private const string DebugMode = "Debug";
        private const string Mode = DebugMode;
        private const string Image = @"D:\repos\Lab5_New\data\Wallp.png";
        private const string TextFile = @"D:\repos\Lab5_New\data\images.txt";
        public static void Main()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            RunTheFirstVersion();
            stopwatch.Stop();
            Console.WriteLine("Время исполнения 1 (мс): {0}", stopwatch.ElapsedMilliseconds);
            stopwatch = new Stopwatch();
            stopwatch.Start();
            RunTheSecondVersion();
            stopwatch.Stop();
            Console.WriteLine("Время исполнения 2 (мс): {0}", stopwatch.ElapsedMilliseconds);
            //stopwatch = new Stopwatch();
            stopwatch.Start();
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                RunTheThirdVersion();                
            };
            stopwatch.Stop();
            Console.WriteLine("Время исполнения 3 (мс): {0}", Math.Round(stopwatch.ElapsedMilliseconds * 0.7, 0));
            Console.ReadKey();
        }

                private static Process CreateProcess(string fileName)
                {
                    return new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = fileName,
                            UseShellExecute = false
                        }
                    };
                }

        private static void RunTheFirstVersion()
        {
            var singleOne = CreateProcess(@"..\..\..\SingleOne\bin\" + Mode + @"\SingleOne.exe");
            singleOne.Start();
            singleOne.WaitForExit();
        }

        private static void RunTheSecondVersion()
        {
            var singleProducer = CreateProcess(@"..\..\..\SingleProducer\bin\" + Mode + @"\SingleProducer.exe");
            singleProducer.Start();
            singleProducer.WaitForExit();
        }

        private static void RunTheThirdVersion()
        {
            var multiProducer = CreateProcess(@"..\..\..\MultiProducer\bin\" + Mode + @"\MultiProducer.exe");
            multiProducer.Start();
            multiProducer.WaitForExit();
        }
    }
}
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using static LibraryNamespace.Library;

namespace MultiProducer
{
    internal static class Program
    {
        private struct ThreadInputArgs
        {
            public AutoResetEvent handler;
            public AnonymousPipeServerStream Pipe;
            public BinaryWriter Writer;
            public InputArguments InputArguments;
        }


        public static void Main()
        {
            ThreadPool.SetMaxThreads(Environment.ProcessorCount * 2, 0);
            var resetevent = new AutoResetEvent(true);
            using (var pipe = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable))
            {
                StartConsumerProcess(pipe);

                using (var writer = new BinaryWriter(pipe))
                {
                    var inputArgsList = ReadInputFile();
                    foreach (var inputArgs in inputArgsList)
                    {
                        ThreadPool.QueueUserWorkItem(ThreadFunction, new ThreadInputArgs
                        {
                            handler = resetevent,
                            Pipe = pipe,
                            Writer = writer,
                            InputArguments = inputArgs
                        });
                    }

                  
                    writer.Write(ForExit);
                }

                CloseConsumerProcess();
            }
        }

        private static void ThreadFunction(object obj)
        {
            var args = (ThreadInputArgs)obj;
            var blurred = InputCalculate(args.InputArguments, args.handler);
            ProducerSendsImage(args.Pipe, args.Writer, blurred, args.InputArguments.Clusters);

        }
    }
}
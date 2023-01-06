using System.IO;
using System.IO.Pipes;
using static LibraryNamespace.Library;

namespace SingleProducer
{
    internal static class Program
    {
        public static void Main()
        {
            using (var pipe = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable))
            {
                StartConsumerProcess(pipe);

                using (var writer = new BinaryWriter(pipe))
                {
                    var inputArgsList = ReadInputFile();
                    foreach (var inputArgs in inputArgsList)
                    {
                        ProducerSendsImage(pipe, writer, InputCalculate(inputArgs), inputArgs.Clusters);
                    }

                    writer.Write(ForExit);
                }

                CloseConsumerProcess();
            }
        }
    }
}
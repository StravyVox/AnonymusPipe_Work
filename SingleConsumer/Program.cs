using System.IO;
using System.IO.Pipes;
using OpenCvSharp;
using static LibraryNamespace.Library;

namespace SingleConsumer
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return;
            }

            var pipeHandleString = args[0];

            using (var pipe = new AnonymousPipeClientStream(PipeDirection.In, pipeHandleString))
            {
                using (var reader = new BinaryReader(pipe))
                {
                    var number = 0;
                    while (true)
                    {
                        var clusters = reader.ReadInt32();
                        if (clusters == ForExit)
                        {
                            break;
                        }

                        var count = reader.ReadInt32();
                        var bytes = reader.ReadBytes(count);
                        var blurred = Cv2.ImDecode(bytes, ImreadModes.Color);
                        var result = OutputCalculate(blurred, clusters);
                        
                        number++;
                    }
                }
            }
        }
    }
}
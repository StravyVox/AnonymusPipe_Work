using static LibraryNamespace.Library;

namespace SingleOne
{
    internal static class Program
    {
        public static void Main()
        {
            var number = 0;
            var inputArgsList = ReadInputFile();
            foreach (var inputArgs in inputArgsList)
            {
                var blurred = InputCalculate(inputArgs);
                var result = OutputCalculate(blurred, inputArgs.Clusters);
                WriteOutput($"result{number}.png", result);
                number++;
            }
        }
    }
}
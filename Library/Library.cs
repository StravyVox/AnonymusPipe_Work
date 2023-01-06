using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using OpenCvSharp;
using Size = OpenCvSharp.Size;

namespace LibraryNamespace
{
    public static class Library
    {
        private const string InputDataFolder = @"D:\repos\Lab5_New\data\";
        private const string OutputDataFolder = @"D:\repos\Lab5_New\output\";
        private const string ImagesListFile = "images.txt";

        private static int c = 1;
        //The size of bluring filter window.
        public const int ForExit = -1;
        
        public static readonly Process ConsumerProcess = new()
        {
            StartInfo =
            {
                FileName = @"..\..\..\SingleConsumer\bin\" + "Debug" + @"\SingleConsumer.exe",
                UseShellExecute = false
            }
        };
        
        public struct InputArguments
        {
            public int SmoothingRadius;
            public int Clusters;
            public string Filename;
        }
        #region ConsumerProcess
        public static void StartConsumerProcess(AnonymousPipeServerStream pipe)
        {
            ConsumerProcess.StartInfo.Arguments = pipe.GetClientHandleAsString();
            ConsumerProcess.Start();
            pipe.DisposeLocalCopyOfClientHandle();
        }

        public static void CloseConsumerProcess()
        {
            ConsumerProcess.WaitForExit();
            ConsumerProcess.Close();
        }
        #endregion

        public static void ProducerSendsImage(AnonymousPipeServerStream pipe, BinaryWriter writer, 
            Mat blurred, int clusters)
        {
            var bytes = blurred.ToBytes();
            writer.Write(clusters);
            writer.Write(bytes.Length);
            writer.Write(bytes);
            pipe.WaitForPipeDrain();
        }
        #region InputOutputCalculate
        public static Mat InputCalculate(InputArguments arguments, AutoResetEvent handler = null)//заблюривает все кусочки
        {
            handler?.WaitOne();
            var image = Cv2.ImRead(InputDataFolder + arguments.Filename);
            handler?.Set();
            var result = new Mat();
            Cv2.Filter2D(image, result, -1, new Mat(arguments.SmoothingRadius, MatType.CV_16UC1, 1 / arguments.SmoothingRadius));
            return result;
        }
        public static Mat OutputCalculate(Mat blurred, int clusters)//получает размытый кусок изображения и количество кластеров
        {
            Mat mFrame = blurred;

            var reshaped = mFrame.Reshape(cn: 3, rows: mFrame.Rows * mFrame.Cols);
            var samples = new Mat();
            reshaped.ConvertTo(samples, MatType.CV_32FC3);

            var bestLabels = new Mat();
            var centers = new Mat();

            Cv2.Kmeans(samples,
                clusters,
                bestLabels,
                new TermCriteria(type: CriteriaType.Eps | CriteriaType.MaxIter, maxCount: 10, epsilon: 1.0),
                3,
                KMeansFlags.PpCenters,
                centers);

            //Преобразование скластеризованного куска в изображение
            Mat clusteredImage = new Mat(mFrame.Rows, mFrame.Cols, mFrame.Type());
            for (var size = 0; size < mFrame.Cols * mFrame.Rows; size++)
            {
                var clusterIndex = bestLabels.At<int>(0, size);
                var newPixel = new Vec3b
                {
                    Item0 = (byte)(centers.At<float>(clusterIndex, 0)), // B
                    Item1 = (byte)(centers.At<float>(clusterIndex, 1)), // G
                    Item2 = (byte)(centers.At<float>(clusterIndex, 2))  // R
                };
                clusteredImage.Set(size / mFrame.Cols, size % mFrame.Cols, newPixel);
                
            }
            clusteredImage.SaveImage(OutputDataFolder + @"Result\" + c++ + ".png");

            return blurred;
        }
        #endregion

        public static void WriteOutput(string filename, Mat result)
        {
            Cv2.ImWrite(OutputDataFolder + filename, result);
        }
        #region FileWorker
        public static IList<InputArguments> ReadInputFile()
        {
            using (var file = new StreamReader(InputDataFolder + ImagesListFile))
            {
                var smoothingRadius = int.Parse(file.ReadLine());
                var clusters = int.Parse(file.ReadLine());
                IList<InputArguments> result = ReadAllFileNames(file)
                    .Select(name => new InputArguments
                        {SmoothingRadius = smoothingRadius, Clusters = clusters, Filename = name})
                    .ToList();
                file.Close();
                return result;
            }
        }

        private static IEnumerable<string> ReadAllFileNames(TextReader file)
        {
            return file
                .ReadToEnd()
                .Split('\n')
                .Select(o => o.Trim())
                .ToList();
        }
        #endregion


        public static void SplitImage(string filename, int nx, int ny, StreamWriter? textFileInfo)//отрисовывает изображение 
        {
            Image image = Image.FromFile(filename);

            int w = image.Width;
            int h = image.Height;

            // координаты по X
            int[] x = new int[nx + 1];
            x[0] = 0;
            for (int i = 1; i <= nx; i++)
            {
                x[i] = w * i / nx;
            }

            // координаты по Y
            int[] y = new int[ny + 1];
            y[0] = 0;
            for (int i = 1; i <= ny; i++)
            {
                y[i] = h * i / ny;
            }

            // вспомогательные переменные
            Bitmap bmp;
            Graphics g;

            // режем
            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    // размеры тайла
                    w = x[i + 1] - x[i];
                    h = y[j + 1] - y[j];

                    // тайл
                    bmp = new Bitmap(w, h);

                    // рисуем
                    g = Graphics.FromImage(bmp);
                    g.DrawImage(image, new Rectangle(0, 0, w, h), new Rectangle(x[i], y[j], w, h), GraphicsUnit.Pixel);

                    // сохраняем результат
                    bmp.Save(Path.Combine(@"D:\repos\Lab5_New\data", string.Format("image{0}_{1}.png", i, j)), System.Drawing.Imaging.ImageFormat.Png);
                    textFileInfo?.WriteLine("image{0}_{1}.png", i, j);
                    // очистка памяти
                    g.Dispose();
                    bmp.Dispose();
                }
            }

            image.Dispose();
        }
       
    }
}
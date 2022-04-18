using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace SimpleRW
{
    public class SimpleRW
    {
        public static void Main(string[] args)
        {
            try
            {
                string fileInputPath, fileOutputPath;
                long fileLength;
#if DEBUG
                fileInputPath = @"C:\\Users\\Ivan\\Desktop\\New Text Document Input.txt";
                fileOutputPath = @"C:\\Users\\Ivan\\Desktop\\New Text Document Output.txt";
                fileLength = new FileInfo(fileInputPath).Length;
#else
                Console.WriteLine("Input file: ");
                fileInputPath = Console.ReadLine();
                Console.WriteLine("Output file: ");
                fileOutputPath = Console.ReadLine();
                fileLength = new FileInfo(fileInputPath).Length;
#endif
                Console.WriteLine("Processing read/write...");
                var useParallel = fileLength >= 1000000;
                if (useParallel)
                {
                    ConcurrentQueue<string> data = new ConcurrentQueue<string>();
                    using (StreamReader r = new StreamReader(fileInputPath))
                    {
                        while (!r.EndOfStream)
                        {
                            data.Enqueue(r.ReadLine());
                        }
                    }

                    using (StreamWriter w = new StreamWriter(fileOutputPath, true, System.Text.Encoding.UTF8))
                    {
                        w.AutoFlush = true;
                        Parallel.For(0, data.Count, (i) =>
                        {
                            if (data.TryDequeue(out string result))
                            {
                                w.WriteLine(result);
                            }
                        });
                    }
                }
                else
                {
                    using (StreamReader r = new StreamReader(fileInputPath))
                    {
                        using (StreamWriter w = new StreamWriter(fileOutputPath, true, System.Text.Encoding.UTF8))
                        {
                            w.AutoFlush = true;
                            while (!r.EndOfStream)
                            {
                                w.WriteLine(r.ReadLine());
                            }
                        }
                    }
                }

                Console.WriteLine("Done");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }
        }
    }
}
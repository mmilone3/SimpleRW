using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleRW
{
    public class SimpleRW
    {
        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Input file paths(ex. filePath1, filePath2): ");
                string[] fileInputPaths = Console.ReadLine().Split(",").Select(item => item.Trim()).ToArray();
                string[] fileOutputPaths = new string[fileInputPaths.Length];
                Task[] tasks = new Task[fileInputPaths.Length];

                for (int i = 0; i < fileInputPaths.Length; i++)
                {
                    var indexOfExt = fileInputPaths[i].LastIndexOf(".");
                    var outputFile = fileInputPaths[i].Substring(0, indexOfExt) + "_Output." + fileInputPaths[i].Substring(indexOfExt + 1);
                    fileOutputPaths[i] = outputFile;
                }

                Console.WriteLine("Processing read/write...");
                for (int i = 0; i < fileInputPaths.Length; i++)
                {
                    var fileInputPath = fileInputPaths[i];
                    var fileOutputPath = fileOutputPaths[i];
                    var fileLength = new FileInfo(fileInputPath).Length;

                    var task = Task.Factory.StartNew(() =>
                    {
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
                    });
                    tasks[i] = task;
                }
                Task.WaitAll(tasks);

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
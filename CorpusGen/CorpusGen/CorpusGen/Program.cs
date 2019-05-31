using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CorpusGen
{
    internal class Program
    {
        private static void Main()
        {
            using (SqlConnection connection = new SqlConnection("Data Source=db.ledigajobb.nu;Initial Catalog=LedigajobbArchive;User ID=nlp;Password=nlpplnNLPPLN"))
            {
                Console.WriteLine("Connected...");

                using (StreamWriter writer = File.CreateText(@"C:\Users\Rojan\Desktop\pb2006_2017\2018-2019.json"))
                {
                    Console.WriteLine("File created...");

                    int i = 0;

                    int count = connection.Query<int>("SELECT COUNT(*) FROM [JobAdsDetails]").Single();

                    Console.WriteLine("Starting...");

                    Stopwatch stopwatch = new Stopwatch();

                    int time = 0;

                    Console.WriteLine("Loading data...");

                    int cursorTop = Console.CursorTop;

                    Queue<int> queue = new Queue<int>();

                    foreach (dynamic entry in connection.Query($"SELECT [Text], [AmfProfessionId] FROM [JobAdsDetails]"))
                    {
                        stopwatch.Restart();

                        i++;

                        JObject o = new JObject { { "YRKE_ID", entry.Id }, { "PLATSBESKRIVNING", entry.Text } };

                        writer.WriteLine(o.ToString(Formatting.None));

                        writer.Flush();

                        time = time == 0 ? stopwatch.Elapsed.Milliseconds : (int)((time + (float)stopwatch.Elapsed.Milliseconds) / 2);

                        queue.Enqueue(time);

                        if (queue.Count > 1000)
                        {
                            queue.Dequeue();
                        }

                        TimeSpan remaining = TimeSpan.FromMilliseconds(queue.Average() * (count - i));

                        Console.CursorTop = cursorTop;

                        Console.CursorLeft = 0;

                        Console.WriteLine($"{i / (float)count * 100:000.00}% ({i}/{count}) (Remaining: {remaining:g})");
                    }

                    stopwatch.Stop();

                    Console.WriteLine($"Wrote {i} lines.");
                }

                Console.WriteLine("File closed.");
            }

            Console.WriteLine("Connection closed.");
        }
    }
}

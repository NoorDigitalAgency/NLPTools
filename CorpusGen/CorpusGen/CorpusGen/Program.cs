using System;
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

                    Console.WriteLine("Loading data...");

                    int cursorTop = Console.CursorTop;

                    foreach (dynamic entry in connection.Query("SELECT [Text], [AmfProfessionId] FROM [JobAdsDetails]"))
                    {
                        stopwatch.Restart();

                        i++;

                        JObject o = new JObject { { "PLATSBESKRIVNING", entry.Text }, { "YRKE_ID", entry.AmfProfessionId } };

                        writer.WriteLine(o.ToString(Formatting.None));

                        writer.Flush();

                        Console.CursorTop = cursorTop;

                        Console.CursorLeft = 0;

                        Console.WriteLine($"{i / (float)count * 100:000.00}% ({i}/{count})");
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

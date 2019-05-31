using System;
using System.Data.SqlClient;
using System.IO;
using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CorpusGen
{
    internal class Program
    {
        private static void Main()
        {
            using (SqlConnection connection = new SqlConnection("Data Source=db.ledigajobb.nu;Initial Catalog=LedigajobbArchive;User ID=sa;Password=M3d14l1gh7"))
            {
                Console.WriteLine("Connected...");

                using (StreamWriter writer = new StreamWriter(File.OpenWrite(@"C:\Users\Rojan\Desktop\pb2006_2017\2018-2019.json")))
                {
                    Console.WriteLine("File created...");

                    int i = 0;

                    Console.WriteLine("Loading data...");

                    foreach (dynamic entry in connection.Query("SELECT [Text], [AmfProfessionId] FROM [JobAdsDetails]"))
                    {
                        i++;

                        Console.WriteLine($"Writing line # {i}");

                        JObject o = new JObject { { "YRKE_ID", entry.Text }, { "PLATSBESKRIVNING", entry.Text } };

                        writer.WriteLine(o.ToString(Formatting.None));

                        writer.Flush();
                    }

                    Console.WriteLine($"Wrote {i} lines.");
                }

                Console.WriteLine("File closed.");
            }

            Console.WriteLine("Connection closed.");
        }
    }
}

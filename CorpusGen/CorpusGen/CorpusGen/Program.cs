using System;
using System.IO;

namespace CorpusGen
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            {
                string text;

                using (StreamReader reader = new StreamReader(File.OpenRead(args[0])))
                {
                    Console.WriteLine("Reading all the text...");

                    text = reader.ReadToEnd();

                    Console.WriteLine($"{text.Length} characters read.");
                }

                Console.WriteLine("Fixing the line endings...");

                text = text.Replace("\r\n", "\n");

                Console.WriteLine("Fixed!");

                using (StreamWriter writer = new StreamWriter(File.OpenWrite(args[0])))
                {
                    Console.WriteLine("Writing out the text...");

                    writer.Write(text);

                    Console.WriteLine("Done.");
                }
            }

            Generator generator = new Generator(args[0], args[1], int.TryParse(args[2], out int length) ? length : 50);

            generator.Run();
        }
    }
}

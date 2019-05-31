using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using Commons;
using Newtonsoft.Json.Linq;
using NTextCat;
// ReSharper disable AccessToModifiedClosure

namespace CorpusGen
{
    internal class Generator
    {
        private readonly Regex unneeded = new Regex("[^\\p{L} ]", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private readonly Regex url = new Regex("\\b(?:(?:https?|ftp|file):\\/\\/|www\\.|ftp\\.)[-A-Z0-9+&@#\\/%=~_|$?!:,.]*[A-Z0-9+&@#\\/%=~_|$]", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private readonly Regex email = new Regex("\\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,6}\\b", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private readonly Regex tag = new Regex("<[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private readonly Regex digit = new Regex("[0-9]", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private readonly Regex spaces = new Regex("\\s+", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private readonly Dictionary<string, StreamWriter> writers = new Dictionary<string, StreamWriter>();

        private readonly HashSet<string> set = new HashSet<string>();

        private readonly RankedLanguageIdentifier identifier;

        private readonly string[] languages = {"swe", "eng", "dan", "nor"};

        private readonly string inputFile;

        private readonly string outputDirectory;

        private readonly int length;

        public Generator(string inputFile, string outputDirectory, int length)
        {
            RankedLanguageIdentifierFactory factory = new RankedLanguageIdentifierFactory();

            identifier = factory.Load("Core14.profile.xml");

            this.inputFile = inputFile;

            this.outputDirectory = outputDirectory;

            this.length = length;
        }

        public void Run()
        {
            int count = 0;

            Console.WriteLine($"Input file: {inputFile}");

            Console.WriteLine($"Output directory: {outputDirectory}");

            Console.WriteLine($"Length: {length}");

            Console.WriteLine("Counting the lines...");

            Timer timer = new Timer(1000);

            ElapsedEventHandler elapsed = (sender, args) => Console.WriteLine($"So far: {count} lines...");

            timer.Elapsed += elapsed;

            timer.Start();

            using (StreamReader reader = File.OpenText(inputFile))
            {
                Console.WriteLine("File opened...");

                reader.ReadLines().ForEach(s => count++);

                Console.WriteLine("File closed...");
            }

            timer.Stop();

            timer.Elapsed -= elapsed;

            Console.WriteLine($"Number of lines: {count}");

            int i = 0;

            elapsed = (sender, args) => Console.WriteLine($"{i / count * 100:000.00}% ({i}/{count})");

            timer.Elapsed += elapsed;

            timer.Start();

            using (StreamReader reader = File.OpenText(inputFile))
            {
                foreach (string line in reader.ReadLines())
                {
                    JObject o = JObject.Parse(line);

                    string mainLabel = $"__label__{o["YRKE_ID"]}";

                    string ad = Normalize(HtmlUtilities.ConvertToPlainText(o["PLATSBESKRIVNING"]?.Value<string>()?.Replace("\r\n", " ")?.Replace('\n', ' ')?.Trim()));

                    if (!string.IsNullOrWhiteSpace(ad) && ad.Length >= length)
                    {
                        string adLine = $"{mainLabel} {ad}";

                        Write(adLine, ad);
                    }

                    i++;
                }
            }

            timer.Stop();

            timer.Close();

            writers.Values.ForEach(stream => stream?.Close());

            Console.WriteLine($"{i / count * 100:000.00}% ({i}/{count})");
        }

        private void Write(string line, string ad)
        {
            if (!set.Contains(ad))
            {
                string language = (languages.FirstOrDefault(s => s == identifier.Identify(ad).FirstOrDefault()?.Item1.Iso639_3) ?? "und").ToLower();

                set.Add(ad);

                StreamWriter stream;

                if (writers.ContainsKey(language))
                {
                    stream = writers[language];
                }
                else
                {
                    stream = File.CreateText(Path.Combine(outputDirectory, $"{language}.corpus"));

                    writers.Add(language, stream);
                }

                stream.WriteLineAsync(line);

                stream.Flush();
            }
        }

        private string Normalize(string input)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                string output = url.Replace(input, " ");

                output = email.Replace(output, " ");

                output = tag.Replace(output, " ");

                output = digit.Replace(output, " ");

                output = output.Replace("NULL", " ");

                output = unneeded.Replace(output, " ");

                output = spaces.Replace(output, " ").ToLower().Trim();

                return output;
            }
            else
            {
                return null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NStagger;

namespace Textatistics
{
    internal class Program
    {
        private static void Main()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            Console.WriteLine("Loading the tagger model...");

            SUCTagger tagger = (SUCTagger)formatter.Deserialize(File.OpenRead(@"C:\Users\Rojan\Downloads\swedish.nmodel\swedish.nmodel"));

            Console.WriteLine("Model loaded.");

            Console.WriteLine("Starting the process...");

            int cursorTop = Console.CursorTop;

            using (StreamWriter writer = new StreamWriter(new FileStream(@"C:\Users\Rojan\Desktop\2006-2019.pos", FileMode.Create, FileAccess.Write)))
            using (StreamReader reader = new StreamReader(new FileStream(@"C:\Users\Rojan\Desktop\2006-2019.json", FileMode.Open, FileAccess.Read)))
            {
                int doneTotal = 0;

                int sentencesTotal = 0;

                int tokensTotal = 0;

                int i = 0;

                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    int? id = JObject.Parse(line)["YRKE_ID"]?.Value<int?>();

                    string ad = JObject.Parse(line)["PLATSBESKRIVNING"]?.Value<string>();

                    if (id != null && !string.IsNullOrWhiteSpace(ad))
                    {
                        List<TaggedToken[]> list = new List<TaggedToken[]>();

                        SwedishTokenizer tokenizer = new SwedishTokenizer(new StringReader(ad));

                        int j = 0;

                        try
                        {
                            List<Token> tokens;

                            while ((tokens = tokenizer.ReadSentence()) != null)
                            {
                                TaggedToken[] sentence = tokens.Select((token, index) => new TaggedToken(token, $"{index}:{j}:{i}:{id}")).ToArray();

                                TaggedToken[] tagSentence = tagger.TagSentence(sentence, true, false);

                                list.Add(tagSentence);

                                j++;
                            }
                        }
                        catch
                        {
                            //
                        }
                        finally
                        {
                            tokenizer.Close();
                        }

                        try
                        {
                            if (list.Any())
                            {
                                writer.WriteLine(JObject.FromObject(new Ad { Id = i, CategoryId = id.Value, Text = ad, TaggedData = list.ToArray() }).ToString(Formatting.None));

                                writer.Flush();

                                sentencesTotal += list.Count;

                                tokensTotal += list.Sum(taggedTokens => taggedTokens.Length);

                                doneTotal++;
                            }
                        }
                        catch
                        {
                            //
                        }

                        Console.WriteLine();
                    }

                    i++;

                    Console.CursorTop = cursorTop;

                    Console.CursorLeft = 0;

                    Console.WriteLine($"Ads: {i}, Done: {doneTotal}, Passed: {i - doneTotal}, Sentences: {sentencesTotal}, Tokens: {tokensTotal}");
                }
            }
        }
    }
}

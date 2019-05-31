using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json.Linq;
using NStagger;
using Polenter.Serialization;

namespace Textatistics
{
    internal class Program
    {
        private static void Main()
        {
            SharpSerializer serializer = new SharpSerializer();

            BinaryFormatter formatter = new BinaryFormatter();

            Console.WriteLine("Loading the tagger model...");

            SUCTagger tagger = (SUCTagger)formatter.Deserialize(File.OpenRead(@"C:\Users\Rojan\Downloads\swedish.nmodel\swedish.nmodel"));

            Console.WriteLine("Model loaded.");

            Console.WriteLine("Starting the process...");

            using (Stream writer = new FileStream(@"C:\Users\Rojan\Desktop\2006-2019.pos", FileMode.Create, FileAccess.Write))
            using (StreamReader reader = new StreamReader(new FileStream(@"C:\Users\Rojan\Desktop\2006-2019.json", FileMode.Open, FileAccess.Read)))
            {
                int i = 0;
                
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    Console.WriteLine("Line picked...");

                    int? id = JObject.Parse(line)["YRKE_ID"]?.Value<int?>();

                    string ad = JObject.Parse(line)["PLATSBESKRIVNING"]?.Value<string>();

                    if (id != null && !string.IsNullOrWhiteSpace(ad))
                    {
                        Console.WriteLine($"ID: {id}, Text: {ad.Substring(0, 75)}...");

                        List<TaggedToken[]> list = new List<TaggedToken[]>();

                        SwedishTokenizer tokenizer = new SwedishTokenizer(new StringReader(ad));

                        List<Token> tokens;

                        int j = 0;

                        while ((tokens = tokenizer.ReadSentence()) != null)
                        {
                            TaggedToken[] sentence = tokens.Select((token, index) => new TaggedToken(token, $"{index}:{j}:{i}:{id}")).ToArray();

                            TaggedToken[] tagSentence = tagger.TagSentence(sentence, true, false);

                            list.Add(tagSentence);

                            j++;
                        }

                        tokenizer.Close();

                        Console.WriteLine($"Sentences: {list.Count}, Tokens: {list.Sum(taggedTokens => taggedTokens.Length)}");

                        try
                        {
                            Console.WriteLine("Writing the ad...");

                            serializer.Serialize(new Ad { Id = i, CategoryId = id.Value, Text = ad, TaggedData = list.ToArray()}, writer);

                            writer.Flush();

                            Console.WriteLine("Done.");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Error: {e}");
                        }
                    }

                    i++;

                    Console.WriteLine($"Ads done: {i}.");
                }
            }
        }
    }
}

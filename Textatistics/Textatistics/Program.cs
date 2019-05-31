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

            SUCTagger tagger = (SUCTagger)formatter.Deserialize(File.OpenRead(@"C:\Users\Rojan\Downloads\swedish.nmodel\swedish.nmodel"));

            using (Stream writer = File.OpenWrite(@"C:\Users\Rojan\Desktop\2006-2019.pos"))
            using (StreamReader reader = new StreamReader(File.OpenRead(@"C:\Users\Rojan\Desktop\2006-2019.json")))
            {
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

                        serializer.Serialize(new Ad { Id = i, CategoryId = id.Value, Text = ad, TaggedData = list.ToArray()}, writer);

                        writer.Flush();
                    }

                    i++;
                }
            }
        }
    }
}

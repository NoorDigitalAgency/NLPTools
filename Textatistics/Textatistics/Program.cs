using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using NStagger;

namespace Textatistics
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            Console.WriteLine("Loading the tagger model...");

            SUCTagger tagger = (SUCTagger)formatter.Deserialize(File.OpenRead(args[0]));

            Console.WriteLine("Model loaded.");

            Console.WriteLine("Counting the ads...");

            int count = 0;

            using (StreamReader reader = new StreamReader(new FileStream(args[1], FileMode.Open, FileAccess.Read)))
            {
                while (reader.ReadLine() != null)
                {
                    count++;
                }
            }

            Console.WriteLine($"{count} ads...");

            int cursorTop = Console.CursorTop;

            using (Stream writer = new FileStream(args[2], FileMode.Create, FileAccess.Write))
            using (StreamReader reader = new StreamReader(new FileStream(args[1], FileMode.Open, FileAccess.Read)))
            {
                int doneTotal = 0;

                int sentencesTotal = 0;

                int tokensTotal = 0;

                int i = 0;

                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    int indexOf = line.IndexOf(" ", StringComparison.Ordinal);

                    string id = line.Substring(0, indexOf);

                    string adText = line.Substring(indexOf + 1);

                    Ad ad = new Ad { Category = id, Sentences = new List<Sentence>() };

                    SwedishTokenizer tokenizer = new SwedishTokenizer(new StringReader(adText));

                    int j = 0;

                    try
                    {
                        List<NStagger.Token> tokens;

                        while ((tokens = tokenizer.ReadSentence()) != null)
                        {

                            TaggedToken[] sentence = tokens.Select((token, index) => new TaggedToken(token, $"{index}:{j}:{i}:{id}")).ToArray();

                            TaggedToken[] tagSentence = tagger.TagSentence(sentence, true, false);

                            ad.Sentences.Add(new Sentence
                            {
                                Offset = j,

                                Tokens = tagSentence.Select(token => new Token
                                {
                                    Id = token.Id,

                                    Lemma = token.Lemma,

                                    Value = token.LowerCaseText,

                                    NeTag = token.NeTag,

                                    NeTypeTag = token.NeTypeTag,

                                    PosTag = token.PosTag,

                                    IsCapitalized = token.Token.IsCapitalized,

                                    IsSpace = token.Token.IsSpace,

                                    Offset = token.Token.Offset,

                                    TokenType = (int)token.Token.Type

                                }).ToArray()
                            });

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
                        if (ad.Sentences.Any())
                        {
                            formatter.Serialize(writer, ad);

                            writer.Flush();

                            sentencesTotal += ad.Sentences.Count;

                            tokensTotal += ad.Sentences.Sum(sentence => sentence.Tokens.Length);

                            doneTotal++;
                        }
                    }
                    catch
                    {
                        //
                    }

                    i++;

                    Console.CursorTop = cursorTop;

                    Console.CursorLeft = 0;

                    Console.WriteLine($"Ads: {i}/{count}, Done: {doneTotal}, Passed: {i - doneTotal}, Sentences: {sentencesTotal}, Tokens: {tokensTotal}, Percentage: {i/(float)count*100:000.00}%");
                }
            }
        }
    }
}

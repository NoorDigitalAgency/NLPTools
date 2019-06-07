﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using Newtonsoft.Json.Linq;
using NStagger;

namespace Textatistics
{
    internal class Program
    {
        private static void TrainModel()
        {
            MLContext context = new MLContext(0);

            IDataView dataView = context.Data.LoadFromTextFile<LanguageSentence>(@"data.corpus");

            DataOperationsCatalog.TrainTestData data = context.Data.TrainTestSplit(dataView, 0.2D);

            EstimatorChain<KeyToValueMappingTransformer> pipeline = context.Transforms.Conversion.MapValueToKey("Label", nameof(LanguageSentence.Label))

                .Append(context.Transforms.Text.FeaturizeText("Features", nameof(LanguageSentence.Sentence)))

                .AppendCacheCheckpoint(context)

                .Append(context.MulticlassClassification.Trainers.SdcaMaximumEntropy())

                .Append(context.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            TransformerChain<KeyToValueMappingTransformer> model = pipeline.Fit(data.TrainSet);

            Console.WriteLine($"=============== Evaluating to get model's accuracy metrics - Starting time: {DateTime.Now.ToString()} ===============");

            MulticlassClassificationMetrics testMetrics = context.MulticlassClassification.Evaluate(model.Transform(data.TestSet));

            Console.WriteLine($"=============== Evaluating to get model's accuracy metrics - Ending time: {DateTime.Now.ToString(CultureInfo.InvariantCulture)} ===============");
            Console.WriteLine($"*************************************************************************************************************");
            Console.WriteLine($"*       Metrics for Multi-class Classification model - Test Data     ");
            Console.WriteLine($"*------------------------------------------------------------------------------------------------------------");
            Console.WriteLine($"*       MicroAccuracy:    {testMetrics.MicroAccuracy:0.###}");
            Console.WriteLine($"*       MacroAccuracy:    {testMetrics.MacroAccuracy:0.###}");
            Console.WriteLine($"*       LogLoss:          {testMetrics.LogLoss:#.###}");
            Console.WriteLine($"*       LogLossReduction: {testMetrics.LogLossReduction:#.###}");
            Console.WriteLine($"*************************************************************************************************************");

            context.Model.Save(model, data.TrainSet.Schema, @"language-detection.model");
        }

        public static void PredictIssue()
        {
            MLContext mlContext = new MLContext();

            ITransformer loadedModel = mlContext.Model.Load(@"language-detection.model", out _);

            PredictionEngine<LanguageSentence, LanguagePrediction> engine = mlContext.Model.CreatePredictionEngine<LanguageSentence, LanguagePrediction>(loadedModel);

            while (true)
            {
                string text = Console.ReadLine();

                LanguageSentence sentence = new LanguageSentence { Sentence = text };

                LanguagePrediction prediction = engine.Predict(sentence);

                float score = (float)Math.Round(prediction.Score.Max() * 100, 2);

                Console.WriteLine($"=============== Single Prediction - Result: {prediction.Label} {score} ===============");
            }
        }

        private static void Main(string[] args)
        {
            goto abbr;

        corpus:

            int linesCount = 1_250_000;

            EnglishTokenizer englishTokenizer = new EnglishTokenizer(new StringReader(Regex.Replace(Regex.Replace(File.ReadAllText(@"en.corpus"), "<[^>]*>", "", RegexOptions.Multiline), "\n+", "\n", RegexOptions.Multiline).Replace("p.m.", "pm")));

            List<string> lines = new List<string>();

            List<NStagger.Token> sen;

            while ((sen = englishTokenizer.ReadSentence()) != null && lines.Count < linesCount)
            {
                if (sen.Count >= 4)
                {
                    string line = $"en\t{string.Join(" ", sen.Where(token => token.Type == TokenType.Latin).Select(token => token.Value.ToLower())).Replace(" 's", "'s")}.";

                    if (line.Length > 10)
                    {
                        lines.Add(line);
                    }
                }
            }

            File.WriteAllLines(@"en-clean.corpus", lines);

            SwedishTokenizer swedishTokenizer = new SwedishTokenizer(new StringReader(Regex.Replace(Regex.Replace(File.ReadAllText(@"sv.corpus"), "<[^>]*>", "", RegexOptions.Multiline), "\n+", "\n", RegexOptions.Multiline)));

            lines = new List<string>();

            while ((sen = swedishTokenizer.ReadSentence()) != null && lines.Count < linesCount)
            {
                if (sen.Count >= 4)
                {
                    string line = $"sv\t{string.Join(" ", sen.Where(token => token.Type == TokenType.Latin).Select(token => token.Value.ToLower()))}.";

                    if (line.Length > 10)
                    {
                        lines.Add(line);
                    }
                }
            }

            File.WriteAllLines(@"sv-clean.corpus", lines);

            LatinTokenizer latinTokenizer = new LatinTokenizer(new StringReader(Regex.Replace(Regex.Replace(File.ReadAllText(@"da.corpus"), "<[^>]*>", "", RegexOptions.Multiline), "\n+", "\n", RegexOptions.Multiline)));

            lines = new List<string>();

            while ((sen = latinTokenizer.ReadSentence()) != null && lines.Count < linesCount)
            {
                if (sen.Count >= 4)
                {
                    string line = $"da\t{string.Join(" ", sen.Where(token => token.Type == TokenType.Latin).Select(token => token.Value.ToLower()))}.";

                    if (line.Length > 10)
                    {
                        lines.Add(line);
                    }
                }
            }

            File.WriteAllLines(@"da-clean.corpus", lines);

            latinTokenizer = new LatinTokenizer(new StringReader(Regex.Replace(Regex.Replace(File.ReadAllText(@"fi.corpus"), "<[^>]*>", "", RegexOptions.Multiline), "\n+", "\n", RegexOptions.Multiline)));

            lines = new List<string>();

            while ((sen = latinTokenizer.ReadSentence()) != null && lines.Count < linesCount)
            {
                if (sen.Count >= 4)
                {
                    string line = $"fi\t{string.Join(" ", sen.Where(token => token.Type == TokenType.Latin).Select(token => token.Value.ToLower()))}.";

                    if (line.Length > 10)
                    {
                        lines.Add(line);
                    }
                }
            }

            File.WriteAllLines(@"fi-clean.corpus", lines);

            string[] enLines = File.ReadAllLines(@"en-clean.corpus", Encoding.UTF8);

            Queue<string> trainEn = new Queue<string>(enLines.Take((int)(enLines.Length * (4f / 5))));

            Queue<string> testEn = new Queue<string>(enLines.Skip(trainEn.Count));

            string[] svLines = File.ReadAllLines(@"sv-clean.corpus", Encoding.UTF8);

            Queue<string> trainSv = new Queue<string>(svLines.Take((int)(svLines.Length * (4f / 5))));

            Queue<string> testSv = new Queue<string>(svLines.Skip(trainSv.Count));

            string[] daLines = File.ReadAllLines(@"da-clean.corpus", Encoding.UTF8);

            Queue<string> trainDa = new Queue<string>(daLines.Take((int)(daLines.Length * (4f / 5))));

            Queue<string> testDa = new Queue<string>(daLines.Skip(trainDa.Count));

            string[] fiLines = File.ReadAllLines(@"fi-clean.corpus", Encoding.UTF8);

            Queue<string> trainFi = new Queue<string>(fiLines.Take((int)(fiLines.Length * (4f / 5))));

            Queue<string> testFi = new Queue<string>(fiLines.Skip(trainFi.Count));

            using (StreamWriter streamWriter = new StreamWriter(File.OpenWrite(@"train.corpus")))
            {
                while (trainSv.Any() || trainEn.Any() || trainDa.Any() || trainFi.Any())
                {
                    if (trainSv.Any())
                    {
                        streamWriter.WriteLine(trainSv.Dequeue());
                    }

                    if (trainEn.Any())
                    {
                        streamWriter.WriteLine(trainEn.Dequeue());
                    }

                    if (trainDa.Any())
                    {
                        streamWriter.WriteLine(trainDa.Dequeue());
                    }

                    if (trainFi.Any())
                    {
                        streamWriter.WriteLine(trainFi.Dequeue());
                    }

                    streamWriter.Flush();
                }
            }

            using (StreamWriter streamWriter = new StreamWriter(File.OpenWrite(@"test.corpus")))
            {
                while (testSv.Any() || testEn.Any() || testDa.Any() || testFi.Any())
                {
                    if (testSv.Any())
                    {
                        streamWriter.WriteLine(testSv.Dequeue());
                    }

                    if (testEn.Any())
                    {
                        streamWriter.WriteLine(testEn.Dequeue());
                    }

                    if (testDa.Any())
                    {
                        streamWriter.WriteLine(testDa.Dequeue());
                    }

                    if (testFi.Any())
                    {
                        streamWriter.WriteLine(testFi.Dequeue());
                    }

                    streamWriter.Flush();
                }
            }

        train:

            TrainModel();

            return;

        predict:

            PredictIssue();

            return;

        abbr:

            Console.Clear();

            int totalLines = 0;

            /*using (StreamReader reader = new StreamReader(new FileStream(@"C:\Users\Rojan\Desktop\pb2006_2017\2006-2019-swe.json", FileMode.Open, FileAccess.Read)))
            {
                Console.WriteLine("Counting the lines...");

                while (reader.ReadLine() != null)
                {
                    totalLines++;

                    if (totalLines % 5000 == 0)
                    {
                        Console.CursorLeft = 0;

                        Console.Write($"Lines: {totalLines}        ");
                    }
                }
            }*/

            Console.CursorLeft = 0;

            Console.Write($"Lines: {totalLines}        ");

            List<string> ex = new List<string>();

            List<string> li = new List<string>();

            string[] abbr = { @"\bagr\.", @"\bapr\.", @"\bbl\.a\.", @"\bd\.y\.", @"\bd\.ä\.", @"\be\.Kr\.", @"\be\.o\.", @"\bev\.", @"\bfeb\.", @"\bfm\.", @"\bforts\.", @"\bfr\.o\.m\.", @"\bf\.ö\.", @"\bst\.f\.", @"\bjun\.", @"\bkand\.", @"\blic\.", @"\blör\.", @"\bmag\.", @"\bm\.fl\.", @"\bm\.m\.", @"\bmom\.", @"\bmån\.", @"\bodont\.", @"\bons\.", @"\bpar\.", @"\bp\.g\.a\.", @"\bpl\.", @"\bpol\.", @"\bsep\.", @"\bs\.k\.", @"\bst\.", @"\bstud\.", @"\btekn\.", @"\bteol\.", @"\btis\.", @"\bt\.o\.m\.", @"\btr\.", @"\bu\.a\.", @"\buppl\.", @"\bv\.g\.v\.", @"\badr\.", @"\baug\.", @"\bdec\.", @"\bdvs\.", @"\be\.dyl\.", @"\bekon\.", @"\bem\.", @"\betc\.", @"\bfarm\.", @"\bf\.d\.", @"\bf\.n\.", @"\bfre\.", @"\bf\.v\.b\.", @"\bjan\.", @"\bjul\.", @"\bjur\.", @"\bkap\.", @"\bkl\.", @"\bkr\.", @"\bL\.", @"\bleg\.", @"\bmar\.", @"\bmed\.", @"\bmilj\.", @"\bmin\.", @"\bmån\.", @"\bn\.b\.", @"\bnov\.", @"\bo\.d\.", @"\bokt\.", @"\bosv\.", @"\bpl\.", @"\bresp\.", @"\bsek\.", @"\bsid\.", @"\bSt\.", @"\bsön\.", @"\btel\.", @"\bt\.ex\.", @"\btim\.", @"\btor\.", @"\bt\.v\.", @"\bu\.p\.a\.", @"\bvard\.", @"\bv\.g\.", @"\bäv\.", @"\bdr\.", @"\bd\.y\.", @"\bd\.ä\.", @"\be\.d\.", @"\beg\.", @"\bf\.d\.", @"\bfol\.", @"\bg\.b\.", @"\bg\.m\.", @"\bh\.a\.", @"\bhem\.äg\.", @"\bhem\.eg\.", @"\bhusm\.", @"\blysn\.", @"\bn\.b\.", @"\bn:m:", @"\bL:a", @"\bo\.d\.", @"\bo\.ä\.d\.", @"\bo\.s\.", @"\bo\.ä\.s\.", @"\bo\.ä\.", @"\bsal\.", @"\bskärs\.", @"\bsusc\.", @"\bu\.", @"\bu\.m\.", @"\bu\.ä\.", @"\by\.", @"\bä\.", @"\bobs\.", @"\bd\.v\.s\.", @"\bm\.a\.o\.", @"\bel\.", @"\bt\.ex\.", @"\bfig\.", @"\bev\.", @"\bo\.dyl\.", @"\bv\.", @"\bs\.k\.", @"\bm\.m\.", @"\bm\.fl\.", @"\bosv\.", @"\betc\.", @"\bf\.ö\.", @"\bang\.", @"\bung\.", @"\bf\.Kr\.", @"\be\.Kr\.", @"\bkl\.", @"\bforts\.", @"\bbl\.a\.", @"\bt\.o\.m\.", @"\bfr\.o\.m\.", @"\bs\." };

            abbr = abbr.Distinct().ToArray();

            Dictionary<string, Regex> dictionary = abbr.ToDictionary(s => s, s => new Regex(s));

            HashSet<string> remaining = new HashSet<string>(abbr);

            using (StreamReader reader = new StreamReader(new FileStream(@"C:\Users\Rojan\Desktop\pb2006_2017\2006-2019-swe.json", FileMode.Open, FileAccess.Read)))
            {
                int lineNumber = 0;

                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    line = string.Join("\n", line.Substring(line.IndexOf(" ", StringComparison.Ordinal) + 1).ToLines(true));

                    lineNumber++;

                    if (lineNumber % 5000 == 0)
                    {
                        Console.CursorLeft = 0;

                        Console.Write($"Line #{lineNumber}/{totalLines} ({lineNumber / (float)totalLines * 100:##0.00}%), testing ({remaining.Count:000})       ");
                    }

                    foreach (string abb in remaining)
                    {
                        string format = abb.Replace("\\b", "").Replace("\\", "");

                        Regex r = dictionary[abb];

                        if (r.IsMatch(line))
                        {
                            Console.CursorLeft = 0;

                            Console.Write($"Line #{lineNumber}/{totalLines} ({lineNumber / (float)totalLines * 100:##0.00}%), testing ({remaining.Count:000})       ");

                            ConsoleColor color = Console.ForegroundColor;

                            Console.ForegroundColor = ConsoleColor.DarkGreen;

                            Console.WriteLine(format);

                            Console.ForegroundColor = color;

                            //line = r.Replace(line, match => match.Value.Code());

                            li.Add(line);

                            ex.Add(abb);

                            remaining.Remove(abb);

                            break;
                        }
                    }

                    if (!remaining.Any())
                    {
                        break;
                    }
                }
            }

            Console.WriteLine();

            Console.WriteLine("Writing out the output file...");

            string text = "";

            List<string> supported = new List<string>();

            List<string> unsupported = new List<string>();

            for (int i = 0; i < li.Count; i++)
            {
                string line = li[i];

                string abb = ex[i];

                string title = $"{abb.Replace("\\b", "").Replace("\\s", "").Replace("\\", "")}";

                string org = abb;

                abb = abb.Replace(@"\.", @"(?:\s|\r\n)?\.(?:\s|\r\n)?");

                abb = abb.Contains('.') ? abb.Substring(0, abb.Length - 12) : abb;

                swedishTokenizer = new SwedishTokenizer(new StringReader(line));

                List<NStagger.Token> se = new List<NStagger.Token>();

                string part = "";

                while ((se = swedishTokenizer.ReadSentence()) != null)
                {
                    string format = string.Join(" ", se.Select(token => token.Value));

                    part += $"{format}\r\n";
                }

                bool isMatch = Regex.IsMatch(part, org);

                if (isMatch)
                {
                    supported.Add(title);
                }
                else
                {
                    unsupported.Add(title);
                }

                string style = isMatch ? "green" : "red";

                string pattern = $"({abb})";

                string replacement = $"<span style=\"background-color:{style};color:white;\">$1</span>";

                Regex tag = new Regex(pattern);

                string html = tag.Replace(part, replacement);

                text += $"<h2>{title}</h2><p>{html.Replace("\r\n", "<br />")}</p>";
            }

            File.WriteAllText("out.html", $"<html><body>{text}</body></html>");

            File.WriteAllLines("supported.txt", supported);

            File.WriteAllLines("unsupported.txt", unsupported);

            File.WriteAllLines("not-found.txt", abbr.Except(ex));

            Console.WriteLine("Done!");

            return;

        tokenization:

            Regex regex = new Regex(@"\b(?:\d{1,4}|\w{1,3}) .?$");

            using (StreamWriter writer = new StreamWriter(new FileStream(args[1], FileMode.Create, FileAccess.Write)))
            using (StreamReader reader = new StreamReader(new FileStream(args[0], FileMode.Open, FileAccess.Read)))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Substring(line.IndexOf(" ", StringComparison.Ordinal) + 1);

                    line = line

                        .Replace("m.m.", "mm ")

                        .Replace("ref. nr.", "referencenummer ")

                        .Replace("ref.nr.", "referencenummer ")

                        .Replace("nr.", "nummer ")

                        .Replace("ref.", "reference ")

                        .Replace("t.v.", "tv ")

                        .Replace("kl.", "klockan ");

                    line = Regex.Replace(line, @"(\b\d{2,4})\.(\d{1,2})\.(\d{1,2})\b", "$1-$2-$3");

                    swedishTokenizer = new SwedishTokenizer(new StringReader(line));

                    List<NStagger.Token> firstSentence;

                    List<NStagger.Token> secondSentence;

                    while ((firstSentence = swedishTokenizer.ReadSentence()) != null && (secondSentence = swedishTokenizer.ReadSentence()) != null)
                    {
                        string first = string.Join(" ", firstSentence.Select(token => token.Value));

                        string second = string.Join(" ", secondSentence.Select(token => token.Value));

                        if (regex.IsMatch(first) || regex.IsMatch(second))
                        {
                            writer.WriteLine();

                            writer.WriteLine(first);

                            writer.WriteLine(second);

                            writer.WriteLine();
                        }
                    }
                }
            }

            return;

        pos:

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

                    Console.WriteLine($"Ads: {i}/{count}, Done: {doneTotal}, Passed: {i - doneTotal}, Sentences: {sentencesTotal}, Tokens: {tokensTotal}, Percentage: {i / (float)count * 100:000.00}%");
                }
            }

        done: return;
        }
    }
}
